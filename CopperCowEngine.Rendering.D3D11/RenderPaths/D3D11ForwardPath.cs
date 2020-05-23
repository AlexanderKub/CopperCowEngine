using System.Linq;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.D3D11.Utils;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.ShaderGraph;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Vector4 = System.Numerics.Vector4;
using Vector3 = System.Numerics.Vector3;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal class D3D11ForwardPath : BaseD3D11RenderPath
    {
        private const int MaxNumLights = 1024;
        private static readonly Matrix4x4 SkySphereMatrix = 
            Matrix4x4.CreateScale(Vector3.One * 5000f) * 
            Matrix4x4.CreateFromYawPitchRoll(0, MathUtil.Pi * 0.5f, 0);
        
        private const string DepthPrePassVertexShaderName = "DepthPrePassVS";
        private const string DepthMaskedPrePassVertexShaderName = "DepthMaskedPrePassVS";
        private const string DepthMaskedPrePassPixelShaderName = "DepthMaskedPrePassPS";
        private const string DepthAndVelocityPrePassVertexShaderName = "DepthAndVelocityPrePassVS";
        private const string DepthAndVelocityPrePassPixelShaderName = "DepthAndVelocityPrePassPS";
        private const string DepthAndVelocityMaskedPrePassVertexShaderName = "DepthAndVelocityMaskedPrePassVS";
        private const string DepthAndVelocityMaskedPrePassPixelShaderName = "DepthAndVelocityMaskedPrePassPS";

        private const string ColourPassVertexShaderName = "ForwardStandardVS";
        private const string ColourPassPixelShaderName = "ForwardStandardPS";
        private const string ColourPassTexturedPixelShaderName = "ForwardStandardTexturedPS";
        private const string ColourPassHdrPixelShaderName = "ForwardStandardHdrPS";
        private const string ColourPassHdrTexturedPixelShaderName = "ForwardStandardHdrTexturedPS";
        
        private const string ColourPassSkyDomePixelShaderName = "ForwardSkyDomePS";
        private const string ColourPassSkyDomeHdrPixelShaderName = "ForwardSkyDomeHdrPS";

        private const string ScreenQuadVertexShaderName = "ScreenQuadVS";
        
        private string ScreenQuadPixelShaderName
        {
            get
            {
                return MsaaEnable switch
                {
                    false => (BloomEnable switch
                    {
                        false => "ScreenQuadHdrPS",
                        true => "ScreenQuadHdrBloomPS",
                    }),
                    true => (BloomEnable switch
                    {
                        false => "ScreenQuadHdrMsaaPS",
                        true => "ScreenQuadHdrBloomMsaaPS",
                    })
                };
            }
        }

        private RenderTargetPack _velocityTarget;
        private RenderTargetPack _normalsTarget;
        private RenderTargetPack _screenQuadTarget;
        private DepthStencilTargetPack _depthStencilTarget;
        private DepthStencilView _readonlyDepthStencilView;
        private ShaderResourceView _lightsBufferShaderResource;

        private Buffer _perFrameConstantBuffer;
        private Buffer _perObjectConstantBuffer;
        private Buffer _perMaterialConstantBuffer;
        private Buffer _perFramePreviousConstantBuffer;
        private Buffer _perObjectPreviousConstantBuffer;
        private Buffer _postProcessConstantBuffer;
        private Buffer _perLightBatchConstantBuffer;
        private Buffer _lightsBuffer; 
        private InputLayout _quadLayout;

        private BrandNewCommonStructs.PerFrameConstBufferStruct _perFrameConstValue;
        private BrandNewCommonStructs.PerObjectConstBufferStruct _perObjectConstValue;
        private BrandNewCommonStructs.PerMaterialConstBufferStruct _perMaterialConstValue;
        private BrandNewCommonStructs.PerFramePreviousConstBufferStruct _perFramePreviousConstValue;
        private BrandNewCommonStructs.PerObjectPreviousConstBufferStruct _perObjectPreviousConstValue;
        private BrandNewCommonStructs.PerLightBatchConstBufferStruct _perLightBatchConstValue;
        private BrandNewCommonStructs.PostProcessBufferStruct _postProcessConstValue;

        private StandardFrameData _frameData;
        private Pass _currentPass = Pass.DepthPrePass;
        
        private readonly BrandNewCommonStructs.LightParamsBufferStruct[] _lightsValuesArray = 
            new BrandNewCommonStructs.LightParamsBufferStruct[MaxNumLights];

        private bool _isNullTextures = true;

        private void DebugTonemapping(float a, float b, float c, float d, float e, float f)
        {
            _postProcessConstValue.ToneMapA = a;
            _postProcessConstValue.ToneMapB = b;
            _postProcessConstValue.ToneMapC = c;
            _postProcessConstValue.ToneMapD = d;
            _postProcessConstValue.ToneMapE = e;
            _postProcessConstValue.ToneMapF = f;
            D3DUtils.WriteToDynamicBuffer(GetContext, _postProcessConstantBuffer, _postProcessConstValue);
        }

        private void DebugTonemappingDefault()
        {
            _postProcessConstValue.ToneMapA = 0.25f;
            _postProcessConstValue.ToneMapB = 0.30606f;
            _postProcessConstValue.ToneMapC = 0.09975f;
            _postProcessConstValue.ToneMapD = 0.35f;
            _postProcessConstValue.ToneMapE = 0.025f;
            _postProcessConstValue.ToneMapF = 0.4f;
            D3DUtils.WriteToDynamicBuffer(GetContext, _postProcessConstantBuffer, _postProcessConstValue);
        }

        public override void Init(D3D11RenderBackend renderBackend)
        {
            base.Init(renderBackend);

            renderBackend.ScriptEngine.RegisterFunction("tonemap", this, "DebugTonemapping");
            renderBackend.ScriptEngine.RegisterFunction("tonemapDef", this, "DebugTonemappingDefault");

            CreateBuffers();
            BindStates();

            _screenQuadTarget?.Dispose();
            if (HdrEnable)
            {
                _screenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadHDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, MsSamplesCount);
            }
            else
            {
                _screenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadLDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, MsSamplesCount);
            }
            
            _normalsTarget?.Dispose();
            _normalsTarget = GetSharedItems.CreateRenderTarget("NormalsTarget",
                    GetDisplay.Width, GetDisplay.Height, Format.R11G11B10_Float, MsSamplesCount);
            
            _velocityTarget?.Dispose();
            if (MotionBlurEnable)
            {
                _velocityTarget = GetSharedItems.CreateRenderTarget("Velocity",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16_Float, MsSamplesCount);
            }
            
            _depthStencilTarget?.Dispose();
            _depthStencilTarget = GetSharedItems.CreateDepthRenderTarget("DepthStencil",
                GetDisplay.Width, GetDisplay.Height, MsSamplesCount);

            _readonlyDepthStencilView?.Dispose();
            ToDispose(_readonlyDepthStencilView = new DepthStencilView(GetDevice, _depthStencilTarget.Map, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = MsSamplesCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil,
            }));
            _readonlyDepthStencilView.DebugName = "ReadonlyDepthStencilView";
            MeshAssetsLoader.LoadMesh("SkyDomeMesh");
        }

        public override void Draw(StandardFrameData frameData)
        {
            _frameData = frameData;

            if (_frameData.CamerasList.Count == 0)
            {
                return;
            }
            DepthPrePass();
            LightCullingPass();
            ColourPass();
            if (HdrEnable)
            {
                ScreenBufferResolvePass();
            }
        }

        public override void Resize()
        {
            _screenQuadTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _normalsTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _velocityTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _depthStencilTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);

            _readonlyDepthStencilView.Dispose();
            ToDispose(_readonlyDepthStencilView = new DepthStencilView(GetDevice, _depthStencilTarget.Map, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = MsSamplesCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil,
            }));
            _readonlyDepthStencilView.DebugName = "ReadonlyDepthStencilView";

            if (HdrEnable)
            {
                InitDownsamplingSizableResources();
            }
        }
        
        protected override void OnMaterialChanged()
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_currentPass)
            {
                case Pass.DepthPrePass:
                {
                    if (CachedMaterial.MetaMaterial.BlendMode == MaterialMeta.BlendModeType.Masked)
                    {
                        SetDepthMaskedPrePass();
                        UpdatePerMaterialBuffer();
                        if (CachedMaterial.HasSampler)
                        {
                            GetContext.PixelShader.SetShaderResource(0,
                                GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset));
                        }
                    } 
                    else if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                    {
                        _currentPass++; // Break depth pre pass on non-opaque objects
                    }
                    break;
                }
                case Pass.DepthMaskedPrePass:
                {
                    if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                    {
                        _currentPass++; // Break depth pre pass on non-opaque objects
                        break;
                    }
                    UpdatePerMaterialBuffer();
                    break;
                }
                case Pass.ColourPass:
                    SetMergerStates(CachedMaterial.MetaMaterial);

                    if (HdrEnable)
                    {
                        SetPixelShader(CachedMaterial.HasSampler ? ColourPassHdrTexturedPixelShaderName : ColourPassHdrPixelShaderName);
                    }
                    else
                    {
                        SetPixelShader(CachedMaterial.HasSampler ? ColourPassTexturedPixelShaderName : ColourPassPixelShaderName);
                    }
                    // TODO: set pixel shader per material

                    UpdatePerMaterialBuffer();
                    if (CachedMaterial.HasSampler)
                    {
                        _isNullTextures = false;
                        SetSamplerState(0, (SamplerStateType)CachedMaterial.TexturesSampler);
                        ShaderResourceView[] textures = {
                            CachedMaterial.HasAlbedoMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset) : null,
                            CachedMaterial.HasNormalMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.NormalMapAsset) : null,
                            CachedMaterial.HasRoughnessMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.RoughnessMapAsset) : null,
                            CachedMaterial.HasMetallicMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.MetallicMapAsset) : null,
                            CachedMaterial.HasOcclusionMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.OcclusionMapAsset) : null,
                            CachedMaterial.HasEmissiveMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.EmissiveMapAsset) : null,
                        };
                        GetContext.PixelShader.SetShaderResources(0, 6, textures);
                    }
                    else
                    {
                        if (_isNullTextures)
                        {
                            break;
                        }
                        _isNullTextures = true;
                        ClearSamplerState(0);
                        GetContext.PixelShader.SetShaderResources(0, 6, null, null, null, null, null, null);
                    }
                    break;
            }
        }

        private void UpdatePerMaterialBuffer()
        {
            _perMaterialConstValue = new BrandNewCommonStructs.PerMaterialConstBufferStruct
            {
                AlbedoColor = new Vector4(CachedMaterial.PropertyBlock.AlbedoColor,
                    CachedMaterial.PropertyBlock.AlphaValue),
                EmissiveColor = new Vector4(CachedMaterial.PropertyBlock.EmissiveColor, 1),
                MetallicValue = CachedMaterial.PropertyBlock.MetallicValue,
                RoughnessValue = CachedMaterial.PropertyBlock.RoughnessValue,
                ReflectanceValue = 0.5f,
                Unlit = 0,
                AlphaClip = CachedMaterial.MetaMaterial.OpacityMaskClipValue,

                TextureTiling = CachedMaterial.PropertyBlock.Tile,
                TextureShift = CachedMaterial.PropertyBlock.Shift,

                OptionsMask0 = CommonStructs.CommonStructsHelper.FloatMaskValue(
                    CachedMaterial.HasAlbedoMap, CachedMaterial.HasNormalMap, 
                    CachedMaterial.HasMetallicMap, CachedMaterial.HasRoughnessMap),
                OptionsMask1 = CommonStructs.CommonStructsHelper.FloatMaskValue(
                    CachedMaterial.HasOcclusionMap, CachedMaterial.HasEmissiveMap, false, false),// unlit // non-shadow
            };
            
            D3DUtils.WriteToDynamicBuffer(GetContext, _perMaterialConstantBuffer, _perMaterialConstValue);
        }

        private void DepthPrePass()
        {
            _currentPass = Pass.DepthPrePass;

            SetDepthStencilState(DepthStencilStateType.Greater);
            SetRasterizerState(RasterizerStateType.SolidBackCull);
            SetInputLayout(GetSharedItems.StandardInputLayout);

            if (MotionBlurEnable)
            {
                GetContext.OutputMerger.SetRenderTargets(_depthStencilTarget.View, _velocityTarget.TargetView);
                SetVertexShader(DepthAndVelocityPrePassVertexShaderName);
                SetPixelShader(DepthAndVelocityPrePassPixelShaderName);
                GetContext.ClearRenderTargetView(_velocityTarget.TargetView, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            }
            else
            {
                GetContext.OutputMerger.SetRenderTargets(_depthStencilTarget.View, (RenderTargetView)null);
                SetVertexShader(DepthPrePassVertexShaderName);
                SetNullPixelShader();
            }
            
            GetContext.ClearDepthStencilView(_depthStencilTarget.View,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                0.0f, 0);

            if (MotionBlurEnable)
            {
                _perFramePreviousConstValue = new BrandNewCommonStructs.PerFramePreviousConstBufferStruct
                {
                    PreviousViewProjection = _frameData.CamerasList[0].PreviousViewProjection,
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, _perFramePreviousConstantBuffer, _perFramePreviousConstValue);
            }

            _perFrameConstValue = new BrandNewCommonStructs.PerFrameConstBufferStruct
            {
                Projection = _frameData.CamerasList[0].Projection,
                View = _frameData.CamerasList[0].View,
                CameraPosition = _frameData.CamerasList[0].Position,
                FrameTime = _frameData.CamerasList[0].FrameTime,
                PerspectiveValues = new Vector4(
                    1 / _frameData.CamerasList[0].Projection.M11,
                    1 / _frameData.CamerasList[0].Projection.M22,
                    _frameData.CamerasList[0].Projection.M43,
                    -_frameData.CamerasList[0].Projection.M33),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _perFrameConstantBuffer, _perFrameConstValue);

            ClearMesh();
            ClearMaterial();

            DrawSkySphere(_frameData.CamerasList[0].Position);

            foreach (var rendererData in _frameData.PerCameraRenderers[0].Select(index => _frameData.RenderersList[index]))
            {
                SetMesh(rendererData.MeshGuid);
                SetMaterial(rendererData.MaterialGuid);

                if (_currentPass != Pass.DepthPrePass && _currentPass != Pass.DepthMaskedPrePass)
                {
                    break;
                }
                
                _perObjectConstValue.World = rendererData.TransformMatrix;
                _perObjectConstValue.WorldViewProjection = rendererData.TransformMatrix * _frameData.CamerasList[0].ViewProjection;
                _perObjectPreviousConstValue.PreviousWorldViewProjection = rendererData.PreviousTransformMatrix * _frameData.CamerasList[0].ViewProjection;

                D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstValue);
                if (MotionBlurEnable)
                {
                    D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectPreviousConstantBuffer,
                        _perObjectPreviousConstValue);
                }

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }

        private void SetDepthMaskedPrePass()
        {
            _currentPass = Pass.DepthMaskedPrePass;
            SetRasterizerState(RasterizerStateType.SolidNoneCull);

            if (MotionBlurEnable)
            {
                SetVertexShader(DepthAndVelocityMaskedPrePassVertexShaderName);
                SetPixelShader(DepthAndVelocityMaskedPrePassPixelShaderName);
            }
            else
            {
                SetVertexShader(DepthMaskedPrePassVertexShaderName);
                SetPixelShader(DepthMaskedPrePassPixelShaderName);
            }
        }

        private void LightCullingPass()
        {
            _currentPass = Pass.LightCullingPass;
            //TODO: frame light data

            for (var i = 0; i < 4; i++)
            {
                _lightsValuesArray[i] = new BrandNewCommonStructs.LightParamsBufferStruct
                {
                    Type = (_frameData.LightsList.Count > 0 ? 1 : 0),//i > 0 ? 0 : 
                    Params = new Vector3(0, 0, 0),
                    Center = new Vector3((i) * 5 - 10, 2, 5),
                    InverseRange = 1f / 10f,
                    Color = new Vector3(1f, 1f, 1f),
                    Intensity = 2.5f,
                };
            }
            
            GetContext.UpdateSubresource(_lightsValuesArray, _lightsBuffer);
        }

        private void ColourPass()
        {
            _currentPass = Pass.ColourPass;

            var target = HdrEnable ? _screenQuadTarget.TargetView : GetDisplay.RenderTarget;
            GetContext.ClearRenderTargetView(target, Color.Black);
            GetContext.OutputMerger.SetRenderTargets(_readonlyDepthStencilView, target, _normalsTarget.TargetView);
            SetRasterizerState(RasterizerStateType.SolidNoneCull);
            
            SetVertexShader(ColourPassVertexShaderName);
            
            ClearMesh();
            ClearMaterial();

            _perLightBatchConstValue = new BrandNewCommonStructs.PerLightBatchConstBufferStruct
            {
                LightIndex0 = 0,
                LightIndex1 = 1,
                LightIndex2 = 2,
                LightIndex3 = 3,
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _perLightBatchConstantBuffer, _perLightBatchConstValue);
            
            SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
            DrawSkySphere(_frameData.CamerasList[0].Position);

            foreach (var rendererData in _frameData.PerCameraRenderers[0].Select(index => _frameData.RenderersList[index]))
            {
                SetMesh(rendererData.MeshGuid);
                SetMaterial(rendererData.MaterialGuid);

                _perObjectConstValue.World = rendererData.TransformMatrix;
                _perObjectConstValue.WorldViewProjection = rendererData.TransformMatrix * _frameData.CamerasList[0].ViewProjection;
                _perObjectPreviousConstValue.PreviousWorldViewProjection = rendererData.PreviousTransformMatrix * _frameData.CamerasList[0].ViewProjection;

                D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstValue);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }

        private void ScreenBufferResolvePass()
        {
            _currentPass = Pass.ScreenBufferResolvePass;
            
            ClearMesh();
            GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTarget);
            if (HdrEnable)
            {
                DownSampling(_screenQuadTarget, _frameData.CamerasList[0].FrameTime);
            }
            SetBlendState(BlendStateType.Opaque);
            SetRasterizerState(RasterizerStateType.SolidBackCull);
            
            SetInputLayout(_quadLayout);
            SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
            SetVertexShader(ScreenQuadVertexShaderName);
            
            SetPixelShader(ScreenQuadPixelShaderName);
            
            GetContext.PixelShader.SetShaderResource(0, _screenQuadTarget.ResourceView);
            GetContext.PixelShader.SetShaderResource(1, _velocityTarget.ResourceView);
            GetContext.PixelShader.SetShaderResource(2, _depthStencilTarget.ResourceView);
            GetContext.PixelShader.SetShaderResource(3, CurrentAvgLuminanceSrv);
            if (BloomEnable)
            {
                GetContext.PixelShader.SetShaderResource(4, BloomResultSrv);
            }
            if (DofBlurEnable)
            {
                GetContext.PixelShader.SetShaderResource(5, DownScaledHdrSrv);
            }

            DX_Draw(4, 0);
            GetContext.PixelShader.SetShaderResource(0, null);
            GetContext.PixelShader.SetShaderResource(1, null);
            GetContext.PixelShader.SetShaderResource(2, null);
            GetContext.PixelShader.SetShaderResource(3, null);
            if (BloomEnable)
            {
                GetContext.PixelShader.SetShaderResource(4, null);
            }
            if (DofBlurEnable)
            {
                GetContext.PixelShader.SetShaderResource(5, null);
            }
            //GetContext.OutputMerger.ResetTargets();
        }

        private void DrawSkySphere(Vector3 cameraPosition)
        {
            SetMesh(MeshAssetsLoader.GetGuid("SkyDomeMesh"));

            if (_currentPass == Pass.ColourPass)
            {
                SetPixelShader(HdrEnable ? ColourPassSkyDomeHdrPixelShaderName : ColourPassSkyDomePixelShaderName);
                var textures = new[]
                {
                    (ShaderResourceView)null, null, null, null, null, null,
                };
                GetContext.PixelShader.SetShaderResources(0, 6, textures);

                _perMaterialConstValue = new BrandNewCommonStructs.PerMaterialConstBufferStruct
                {
                    Unlit = 1f,
                    OptionsMask0 = CommonStructs.CommonStructsHelper.FloatMaskValue(true, false, false, false),
                    OptionsMask1 = CommonStructs.CommonStructsHelper.FloatMaskValue(false, false, true, false),
                };
            }

            var world = SkySphereMatrix * Matrix4x4.CreateTranslation(cameraPosition);
            _perObjectConstValue = new BrandNewCommonStructs.PerObjectConstBufferStruct()
            {
                World = world,
                WorldViewProjection = world * _frameData.CamerasList[0].ViewProjection,
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstValue);
            D3DUtils.WriteToDynamicBuffer(GetContext, _perMaterialConstantBuffer, _perMaterialConstValue);
            DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
        }

        private void CreateBuffers()
        {
            var bufferDescription = new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
                SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.PerFrameConstBufferStruct>(),
            };
            _perFrameConstantBuffer?.Dispose();
            ToDispose(_perFrameConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perFrameConstantBuffer.DebugName = "PerFrameBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.PerMaterialConstBufferStruct>();
            _perMaterialConstantBuffer?.Dispose();
            ToDispose(_perMaterialConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perMaterialConstantBuffer.DebugName = "PerMaterialBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.PerObjectConstBufferStruct>();
            _perObjectConstantBuffer?.Dispose();
            ToDispose(_perObjectConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perObjectConstantBuffer.DebugName = "PerObjectBuffer";

            if (MotionBlurEnable)
            {
                bufferDescription.SizeInBytes =
                    Utilities.SizeOf<BrandNewCommonStructs.PerFramePreviousConstBufferStruct>();
                _perFramePreviousConstantBuffer?.Dispose();
                ToDispose(_perFramePreviousConstantBuffer = new Buffer(GetDevice, bufferDescription));
                _perFramePreviousConstantBuffer.DebugName = "PerFramePreviousBuffer";

                bufferDescription.SizeInBytes =
                    Utilities.SizeOf<BrandNewCommonStructs.PerObjectPreviousConstBufferStruct>();
                _perObjectPreviousConstantBuffer?.Dispose();
                ToDispose(_perObjectPreviousConstantBuffer = new Buffer(GetDevice, bufferDescription));
                _perObjectPreviousConstantBuffer.DebugName = "PerObjectPreviousBuffer";
            }

            bufferDescription.SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.PerLightBatchConstBufferStruct>();
            _perLightBatchConstantBuffer?.Dispose();
            ToDispose(_perLightBatchConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perLightBatchConstantBuffer.DebugName = "PerLightBatchBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.PostProcessBufferStruct>();
            _postProcessConstantBuffer?.Dispose();
            ToDispose(_postProcessConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _postProcessConstantBuffer.DebugName = "PostProcessConstantBuffer";

            _postProcessConstValue = new BrandNewCommonStructs.PostProcessBufferStruct
            {
                ExposurePow = 0.5f,
                MaxLuminance = 1.24996f,
                MinLuminance = 0,
                MiddleGrey = 0.24516f,
                NumeratorMultiplier = 1.5019f,
                
                ToneMapA = 0.25f,
                ToneMapB = 0.30606f,
                ToneMapC = 0.09975f,
                ToneMapD = 0.35f,
                ToneMapE = 0.025f,
                ToneMapF = 0.4f,
                BloomScale = 0.1f,
                DOFFarValues = new Vector4(0.005f, 1, 0, 0),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _postProcessConstantBuffer, _postProcessConstValue);
            
            D3D11ShaderLoader.GetShader<VertexShader>(ScreenQuadVertexShaderName, out var vsSignature);
            _quadLayout?.Dispose();
            ToDispose(_quadLayout = new InputLayout(GetDevice, vsSignature, new[] {
                new InputElement("SV_VertexID", 0, Format.R32_UInt, 0, 0),
            }));
            _quadLayout.DebugName = "QuadLayout";

            // Lights buffers
            var lightsBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.LightParamsBufferStruct>() * MaxNumLights, 
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = Utilities.SizeOf<BrandNewCommonStructs.LightParamsBufferStruct>(),
                CpuAccessFlags = CpuAccessFlags.Write,
            };
            _lightsBuffer = Buffer.Create(GetDevice, _lightsValuesArray, lightsBufferDesc);
            _lightsBuffer.DebugName = "LightsBuffer";
            _lightsBufferShaderResource?.Dispose();
            _lightsBufferShaderResource = new ShaderResourceView(GetDevice, _lightsBuffer) { DebugName = "LightsBufferSRV" };

            if (HdrEnable)
            {
                InitDownsamplingResources();
            }
        }

        private void BindStates()
        {
            // TODO: Bind only needed buffers for shader type
            GetContext.VertexShader.SetConstantBuffer(2, _perObjectConstantBuffer);
            GetContext.VertexShader.SetConstantBuffer(3, _perMaterialConstantBuffer);
            if (MotionBlurEnable)
            {
                GetContext.VertexShader.SetConstantBuffer(4, _perFramePreviousConstantBuffer);
                GetContext.VertexShader.SetConstantBuffer(5, _perObjectPreviousConstantBuffer);
            }
            
            GetContext.PixelShader.SetConstantBuffer(0, _perLightBatchConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(1, _perFrameConstantBuffer);
            //GetContext.PixelShader.SetConstantBuffer(2, _perObjectConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(3, _perMaterialConstantBuffer);
            if (MotionBlurEnable)
            {
                GetContext.PixelShader.SetConstantBuffer(4, _perFramePreviousConstantBuffer);
                GetContext.PixelShader.SetConstantBuffer(5, _perObjectPreviousConstantBuffer);
            }
            if (HdrEnable)
            {
                GetContext.PixelShader.SetConstantBuffer(6, _postProcessConstantBuffer);
            }
            
            GetContext.PixelShader.SetShaderResource(6, 
                GetSharedItems.LoadTextureShaderResourceView(MaterialInstance.GetSkySphereMaterial().AlbedoMapAsset, true));
            GetContext.PixelShader.SetShaderResource(7,
                GetSharedItems.LoadTextureShaderResourceView(MaterialInstance.GetSkySphereMaterial().EmissiveMapAsset, true));
                //GetSharedItems.IrradianceMap);
            GetContext.PixelShader.SetShaderResource(8, GetSharedItems.BRDFxLookUpTable);
            GetContext.PixelShader.SetShaderResource(9, _lightsBufferShaderResource);

            SetSamplerState(2, SamplerStateType.LinearSampler);
            SetSamplerState(3, SamplerStateType.TrilinearWrap);
            SetSamplerState(4, SamplerStateType.PointClamp);
            SetSamplerState(5, SamplerStateType.BilinearWrap);
            SetSamplerState(6, SamplerStateType.IBLSampler);
            SetSamplerState(7, SamplerStateType.PreIntegratedSampler);
        }

        private enum Pass
        {
            DepthPrePass,
            DepthMaskedPrePass,
            LightCullingPass,
            ColourPass,
            ScreenBufferResolvePass
        }
    }
}
