﻿#define MultiThreadSupport

using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Threading.Tasks;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using Color = SharpDX.Color;
using Matrix = SharpDX.Matrix;

namespace CopperCowEngine.Rendering.D3D11.Editor.Loaders
{
    /// <summary>
    /// Class for converting Spherical Environment Map from .hdr format
    /// to TextureCubeAsset and calculate PreFiltered and Irradiance maps from it.
    /// </summary>
    internal class IblMapsPreRender
    {
        private struct CubeFaceCamera
        {
            public Matrix View;
            public Matrix Projection;
        }
        
        private struct BrdfParamsBufferStruct
        {
            // ReSharper disable once NotAccessedField.Local
            public float Roughness;
            // ReSharper disable once NotAccessedField.Local
            public Vector3 Filler;
        }

        private Buffer _constantsBuffer;
        private Buffer _brdfParamsBuffer;

        private VertexShader _sphereToCubeMapVs;
        private VertexShader _integrateQuadVs;
        private PixelShader _sphereToCubeMapPs;
        private PixelShader _irradiancePs;
        private PixelShader _preFilteredPs;
        private PixelShader _integrateBrdFxPs;
        private InputLayout _customInputLayout;
        private SamplerState _sampler;

        private Texture2D _inputMap;
        private Texture2D _convertedCubeMap;
        private Texture2D _irradianceCubeMap;
        private Texture2D _preFilteredCubeMap;
        private Texture2D _integrateBrdFxMap;

        private ShaderResourceView _inputSrv;
        private ShaderResourceView _convertedCubeSrv;

        private DeviceContext[] _contextList;
        private RenderTargetView[] _outputRtVs = new RenderTargetView[6];
        private RenderTargetView _integrateBrdFxRtv;
        private readonly CubeFaceCamera[] _cameras = new CubeFaceCamera[6];
        private ViewportF _viewport;

        private Device _device;
        private DeviceContext GetContext => _device?.ImmediateContext;
        
        private const int IrradianceSize = 32;
        private const int PreFilteredMipsCount = 9;
        private const int BrdFxMapSize = 256;

        private const int Threads = 2;
        private int _outputResolution;
        private int _outputSize;

        private enum RenderState : byte
        {
            CubeMap,
            IrradianceMap,
            PreFilteredMap,
            IntegrateBrdf,
        }

        private RenderState _currentState;

        public void Init(string sourcePath, int resolution)
        {
            if (!InitDevice())
            {
                return;
            }
            _outputSize = resolution;

            InputMapInit(sourcePath);

            var outputTextureDescription = OutputMapInit();

            IrradianceAndPreFilteredMapsInit(outputTextureDescription);

            SetupShadersAndBuffers();
        }

        private bool InitDevice()
        {
            var driverTypes = new[] {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference,
            };

            const DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;
            var levels = new[] {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
            };

            foreach (var driverType in driverTypes)
            {
                _device = new Device(driverType, deviceCreationFlags, levels);
                if (_device == null)
                {
                    continue;
                }
                _device.DebugName = "The Device";
                break;
            }

            if (_device == null)
            {
                Console.WriteLine("Device not created!");
                return false;
            }

            // Create context list
#if !MultiThreadSupport
            _contextList = null;
#else
            _contextList = new DeviceContext[Threads];
            for (var i = 0; i < Threads; i++)
            {
                _contextList[i] = ToDispose(new DeviceContext(_device));
                _contextList[i].DebugName = $"Context#{i}";
            }
#endif
            //Console.WriteLine($"Device was created! DriverType: {CurrentDriverType.ToString()}");
            return true;
        }

        private void InputMapInit(string sourcePath)
        {
            var initData = GetHdrTextureData(sourcePath, out var inputWidth, out var inputHeight);
            var inputTextureDesc = new Texture2DDescription()
            {
                Format = Format.R32G32B32A32_Float,
                Width = inputWidth,
                Height = inputHeight,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };

            _inputMap = ToDispose(new Texture2D(_device, inputTextureDesc, initData));
            _inputMap.DebugName = "InputMap";
            //_inputMap.Save(GetContext, _device, "TestInputHdr");

            var descSrv = new ShaderResourceViewDescription
            {
                Format = inputTextureDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = { MipLevels = inputTextureDesc.MipLevels, MostDetailedMip = 0 },
            };

            _inputSrv = ToDispose(new ShaderResourceView(_device, _inputMap, descSrv));
            _inputSrv.DebugName = "InputSRV";
        }

