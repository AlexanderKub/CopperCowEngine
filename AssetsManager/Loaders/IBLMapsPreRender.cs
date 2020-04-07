using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

namespace AssetsManager.Loaders
{
    /// <summary>
    /// Class for converting Spherical Environment Map from .hdr format
    /// to TextureCubeAsset and calculate PreFiltered and Irradiance maps from it.
    /// </summary>
    internal class IBLMapsPreRender
    {
        // TODO: D3D11 specific code
        private struct CubeFaceCamera
        {
            public Matrix View;
            public Matrix Projection;
        }

        private struct BRDFParamsBufferStruct
        {
            public float Roughness;
            private Vector3 filler;
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

        public DeviceContext GetContext => _device?.ImmediateContext;

        private const int OutputMapSize = 512;
        private const int IrradianceSize = 32;
        private const int PreFilteredSize = 512;
        private const int PreFilteredMipsCount = 6;
        private const int BrdFxMapSize = 512;

        private const int Threads = 2;
        private int _outputResolution = OutputMapSize;

        private enum RenderState
        {
            CubeMap,
            IrradianceMap,
            PreFilteredMap,
            IntegrateBrdf,
        }

        private RenderState _currentState;

        public void Init(string sourcePath)
        {
            if (!InitDevice())
            {
                return;
            }

            #region InputMap
            var initData = GetHDRTextureData(sourcePath, out var inputWidth, out var inputHeight);
            var inputTextureDesc = new Texture2DDescription()
            {
                Format = Format.R32G32B32_Float,
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

            var descSrv = new ShaderResourceViewDescription
            {
                Format = inputTextureDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = {MipLevels = inputTextureDesc.MipLevels, MostDetailedMip = 0},
            };
            _inputSrv = ToDispose(new ShaderResourceView(_device, _inputMap, descSrv));
            _inputSrv.DebugName = "InputSRV";
            #endregion

            #region OutputMap
            var outputTextureDescription = new Texture2DDescription()
            {
                Width = _outputResolution,
                Height = _outputResolution,
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

            descSrv = new ShaderResourceViewDescription
            {
                Format = outputTextureDescription.Format,
                Dimension = ShaderResourceViewDimension.TextureCube,
                TextureCube = {MipLevels = -1, MostDetailedMip = 0},
            };
            _convertedCubeSrv = ToDispose(new ShaderResourceView(_device, _convertedCubeMap, descSrv));
            _convertedCubeSrv.DebugName = "ConvertedCubeSRV";
            #endregion

            #region IrradianceMap
            outputTextureDescription.Width = IrradianceSize;
            outputTextureDescription.Height = IrradianceSize;
            outputTextureDescription.MipLevels = 1;
            _irradianceCubeMap = ToDispose(new Texture2D(_device, outputTextureDescription));
            _irradianceCubeMap.DebugName = "IrradianceCubeMap";
            #endregion

            #region PreFilteredMap
            outputTextureDescription.Width = PreFilteredSize;
            outputTextureDescription.Height = PreFilteredSize;
            outputTextureDescription.MipLevels = PreFilteredMipsCount;
            _preFilteredCubeMap = ToDispose(new Texture2D(_device, outputTextureDescription));
            _preFilteredCubeMap.DebugName = "PreFilteredCubeMap";
            #endregion

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
            if (Threads == 1)
            {
                _contextList = null;
            }
            else
            {
                _contextList = new DeviceContext[Threads];
                for (var i = 0; i < Threads; i++)
                {
                    _contextList[i] = ToDispose(new DeviceContext(_device));
                    _contextList[i].DebugName = $"Context#{i}";
                }
            }
            //Console.WriteLine($"Device was created! DriverType: {CurrentDriverType.ToString()}");
            return true;
        }

        private void SetupShadersAndBuffers()
        {
            var assetsManager = AssetsManagerInstance.GetManager();
            var meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapPS");
            var shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _sphereToCubeMapPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _sphereToCubeMapPs.DebugName = "SphereToCubeMapPS";

            meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IrradiancePS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _irradiancePs = ToDispose(new PixelShader(_device, shaderBytecode));
            _irradiancePs.DebugName = "IrradiancePS";

            meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_PreFilteredPS");
            shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _preFilteredPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _preFilteredPs.DebugName = "PreFilteredPS";

            meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapVS");
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
                Utilities.SizeOf<BRDFParamsBufferStruct>(),
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

        void SetViewPoint(Vector3 camera)
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
                +Vector3.UnitZ,// -Y
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

        private void CreateRenderTargetsFromMap(Texture2D map, bool withMips)
        {
            var renderTargetViewDescription = new RenderTargetViewDescription
            {
                Format = map.Description.Format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
                Texture2DArray = {ArraySize = 1},
            };

            foreach (var renderTarget in _outputRtVs)
            {
                renderTarget?.Dispose();
            }

            if (withMips)
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

        private static DataBox[] GetHDRTextureData(string sourcePath, out int inputWidth, out int inputHeight)
        {
            var imageFloats = TextureLoader.LoadHdrTexture(sourcePath, out inputWidth, out inputHeight, out var pixelSize);
            if (inputWidth == 0)
            {
                Console.WriteLine("Texture not loaded!");
                return null;
            }

            var pSrcBits = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(imageFloats, 0);
            var initData = new DataBox[1];
            initData[0].DataPointer = pSrcBits;
            initData[0].RowPitch = inputWidth * pixelSize;
            initData[0].SlicePitch = inputWidth * inputHeight * pixelSize;
            return initData;
        }

        public void Render(string path)
        {
            if (GetContext == null)
            {
                return;
            }

            _currentState = RenderState.CubeMap;
            SetViewPoint(Vector3.Zero);
            _outputResolution = OutputMapSize;
            _viewport = new Viewport(0, 0, _outputResolution, _outputResolution);
            CreateRenderTargetsFromMap(_convertedCubeMap, false);
            UpdateThreaded(path);

            _currentState = RenderState.IrradianceMap;
            _viewport = new Viewport(0, 0, IrradianceSize, IrradianceSize);
            _outputResolution = IrradianceSize;
            CreateRenderTargetsFromMap(_irradianceCubeMap, false);
            UpdateThreaded(path);

            _currentState = RenderState.PreFilteredMap;
            _viewport = new Viewport(0, 0, PreFilteredSize, PreFilteredSize);
            _outputResolution = PreFilteredSize;
            CreateRenderTargetsFromMap(_preFilteredCubeMap, true);
            UpdateThreaded(path);
        }

        public void RenderBRDF(string outputPath)
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
            InitBRDFxResources();
            _viewport = new Viewport(0, 0, BrdFxMapSize, BrdFxMapSize);
            UpdateCubeFace(GetContext, 0);
            _integrateBrdFxMap.Save(GetContext, _device, outputPath, false);
        }

        private void InitBRDFxResources()
        {
            var assetsManager = AssetsManagerInstance.GetManager();
            //var meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_SphereToCubeMapPS");

            var meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IntegrateBRDFxPS");
            var shaderBytecode = new ShaderBytecode(meta.Bytecode);
            _integrateBrdFxPs = ToDispose(new PixelShader(_device, shaderBytecode));
            _integrateBrdFxPs.DebugName = "IntegrateBRDFxPS";

            meta = assetsManager.LoadAsset<AssetsMeta.ShaderAsset>("IBL_PR_IntegrateQuadVS");
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
                Format = Format.R16G16_Float, //R8G8B8A8_UNorm for debug
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

        private void UpdateCubeFace(DeviceContext context, int index)
        {
            UpdateCubeFace(context, index, -1);
        }

        private void UpdateCubeFace(DeviceContext context, int index, int mip)
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
                UnbindContextRTV(context);
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
            UnbindContextRTV(context);
        }

        private void UnbindContextRTV(DeviceContext context)
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

        private void UpdatePreFilteredFaces(DeviceContext context, int startIndex, int endIndex)
        {
            for (var mip = 0; mip < PreFilteredMipsCount; mip++)
            {
                var mipSize = (int)(PreFilteredSize * Math.Pow(0.5, mip));
                var roughness = mip / (float)(PreFilteredMipsCount - 1);
                var cbParams = new BRDFParamsBufferStruct()
                {
                    Roughness = roughness,
                };
                context.UpdateSubresource(ref cbParams, _brdfParamsBuffer);
                for (var j = startIndex; j <= endIndex; j++)
                {
                    _viewport = new Viewport(0, 0, mipSize, mipSize);
                    UpdateCubeFace(context, j, mip);
                }
            }
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
                    _convertedCubeMap.Save(GetContext, _device, $"{path}CubeMap", false);
                    break;
                case RenderState.IrradianceMap:
                    _irradianceCubeMap.Save(GetContext, _device, $"{path}IrradianceCubeMap", false);
                    break;
                case RenderState.PreFilteredMap:
                    _preFilteredCubeMap.Save(GetContext, _device, $"{path}PreFilteredCubeMap", true);
                    break;
                case RenderState.IntegrateBrdf:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