        private Texture2DDescription OutputMapInit()
        {
            var outputTextureDescription = new Texture2DDescription()
            {
                Width = _outputSize,
                Height = _outputSize,
                ArraySize = 6,
                MipLevels = 0,
                Format = Format.R16G16B16A16_Float,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            };
            _convertedCubeMap = ToDispose(new Texture2D(_device, outputTextureDescription));
            _convertedCubeMap.DebugName = "ConvertedCubeMap";
            outputTextureDescription.OptionFlags = ResourceOptionFlags.TextureCube;

            var descSrv = new ShaderResourceViewDescription
            {
                Format = outputTextureDescription.Format,
                Dimension = ShaderResourceViewDimension.TextureCube,
                TextureCube = { MipLevels = -1, MostDetailedMip = 0 },
            };
            _convertedCubeSrv = ToDispose(new ShaderResourceView(_device, _convertedCubeMap, descSrv));
            _convertedCubeSrv.DebugName = "ConvertedCubeSRV";

            return outputTextureDescription;
        }

        private void IrradianceAndPreFilteredMapsInit(Texture2DDescription outputTextureDescription)
        {
            // Irradiance
            outputTextureDescription.Width = IrradianceSize;
            outputTextureDescription.Height = IrradianceSize;
            outputTextureDescription.MipLevels = 1;
            _irradianceCubeMap = ToDispose(new Texture2D(_device, outputTextureDescription));
            _irradianceCubeMap.DebugName = "IrradianceCubeMap";

            // PreFiltered
            outputTextureDescription.Width = _outputSize;
            outputTextureDescription.Height = _outputSize;
            outputTextureDescription.MipLevels = PreFilteredMipsCount;
            _preFilteredCubeMap = ToDispose(new Texture2D(_device, outputTextureDescription));
            _preFilteredCubeMap.DebugName = "PreFilteredCubeMap";
        }

        private static DataBox[] GetHdrTextureData(string sourcePath, out int inputWidth, out int inputHeight)
        {
            var imageFloats = TextureImporter.LoadHdrTexture(sourcePath, out inputWidth, out inputHeight, out var pixelSize);
            
            if (inputWidth == 0)
            {
                Console.WriteLine("Texture not loaded!");
                return null;
            }

            var n = imageFloats.Length + inputWidth * inputHeight;
            var imageFloatsCorrected = new float[n];
            var j = 0;
            for (var i = 0; i < imageFloats.Length; i+= 3)
            {
                imageFloatsCorrected[j] = imageFloats[i];
                imageFloatsCorrected[j + 1] = imageFloats[i + 1];
                imageFloatsCorrected[j + 2] = imageFloats[i + 2];
                imageFloatsCorrected[j + 3] = 1f;
                j += 4;
            }

            var pSrcBits = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(imageFloatsCorrected, 0);
            var initData = new DataBox[1];
            initData[0].DataPointer = pSrcBits;
            initData[0].RowPitch = inputWidth * (pixelSize + 4);

            return initData;
        }

        private void SetupShadersAndBuffers()
        {
            var assetsManager = AssetsManager.GetManager();
            var meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_SphereToCubeMapPS");
            var shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _sphereToCubeMapPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _sphereToCubeMapPs.DebugName = "SphereToCubeMapPS";

            meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_IrradiancePS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _irradiancePs = ToDispose(new PixelShader(_device, shaderBytecode));
            _irradiancePs.DebugName = "IrradiancePS";

            meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_PreFilteredPS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _preFilteredPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _preFilteredPs.DebugName = "PreFilteredPS";

            meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_SphereToCubeMapVS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _sphereToCubeMapVs = ToDispose(new VertexShader(_device, shaderBytecode));
            _sphereToCubeMapVs.DebugName = "SphereToCubeMapVS";

            _customInputLayout = ToDispose(new InputLayout(_device,
                ShaderSignature.GetInputSignature(shaderBytecode), new[] {
                new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
            }));

            // Create the per environment map buffer ViewProjection matrices
            _constantsBuffer = ToDispose(new Buffer(
                _device,
                Utilities.SizeOf<Matrix>() * 2,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0)
            );
            _constantsBuffer.DebugName = "ConstantsBuffer";

            _brdfParamsBuffer = ToDispose(new Buffer(
                _device,
                Utilities.SizeOf<BrdfParamsBufferStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0)
            );
            _brdfParamsBuffer.DebugName = "BRDFParamsBuffer";

            _sampler = ToDispose(new SamplerState(_device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear, // Trilinear
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunction = Comparison.Never,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            }));
            _sampler.DebugName = "DefaultTrilinearSampler";
        }

        private void SetViewPoint(Vector3 camera)
        {
            // The LookAt targets for view matrices
            var targets = new[] {
                camera + Vector3.UnitX, // +X
                camera - Vector3.UnitX, // -X
                camera + Vector3.UnitY, // +Y
                camera - Vector3.UnitY, // -Y
                camera + Vector3.UnitZ, // +Z
                camera - Vector3.UnitZ  // -Z
            };

            // The "up" vector for view matrices
            var upVectors = new[] {
                Vector3.UnitY, // +X
                Vector3.UnitY, // -X
                -Vector3.UnitZ,// +Y
                Vector3.UnitZ,// -Y
                Vector3.UnitY, // +Z
                Vector3.UnitY, // -Z
            };

            // Create view and projection matrix for each face
            for (var i = 0; i < 6; i++)
            {
                _cameras[i].View = Matrix.LookAtLH(camera, targets[i], upVectors[i]);
                _cameras[i].Projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1.0f, 0.1f, 100.0f);
            }
        }

        public void Render(string path)
        {
            if (GetContext == null)
            {
                return;
            }

            _currentState = RenderState.CubeMap;
            SetViewPoint(Vector3.Zero);
            _outputResolution = _outputSize;
            _viewport = new Viewport(0, 0, _outputResolution, _outputResolution);
            CreateRenderTargetsFromMap(_convertedCubeMap);
            UpdateThreaded(path);

            _currentState = RenderState.IrradianceMap;
            _viewport = new Viewport(0, 0, IrradianceSize, IrradianceSize);
            _outputResolution = IrradianceSize;
            CreateRenderTargetsFromMap(_irradianceCubeMap);
            UpdateThreaded(path);

            _currentState = RenderState.PreFilteredMap;
            _outputResolution = _outputSize;
            _viewport = new Viewport(0, 0, _outputResolution, _outputResolution);
            CreateRenderTargetsFromMap(_preFilteredCubeMap);
            UpdateThreaded(path);
        }

        private void CreateRenderTargetsFromMap(Texture2D map)
        {
            var renderTargetViewDescription = new RenderTargetViewDescription
            {
                Format = map.Description.Format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
                Texture2DArray = { ArraySize = 1 },
            };

            foreach (var renderTarget in _outputRtVs)
            {
                renderTarget?.Dispose();
            }

            if (_currentState == RenderState.PreFilteredMap)
            {
                _outputRtVs = new RenderTargetView[6 * PreFilteredMipsCount];
                for (var j = 0; j < 6; j++)
                {
                    for (var i = 0; i < PreFilteredMipsCount; i++)
                    {
                        renderTargetViewDescription.Texture2DArray.MipSlice = i;
                        renderTargetViewDescription.Texture2DArray.FirstArraySlice = j;
                        _outputRtVs[j * PreFilteredMipsCount + i] = ToDispose(new RenderTargetView(_device, map, renderTargetViewDescription));
                        _outputRtVs[j * PreFilteredMipsCount + i].DebugName = $"CubeRTVFace{j}Mip{i}";
                    }
                }
                return;
            }

            _outputRtVs = new RenderTargetView[6];

            renderTargetViewDescription.Texture2DArray.MipSlice = 0;
            for (var i = 0; i < _outputRtVs.Length; i++)
            {
                renderTargetViewDescription.Texture2DArray.FirstArraySlice = i;
                _outputRtVs[i]?.Dispose();
                _outputRtVs[i] = ToDispose(new RenderTargetView(_device, map, renderTargetViewDescription));
                _outputRtVs[i].DebugName = $"CubeRTVFace{i}";
            }
        }

        public void RenderBrdf(string outputName)
        {
            _currentState = RenderState.IntegrateBrdf;

            if (GetContext == null)
            {
                if (!InitDevice())
                {
                    return;
                }
            }
            SetViewPoint(Vector3.Zero);
            InitBrdFxResources();
            _viewport = new Viewport(0, 0, BrdFxMapSize, BrdFxMapSize);
            UpdateCubeFace(GetContext, 0);
            _integrateBrdFxMap.Save(GetContext, _device, outputName);
        }

        private void InitBrdFxResources()
        {
            var assetsManager = AssetsManager.GetManager();
            //var meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapPS");

            var meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_IntegrateBRDFxPS");
            var shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _integrateBrdFxPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _integrateBrdFxPs.DebugName = "IntegrateBRDFxPS";

            meta = assetsManager.LoadAsset<ShaderAsset>("IBL_PR_IntegrateQuadVS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _integrateQuadVs = ToDispose(new VertexShader(_device, shaderBytecode));
            _integrateQuadVs.DebugName = "IntegrateQuadVS";

            if (_customInputLayout == null)
            {
                _customInputLayout = ToDispose(new InputLayout(_device,
                    ShaderSignature.GetInputSignature(shaderBytecode), new[] {
                    new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
                }));
            }

            if (_constantsBuffer == null)
            {
                // Create the per environment map buffer ViewProjection matrices
                _constantsBuffer = ToDispose(new Buffer(
                    _device,
                    Utilities.SizeOf<Matrix>() * 2,
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None, 0)
                );
                _constantsBuffer.DebugName = "ConstantsBuffer";
            }

            _integrateBrdFxMap = ToDispose(new Texture2D(_device, new Texture2DDescription()
            {
                Width = BrdFxMapSize,
                Height = BrdFxMapSize,
                Format = Format.R16G16_Float,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.RenderTarget,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
            }));

            _integrateBrdFxRtv = ToDispose(new RenderTargetView(_device, _integrateBrdFxMap, new RenderTargetViewDescription()
            {
                Dimension = RenderTargetViewDimension.Texture2D,
                Format = _integrateBrdFxMap.Description.Format,
                Texture2D = new RenderTargetViewDescription.Texture2DResource()
                {
                    MipSlice = 0,
                }
            }));
        }

        private void UpdateThreaded(string path)
        {
            var contexts = _contextList ?? new[] { GetContext };
            var commands = new CommandList[contexts.Length];
            var batchSize = 6 / contexts.Length;

            var tasks = new Task[contexts.Length];

            for (var i = 0; i < contexts.Length; i++)
            {
                var contextIndex = i;

                tasks[i] = Task.Run(() => {
                    var context = contexts[contextIndex];

                    var startIndex = batchSize * contextIndex;
                    var endIndex = Math.Min(startIndex + batchSize, 5);
                    if (contextIndex == contexts.Length - 1)
                    {
                        endIndex = 5;
                    }

                    if (_currentState == RenderState.PreFilteredMap)
                    {
                        UpdatePreFilteredFaces(context, startIndex, endIndex);
                    }
                    else
                    {
                        for (var j = startIndex; j <= endIndex; j++)
                        {
                            UpdateCubeFace(context, j);
                        }
                    }

                    if (context.TypeInfo == DeviceContextType.Deferred)
                    {
                        commands[contextIndex] = ToDispose(context.FinishCommandList(false));
                    }
                });
            }
            Task.WaitAll(tasks);

            // Execute command lists (if any)
            for (var i = 0; i < contexts.Length; i++)
            {
                if (contexts[i].TypeInfo != DeviceContextType.Deferred || commands[i] == null)
                {
                    continue;
                }
                GetContext.ExecuteCommandList(commands[i], false);
                commands[i].Dispose();
                commands[i] = null;
            }

            switch (_currentState)
            {
                case RenderState.CubeMap:
                    GetContext.GenerateMips(_convertedCubeSrv);
                    //_convertedCubeMap.Save(GetContext, _device, $"{path}EnvMap");
                    break;
                case RenderState.IrradianceMap:
                    _irradianceCubeMap.Save(GetContext, _device, $"{path}IrradianceEnvMap");
                    break;
                case RenderState.PreFilteredMap:
                    _preFilteredCubeMap.Save(GetContext, _device, $"{path}PreFilteredEnvMap", true);
                    break;
                case RenderState.IntegrateBrdf:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateCubeFace(DeviceContext context, int index, int mip = -1)
        {
            // Prepare pipeline
            context.ClearState();

            context.Rasterizer.SetViewport(_viewport);
            context.InputAssembler.InputLayout = _customInputLayout;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            if (_currentState == RenderState.IntegrateBrdf)
            {
                context.OutputMerger.SetRenderTargets(null, _integrateBrdFxRtv);
                context.ClearRenderTargetView(_integrateBrdFxRtv, Color.CornflowerBlue);
                context.VertexShader.Set(_integrateQuadVs);
                context.PixelShader.Set(_integrateBrdFxPs);
                context.Draw(4, 0);
                UnbindContextRtv(context);
                return;
            }

            var rtvIndex = mip < 0 ? index : (index * PreFilteredMipsCount + mip);
            context.OutputMerger.SetRenderTargets(null, _outputRtVs[rtvIndex]);
            // Render the scene using the view, projection, RTV and DSV of this cube face
            context.ClearRenderTargetView(_outputRtVs[rtvIndex], Color.CornflowerBlue);

            var viewProj = new[]
            {
                _cameras[index].View,
                _cameras[index].Projection,
            };

            context.UpdateSubresource(viewProj, _constantsBuffer);
            context.VertexShader.Set(_sphereToCubeMapVs);
            context.VertexShader.SetConstantBuffer(0, _constantsBuffer);
            context.PixelShader.SetConstantBuffer(1, _brdfParamsBuffer);
            context.PixelShader.SetSampler(0, _sampler);

            switch (_currentState)
            {
                case RenderState.CubeMap:
                    context.PixelShader.Set(_sphereToCubeMapPs);
                    context.PixelShader.SetShaderResource(0, _inputSrv);
                    break;
                case RenderState.IrradianceMap:
                    context.PixelShader.Set(_irradiancePs);
                    context.PixelShader.SetShaderResource(1, _convertedCubeSrv);
                    break;
                case RenderState.PreFilteredMap:
                    context.PixelShader.Set(_preFilteredPs);
                    context.PixelShader.SetShaderResource(1, _convertedCubeSrv);
                    break;
                case RenderState.IntegrateBrdf:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            context.Draw(14, 0);
            UnbindContextRtv(context);
        }

        private void UpdatePreFilteredFaces(DeviceContext context, int startIndex, int endIndex)
        {
            for (var mip = 0; mip < PreFilteredMipsCount; mip++)
            {
                var mipSize = (int)(_outputSize * Math.Pow(0.5, mip));
                var roughness = mip / (float)(PreFilteredMipsCount - 1);
                var cbParams = new BrdfParamsBufferStruct()
                {
                    Roughness = roughness,
                    Filler = Vector3.Zero,
                };
                context.UpdateSubresource(ref cbParams, _brdfParamsBuffer);
                for (var j = startIndex; j <= endIndex; j++)
                {
                    _viewport = new Viewport(0, 0, mipSize, mipSize);
                    UpdateCubeFace(context, j, mip);
                }
            }
        }

        private static void UnbindContextRtv(DeviceContext context)
        {
            // Unbind the RTV and DSV
            context.VertexShader.Set(null);
            context.VertexShader.SetConstantBuffer(0, null);
            context.PixelShader.Set(null);
            context.PixelShader.SetConstantBuffer(0, null);
            context.PixelShader.SetShaderResources(0, new ShaderResourceView[2]);
            context.PixelShader.SetSampler(0, null);
            context.OutputMerger.ResetTargets();
        }

        #region Dispose section

        private List<IDisposable> _toDisposeList = new List<IDisposable>();

        private T ToDispose<T>(T obj) where T : IDisposable
        {
            _toDisposeList.Add(obj);
            return obj;
        }

        public void Dispose()
        {
            _toDisposeList.Reverse();
            foreach (var item in _toDisposeList)
            {
                item?.Dispose();
            }
            _toDisposeList.Clear();
            _toDisposeList = null;

            if (_contextList != null)
            {
                foreach (var item in _contextList)
                {
                    item?.Dispose();
                }
                _contextList = null;
            }

            GetContext.ClearState();
            GetContext.Flush();
            GetContext.Dispose();
            _device?.Dispose();
            SaveToWicImage.DisposeFactory();
        }
        #endregion
    }
}
