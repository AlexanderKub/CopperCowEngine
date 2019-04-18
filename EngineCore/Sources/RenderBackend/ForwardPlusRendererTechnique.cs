using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore.RenderTechnique
{
    //internal class ForwardPlusRendererTechnique : BaseRendererTechnique
    // {
    //    private CommonStructs.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
    //    private CommonStructs.ConstBufferPerFrameStruct m_PerFrameConstBuffer;
    //    private CommonStructs.ConstBufferDirLightStruct[] m_DirLightsConstBuffer;

    //    private const int MAX_NUM_LIGHTS = 2 * 1024;
    //    private const int TILE_RES = 16;
    //    private const int MAX_NUM_LIGHTS_PER_TILE = 544;

    //    private Vector4[] NonDirLightCenterAndRadiusArray = new Vector4[MAX_NUM_LIGHTS];
    //    private Vector4[] NonDirLightColorArray = new Vector4[MAX_NUM_LIGHTS];
    //    private Vector4[] NonDirLightParamsArray = new Vector4[MAX_NUM_LIGHTS];

    //    private BlendState OpaqueState;
    //    private BlendState AlphaEnableBlendingState;
    //    private BlendState DepthOnlyAlphaTestState;
    //    private BlendState DepthOnlyAlphaToCoverageState;

    //    private RasterizerState DisableCullingRasterizerState;
        
    //    private ShaderResourceView DepthStencilSRV;
    //    private DepthStencilState DepthGreaterState;
    //    private DepthStencilState DepthEqualAndDisableDepthWrite;
    //    private DepthStencilState DepthGreaterAndDisableDepthWrite;

    //    private InputLayout VSInputLayout;

    //    private Buffer PerObjectConstBuffer;
    //    private Buffer PerFrameConstBuffer;
    //    private Buffer DirLightsConstBuffer;

    //    private Buffer LightCenterAndRadiusBuffer;
    //    private Buffer LightColorBuffer;
    //    private Buffer LightParamsBuffer;
    //    private Buffer LightIndexBuffer;

    //    private ShaderResourceView LightCenterAndRadiusSRV;
    //    private ShaderResourceView LightColorSRV;
    //    private ShaderResourceView LightParamsSRV;
    //    private ShaderResourceView LightIndexSRV;
    //    private UnorderedAccessView LightIndexURV;

    //    private BufferDescription VertexBufferDescription;
    //    private BufferDescription IndexBufferDescription;

    //    private bool bMSAAEnabled = false;
    //    public override void Init()
    //    {
    //        BufferDescription ConstantBufferDesc = new BufferDescription()
    //        {
    //            Usage = ResourceUsage.Default,
    //            BindFlags = BindFlags.ConstantBuffer,
    //            CpuAccessFlags = CpuAccessFlags.None,
    //            OptionFlags = ResourceOptionFlags.None,
    //        };

    //        ConstantBufferDesc.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>();
    //        m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct();
    //        PerObjectConstBuffer = Buffer.Create(Engine.Instance.Device, ref m_PerObjectConstBuffer, ConstantBufferDesc);

    //        ConstantBufferDesc.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>();
    //        m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct();
    //        PerFrameConstBuffer = Buffer.Create(Engine.Instance.Device, ref m_PerFrameConstBuffer, ConstantBufferDesc);
    //        m_DirLightsConstBuffer = new CommonStructs.ConstBufferDirLightStruct[3];
    //        DirLightsConstBuffer = Buffer.Create(Engine.Instance.Device, m_DirLightsConstBuffer, ConstantBufferDesc);

    //        BlendStateDescription BlendStateDesc = new BlendStateDescription()
    //        {
    //            AlphaToCoverageEnable = false,
    //            IndependentBlendEnable = false,
    //        };
    //        BlendStateDesc.RenderTarget[0].IsBlendEnabled = false;
    //        BlendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
    //        BlendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
    //        BlendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
    //        BlendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
    //        BlendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
    //        BlendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
    //        BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

    //        OpaqueState = new BlendState(Engine.Instance.Device, BlendStateDesc);

    //        BlendStateDesc.RenderTarget[0].IsBlendEnabled = true;
    //        BlendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
    //        BlendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
    //        AlphaEnableBlendingState = new BlendState(Engine.Instance.Device, BlendStateDesc);

    //        BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
    //        DepthOnlyAlphaTestState = new BlendState(Engine.Instance.Device, BlendStateDesc);

    //        BlendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
    //        BlendStateDesc.AlphaToCoverageEnable = true;
    //        DepthOnlyAlphaToCoverageState = new BlendState(Engine.Instance.Device, BlendStateDesc);

    //        RasterizerStateDescription RasterizerStateDesc = new RasterizerStateDescription()
    //        {
    //            FillMode = FillMode.Solid,
    //            CullMode = CullMode.None,
    //            DepthBias = 0,
    //            DepthBiasClamp = 0.0f,
    //            SlopeScaledDepthBias = 0.0f,
    //            IsDepthClipEnabled = true,
    //            IsScissorEnabled = false,
    //            IsMultisampleEnabled = false,
    //            IsAntialiasedLineEnabled = false,
    //        };
    //        DisableCullingRasterizerState = new RasterizerState(Engine.Instance.Device, RasterizerStateDesc);

    //        DepthStencilStateDescription DepthStencilStateDesc = new DepthStencilStateDescription()
    //        {
    //            IsDepthEnabled = true,
    //            DepthWriteMask = DepthWriteMask.All,
    //            DepthComparison = Comparison.Greater,
    //            IsStencilEnabled = false,
    //        };
    //        DepthGreaterState = new DepthStencilState(Engine.Instance.Device, DepthStencilStateDesc);

    //        DepthStencilStateDesc.DepthWriteMask = DepthWriteMask.Zero;
    //        DepthStencilStateDesc.DepthComparison = Comparison.Equal;
    //        DepthEqualAndDisableDepthWrite = new DepthStencilState(Engine.Instance.Device, DepthStencilStateDesc);
    //        DepthStencilStateDesc.DepthComparison = Comparison.Greater;
    //        DepthGreaterAndDisableDepthWrite = new DepthStencilState(Engine.Instance.Device, DepthStencilStateDesc);


    //        InitLightsData();
    //        AddShaders();

    //        VertexBufferDescription = new BufferDescription()
    //        {
    //            BindFlags = BindFlags.VertexBuffer,
    //            CpuAccessFlags = CpuAccessFlags.None,
    //            OptionFlags = ResourceOptionFlags.None,
    //            Usage = ResourceUsage.Default
    //        };
    //        IndexBufferDescription = new BufferDescription()
    //        {
    //            BindFlags = BindFlags.IndexBuffer,
    //            CpuAccessFlags = CpuAccessFlags.None,
    //            OptionFlags = ResourceOptionFlags.None,
    //            Usage = ResourceUsage.Default,
    //        };
    //        Resize();
    //        Camera.IsInvertedDepthBuffer = true;
    //    }

    //    public override void InitRenderer(Renderer renderer)
    //    {
    //        renderer.PixelShader = null;
    //        renderer.PixelShader = AssetsLoader.GetShader<PixelShader>("ForwardPlusScenePS");
    //        if (renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere)
    //        {
    //            renderer.PixelShader = AssetsLoader.GetShader<PixelShader>("FwdSkySpherePS");
    //        }
    //        renderer.VertexBuffer?.Dispose();
    //        renderer.VertexBuffer = null;
    //        renderer.IndexBuffer?.Dispose();
    //        renderer.IndexBuffer = null;
    //        renderer.VertexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Points, VertexBufferDescription);
    //        renderer.IndexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Indexes, IndexBufferDescription);
    //    }

    //    public override void Draw()
    //    {
    //        base.Draw();
    //        UpdateLights();
    //        Engine.Instance.MainCamera?.Update();
    //        DepthStencilView depthStencilView = Engine.Instance.DisplayRef.DepthStencilViewRef;
    //        RenderTargetView renderTargetView = Engine.Instance.DisplayRef.RenderTargetViewRef;

    //        Engine.Instance.Context.ClearRenderTargetView(renderTargetView, Engine.Instance.ClearColor);
    //        Engine.Instance.Context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 0.0f, 0);

    //        Light[] lights = Engine.Instance.GetLightsByType(Light.LightType.Directional);
    //        //m_DirLightsConstBuffer = new CommonStructs.ConstBufferDirLightStruct[lights.Length];
    //        for (int i = 0; i < lights.Length; i++)
    //        {
    //            m_DirLightsConstBuffer[i] = new CommonStructs.ConstBufferDirLightStruct() {
    //                DirLightDirection = lights[i].Direction,
    //                DirLightColor = lights[i].LightColor,
    //                DirLightIntensity = lights[i].LightIntensity,
    //            };
    //        }

    //        m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
    //        {
    //            Projection = Engine.Instance.MainCamera.Projection,
    //            ProjectionInv = Matrix.Invert(Engine.Instance.MainCamera.Projection),
    //            CameraPos = Engine.Instance.MainCamera.transform.Position,
    //            AlphaTest = bMSAAEnabled ? 0.003f : 0.5f,
    //            MaxNumLightsPerTile = (uint)GetMaxNumLightsPerTile(),
    //            NumLights = (uint)Engine.Instance.NonDirLights.Length,
    //            WindowHeight = (uint)Engine.Instance.DisplayRef.Height,
    //            WindowWidth = (uint)Engine.Instance.DisplayRef.Width,
    //            DirLightsNum = (uint)lights.Length,
    //        };

    //        Engine.Instance.Context.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstBuffer);
    //        Engine.Instance.Context.UpdateSubresource(m_DirLightsConstBuffer, DirLightsConstBuffer);
    //        Engine.Instance.Context.VertexShader.SetConstantBuffer(1, PerFrameConstBuffer);
    //        Engine.Instance.Context.PixelShader.SetConstantBuffer(1, PerFrameConstBuffer);
    //        Engine.Instance.Context.PixelShader.SetConstantBuffer(2, DirLightsConstBuffer);
    //        Engine.Instance.Context.ComputeShader.SetConstantBuffer(1, PerFrameConstBuffer);
            
    //        SharpDX.Mathematics.Interop.RawColor4 BlendFactor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);
    //        Engine.Instance.Context.OutputMerger.SetBlendState(OpaqueState, BlendFactor, 0xFFFFFFFF);
            
    //        #region Depth Pre-Pass
    //        DepthPrePassState = true;
    //        Engine.Instance.Context.OutputMerger.SetRenderTargets(depthStencilView, (RenderTargetView)null);
    //        Engine.Instance.Context.OutputMerger.SetDepthStencilState(DepthGreaterState);
    //        Engine.Instance.Context.InputAssembler.InputLayout = VSInputLayout;

    //        Engine.Instance.Context.VertexShader.Set(ForwardPlusPosOnlyVS);
    //        Engine.Instance.Context.PixelShader.Set(null);
    //        for (int i = 0; i < 2; i++) {
    //            Engine.Instance.Context.PixelShader.SetShaderResources(0, i, (ShaderResourceView)null);
    //        }
    //        Engine.Instance.Context.PixelShader.SetSamplers(0, 1, (SamplerState)null);
    //        //*********************************
    //        //TODO: Draw opaque geometry      *
    //        //*********************************
    //        TransparentPassState = false;
    //        Engine.Instance.SetSolidRender();
    //        Engine.Instance.GameObjects.ForEach((x) => {
    //            x.Draw();
    //        });

    //        Engine.Instance.Context.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);
    //        if (bMSAAEnabled) {
    //            Engine.Instance.Context.OutputMerger.SetBlendState(DepthOnlyAlphaToCoverageState, BlendFactor, 0xFFFFFFFF);
    //        } else {
    //            Engine.Instance.Context.OutputMerger.SetBlendState(DepthOnlyAlphaTestState, BlendFactor, 0xFFFFFFFF);
    //        }

    //        Engine.Instance.Context.Rasterizer.State = DisableCullingRasterizerState;
    //        Engine.Instance.Context.InputAssembler.InputLayout = VSInputLayout;
    //        Engine.Instance.Context.VertexShader.Set(ForwardPlusPosTexVS);
    //        Engine.Instance.Context.PixelShader.Set(ForwardPlusPosTexPS);
    //        Engine.Instance.Context.PixelShader.SetSampler(0, SharedRenderItems.AnisotropicWrapSamplerState);
    //        //*********************************
    //        //TODO: Draw transparent geometry *
    //        //*********************************
    //        TransparentPassState = true;
    //        Engine.Instance.GameObjects.ForEach((x) => {
    //            x.Draw();
    //        });

    //        Engine.Instance.Context.Rasterizer.State = null;
    //        Engine.Instance.Context.OutputMerger.SetBlendState(OpaqueState, BlendFactor, 0xFFFFFFFF);
    //        #endregion

    //        #region Light Culling
    //        DepthPrePassState = false;
    //        m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct()
    //        {
    //            WorldMatrix = Matrix.Identity,
    //            WorldViewMatrix = Matrix.Identity * Engine.Instance.MainCamera.View,
    //            WorldViewProjMatrix = Matrix.Identity * Engine.Instance.MainCamera.ViewProjectionMatrix,
    //        };
    //        Engine.Instance.Context.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjectConstBuffer);
    //        Engine.Instance.Context.ComputeShader.SetConstantBuffer(0, PerObjectConstBuffer);

    //        Engine.Instance.Context.OutputMerger.SetRenderTargets((DepthStencilView)null, (RenderTargetView)null);
    //        Engine.Instance.Context.VertexShader.Set(null);
    //        Engine.Instance.Context.PixelShader.Set(null);
    //        for (int i = 0; i < 2; i++) {
    //            Engine.Instance.Context.PixelShader.SetShaderResources(0, i, (ShaderResourceView)null);
    //        }
    //        Engine.Instance.Context.PixelShader.SetSamplers(0, 1, (SamplerState)null);

    //        Engine.Instance.Context.ComputeShader.Set(LightCullingCS);
    //        Engine.Instance.Context.ComputeShader.SetShaderResources(0, 1, LightCenterAndRadiusSRV);
    //        Engine.Instance.Context.ComputeShader.SetShaderResources(1, 1, DepthStencilSRV);
    //        Engine.Instance.Context.ComputeShader.SetUnorderedAccessView(0, LightIndexURV);

    //        Engine.Instance.Context.Dispatch(GetNumTilesX(), GetNumTilesY(), 1);

    //        Engine.Instance.Context.ComputeShader.Set(null);
    //        Engine.Instance.Context.ComputeShader.SetShaderResources(0, 1, (ShaderResourceView)null);
    //        Engine.Instance.Context.ComputeShader.SetShaderResources(1, 1, (ShaderResourceView)null);
    //        Engine.Instance.Context.ComputeShader.SetUnorderedAccessView(0, null);
    //        #endregion

    //        #region Forward Rendering
    //        DepthPrePassState = false;
    //        Engine.Instance.Context.OutputMerger.SetRenderTargets(depthStencilView, Engine.Instance.DisplayRef.RenderTargetViewRef);
    //        Engine.Instance.Context.OutputMerger.SetDepthStencilState(DepthEqualAndDisableDepthWrite, 0x00);

    //        Engine.Instance.Context.InputAssembler.InputLayout = VSInputLayout;

    //        Engine.Instance.Context.VertexShader.Set(ForwardPlusSceneVS);
    //        Engine.Instance.Context.PixelShader.Set(ForwardPlusScenePS);
    //        //TODO: Depend by render item but not reduant
    //        Engine.Instance.Context.PixelShader.SetSampler(0, SharedRenderItems.LinearWrapSamplerState);

    //        //Radiance
    //        Engine.Instance.Context.PixelShader.SetShaderResource(5, Material.GetSkySphereMaterial().albedoMapView);
    //        //Irradiance
    //        Engine.Instance.Context.PixelShader.SetShaderResource(6, Material.IrradianceMap);
    //        //Lights data
    //        Engine.Instance.Context.PixelShader.SetShaderResources(7, 1, LightCenterAndRadiusSRV);
    //        Engine.Instance.Context.PixelShader.SetShaderResources(8, 1, LightParamsSRV);
    //        Engine.Instance.Context.PixelShader.SetShaderResources(9, 1, LightColorSRV);
    //        Engine.Instance.Context.PixelShader.SetShaderResources(10, 1, LightIndexSRV);
    //        //*********************************
    //        //TODO: Draw opaque geometry      *
    //        //*********************************
    //        TransparentPassState = false;
    //        Engine.Instance.SetSolidRender();
    //        Engine.Instance.GameObjects.ForEach((x) => {
    //            x.Draw();
    //        });
            
    //        Engine.Instance.Context.Rasterizer.State = DisableCullingRasterizerState;
    //        Engine.Instance.Context.OutputMerger.SetBlendState(AlphaEnableBlendingState, BlendFactor, 0xFFFFFFFF);
    //        Engine.Instance.Context.OutputMerger.SetDepthStencilState(DepthGreaterAndDisableDepthWrite, 0x00);
    //        //TODO: shader macros
    //        Engine.Instance.Context.PixelShader.Set(ForwardPlusScenePS);
    //        //*********************************
    //        //TODO: Draw transparent geometry *
    //        //*********************************
    //        TransparentPassState = true;
    //        Engine.Instance.GameObjects.ForEach((x) => {
    //            x.Draw();
    //        });

    //        Engine.Instance.Context.Rasterizer.State = null;
    //        for (int i = 0; i < 2; i++) {
    //            Engine.Instance.Context.PixelShader.SetShaderResources(0, i, (ShaderResourceView)null);
    //        }
    //        Engine.Instance.Context.OutputMerger.SetDepthStencilState(DepthGreaterState, 0x00);
    //        #endregion
    //    }

    //    private bool DepthPrePassState = true;
    //    private bool TransparentPassState = false;
    //    public override void RenderItem(Renderer renderer)
    //    {
    //        Material mref = renderer.RendererMaterial;
    //        //TODO: refactor scene Graph
    //        if (mref.PropetyBlock.AlphaValue < 1.0f) {
    //            if (!TransparentPassState) { return; }
    //        } else { 
    //            if (TransparentPassState) { return; }
    //        }

    //        if (!FrustrumCullingTest(renderer)) {
    //            return;
    //        }

    //        m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct()
    //        {
    //            WorldViewProjMatrix = renderer.transform.TransformMatrix * Engine.Instance.MainCamera.ViewProjectionMatrix,
    //            WorldViewMatrix = renderer.transform.TransformMatrix * Engine.Instance.MainCamera.View,
    //            WorldMatrix = renderer.transform.TransformMatrix,

    //            textureTiling = renderer.GetPropetyBlock.Tile,
    //            textureShift = renderer.GetPropetyBlock.Shift,

    //            AlbedoColor = new Vector4(renderer.GetPropetyBlock.AlbedoColor, renderer.GetPropetyBlock.AlphaValue),
    //            RoughnessValue = renderer.GetPropetyBlock.RoughnessValue,
    //            MetallicValue = renderer.GetPropetyBlock.MetallicValue,

    //            optionsMask0 = floatMaskVal(mref.HasAlbedoMap, mref.HasNormalMap, mref.HasRoughnessMap, mref.HasMetallicMap),
    //            optionsMask1 = floatMaskVal(mref.HasOcclusionMap,
    //                renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere || renderer.SpecificType == Renderer.SpecificTypeEnum.Unlit,
    //                renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere, false),
    //            filler = Vector2.Zero,
    //        };

    //        Engine.Instance.Context.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjectConstBuffer);
    //        Engine.Instance.Context.VertexShader.SetConstantBuffer(0, PerObjectConstBuffer);
    //        if (!DepthPrePassState) {
    //            Engine.Instance.Context.PixelShader.Set(renderer.PixelShader);
    //            Engine.Instance.Context.PixelShader.SetConstantBuffer(0, PerObjectConstBuffer);

    //            // TODO: Change textures behavior
    //            if (renderer.RendererMaterial.HasSampler) {
    //                Engine.Instance.Context.PixelShader.SetSampler(0, renderer.RendererMaterial.MaterialSampler);
    //                if (renderer.RendererMaterial.HasAlbedoMap) {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(0, renderer.RendererMaterial.albedoMapView);
    //                } else {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(0, null);
    //                } if (renderer.RendererMaterial.HasNormalMap) {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(1, renderer.RendererMaterial.normalMapView);
    //                } else {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(1, null);
    //                } if (renderer.RendererMaterial.HasRoughnessMap) {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(2, renderer.RendererMaterial.roughnessMapView);
    //                } else {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(2, null);
    //                } if (renderer.RendererMaterial.HasMetallicMap) {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(3, renderer.RendererMaterial.metallicMapView);
    //                } else {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(3, null);
    //                }
    //                if (renderer.RendererMaterial.HasOcclusionMap) {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(4, renderer.RendererMaterial.occlusionMapView);
    //                } else {
    //                    Engine.Instance.Context.PixelShader.SetShaderResource(4, null);
    //                }
    //            } else {
    //                Engine.Instance.Context.PixelShader.SetSampler(0, null);
    //                Engine.Instance.Context.PixelShader.SetShaderResource(0, null);
    //                Engine.Instance.Context.PixelShader.SetShaderResource(1, null);
    //                Engine.Instance.Context.PixelShader.SetShaderResource(2, null);
    //                Engine.Instance.Context.PixelShader.SetShaderResource(3, null);
    //                Engine.Instance.Context.PixelShader.SetShaderResource(4, null);
    //            }
    //        }

    //        if (renderer.SpecificType == Renderer.SpecificTypeEnum.Wireframe) {
    //            Engine.Instance.SetWireframeRender();
    //        } else {
    //            Engine.Instance.SetSolidRender();
    //        }

    //        Engine.Instance.Context.InputAssembler.PrimitiveTopology = renderer.Topology;
    //        Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderer.VertexBuffer, 96, 0));
    //        Engine.Instance.Context.InputAssembler.SetIndexBuffer(renderer.IndexBuffer, Format.R32_UInt, 0);
    //        Engine.Instance.Context.DrawIndexed(renderer.Geometry.Indexes.Length, 0, 0);
    //    }

    //    public override void Resize()
    //    {
    //        //Engine.Instance.ResetTargets();
    //        //int Width = Engine.Instance.DisplayRef.Width;
    //        //int Height = Engine.Instance.DisplayRef.Height;

    //        DepthStencilSRV = Engine.Instance.DisplayRef.DepthStencilSRVRef;
    //        /*CreateDepthStencilSurface(out DepthStencilTexture, out DepthStencilSRV,
    //            out PDepthStencilView, Format.D32_Float, Format.R32_Float,
    //            Width, Height, 1);*/

    //        int NumTiles = GetNumTilesX() * GetNumTilesY();
    //        int MaxNumLightsPerTile = GetMaxNumLightsPerTile();

    //        BufferDescription BufferDesc = new BufferDescription()
    //        {
    //            Usage = ResourceUsage.Default,
    //            SizeInBytes = 4 * MaxNumLightsPerTile * NumTiles,
    //            BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
    //        };
    //        LightIndexBuffer = new Buffer(Engine.Instance.Device, BufferDesc);

    //        ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription()
    //        {
    //            Format = Format.R32_UInt,
    //            Dimension = ShaderResourceViewDimension.Buffer,
    //            Buffer = new ShaderResourceViewDescription.BufferResource()
    //            {
    //                ElementOffset = 0,
    //                ElementWidth = MaxNumLightsPerTile * NumTiles,
    //            },
    //        };
    //        LightIndexSRV = new ShaderResourceView(Engine.Instance.Device, LightIndexBuffer, SRVDesc);

    //        UnorderedAccessViewDescription UAVDesc = new UnorderedAccessViewDescription() {
    //            Format = Format.R32_UInt,
    //            Dimension = UnorderedAccessViewDimension.Buffer,
    //            Buffer = new UnorderedAccessViewDescription.BufferResource()
    //            {
    //                FirstElement = 0,
    //                ElementCount = MaxNumLightsPerTile * NumTiles,
    //            },
    //        };
    //        LightIndexURV = new UnorderedAccessView(Engine.Instance.Device, LightIndexBuffer, UAVDesc);
    //    }

    //    public override void Dispose()
    //    {
    //        SharedRenderItems.Dispose();

    //        OpaqueState?.Dispose();
    //        OpaqueState = null;
    //        AlphaEnableBlendingState?.Dispose();
    //        AlphaEnableBlendingState = null;
    //        DepthOnlyAlphaTestState?.Dispose();
    //        DepthOnlyAlphaTestState = null;
    //        DepthOnlyAlphaToCoverageState?.Dispose();
    //        DepthOnlyAlphaToCoverageState = null;

    //        DisableCullingRasterizerState?.Dispose();
    //        DisableCullingRasterizerState = null;
    //        DepthGreaterState?.Dispose();
    //        DepthGreaterState = null;
    //        DepthGreaterAndDisableDepthWrite?.Dispose();
    //        DepthGreaterAndDisableDepthWrite = null;
    //        DepthEqualAndDisableDepthWrite?.Dispose();
    //        DepthEqualAndDisableDepthWrite = null;

    //        PerObjectConstBuffer?.Dispose();
    //        PerObjectConstBuffer = null;
    //        PerFrameConstBuffer?.Dispose();
    //        PerFrameConstBuffer = null;

    //        LightCenterAndRadiusBuffer?.Dispose();
    //        LightCenterAndRadiusBuffer = null;
    //        LightColorBuffer?.Dispose();
    //        LightColorBuffer = null;
    //        LightParamsBuffer?.Dispose();
    //        LightParamsBuffer = null;
    //        LightIndexBuffer?.Dispose();
    //        LightIndexBuffer = null;

    //        LightCenterAndRadiusSRV?.Dispose();
    //        LightCenterAndRadiusSRV = null;
    //        LightColorSRV?.Dispose();
    //        LightColorSRV = null;
    //        LightParamsSRV?.Dispose();
    //        LightParamsSRV = null;
    //        LightIndexSRV?.Dispose();
    //        LightIndexSRV = null;
    //        LightIndexURV?.Dispose();
    //        LightIndexURV = null;
    //    }

    //    #region Helpers functions
    //    private ComputeShader LightCullingCS;
    //    private VertexShader ForwardPlusPosOnlyVS;
    //    private VertexShader ForwardPlusPosTexVS;
    //    private VertexShader ForwardPlusSceneVS;
    //    private PixelShader ForwardPlusPosTexPS;
    //    private PixelShader ForwardPlusScenePS;
    //    private void AddShaders()
    //    {
    //        ForwardPlusSceneVS = AssetsLoader.GetShader<VertexShader>("CommonVS", out ShaderSignature shaderSignature);
    //        VSInputLayout = new InputLayout(Engine.Instance.Device, shaderSignature, new InputElement[] {
    //            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //            new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //            new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //            new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
    //        });
    //        ForwardPlusPosOnlyVS = AssetsLoader.GetShader<VertexShader>("ForwardPlusPosOnlyVS");
    //        ForwardPlusPosTexVS = AssetsLoader.GetShader<VertexShader>("ForwardPlusPosTexVS");

    //        ForwardPlusPosTexPS = AssetsLoader.GetShader<PixelShader>("ForwardPlusPosTexPS");
    //        ForwardPlusScenePS = AssetsLoader.GetShader<PixelShader>("ForwardPlusScenePS");
    //        LightCullingCS = AssetsLoader.GetShader<ComputeShader>("LightCullingCS");
    //    }

    //    float t = 0;
    //    float halfDeg = MathUtil.DegreesToRadians(0.5f);
    //    private void UpdateLights()
    //    {
    //        //TODO: get light from scene information
    //        /*
    //         * NonDirLightCenterAndRadiusArray: xyz - center, w - radius
    //         * NonDirLightColorArray: xy - direction, z-sign - direction part, z-value - cone cosine
    //         * NonDirLightParamsArray rgb - color, a - intensity:
    //         * 
    //         * */
    //        NonDirLightCenterAndRadiusArray = new Vector4[MAX_NUM_LIGHTS];
    //        NonDirLightColorArray = new Vector4[MAX_NUM_LIGHTS];
    //        NonDirLightParamsArray = new Vector4[MAX_NUM_LIGHTS];

    //        Light[] lights = Engine.Instance.NonDirLights;
    //        float CosOfCone = (float)System.Math.Cos(MathUtil.DegreesToRadians(30));
    //        for (int i = 0; i < lights.Length; i++)
    //        {
    //            NonDirLightCenterAndRadiusArray[i] = new Vector4(lights[i].transform.Position, lights[i].radius);
    //            NonDirLightColorArray[i] = lights[i].LightColor;
    //            NonDirLightColorArray[i].W = lights[i].LightIntensity;
    //            NonDirLightParamsArray[i] = new Vector4(lights[i].Direction, (lights[i].Type == Light.LightType.Point) ? 0f : 1f);
    //            NonDirLightParamsArray[i].Z = NonDirLightParamsArray[i].Z > 0 ? CosOfCone : -CosOfCone;
    //        }
    //        Engine.Instance.Context.UpdateSubresource(NonDirLightCenterAndRadiusArray, LightCenterAndRadiusBuffer);
    //        Engine.Instance.Context.UpdateSubresource(NonDirLightColorArray, LightColorBuffer);
    //        Engine.Instance.Context.UpdateSubresource(NonDirLightParamsArray, LightParamsBuffer);

    //        LightCenterAndRadiusSRV?.Dispose();
    //        LightCenterAndRadiusSRV = new ShaderResourceView(Engine.Instance.Device, LightCenterAndRadiusBuffer, new ShaderResourceViewDescription()
    //        {
    //            Format = Format.R32G32B32A32_Float,
    //            Dimension = ShaderResourceViewDimension.Buffer,
    //            Buffer = new ShaderResourceViewDescription.BufferResource()
    //            {
    //                ElementOffset = 0,
    //                ElementWidth = MAX_NUM_LIGHTS,
    //            },
    //        });

    //        LightColorSRV?.Dispose();
    //        LightColorSRV = new ShaderResourceView(Engine.Instance.Device, LightColorBuffer, new ShaderResourceViewDescription()
    //        {
    //            Format = Format.R8G8B8A8_UNorm,
    //            Dimension = ShaderResourceViewDimension.Buffer,
    //            Buffer = new ShaderResourceViewDescription.BufferResource()
    //            {
    //                ElementOffset = 0,
    //                ElementWidth = MAX_NUM_LIGHTS,
    //            },
    //        });
            
    //        LightParamsSRV?.Dispose();
    //        LightParamsSRV = new ShaderResourceView(Engine.Instance.Device, LightParamsBuffer, new ShaderResourceViewDescription()
    //        {
    //            Format = Format.R32G32B32A32_Float,
    //            Dimension = ShaderResourceViewDimension.Buffer,
    //            Buffer = new ShaderResourceViewDescription.BufferResource()
    //            {
    //                ElementOffset = 0,
    //                ElementWidth = MAX_NUM_LIGHTS,
    //            },
    //        });
    //    }

    //    private void InitLightsData()
    //    {
    //        NonDirLightCenterAndRadiusArray = new Vector4[MAX_NUM_LIGHTS];
    //        NonDirLightColorArray = new Vector4[MAX_NUM_LIGHTS];
    //        NonDirLightParamsArray = new Vector4[MAX_NUM_LIGHTS];

    //        Light[] lights = Engine.Instance.NonDirLights;
    //        float CosOfCone = (float)System.Math.Cos(MathUtil.DegreesToRadians(45));
    //        for (int i = 0; i < lights.Length; i++) {
    //            NonDirLightCenterAndRadiusArray[i] = new Vector4(lights[i].transform.Position, lights[i].radius);
    //            NonDirLightColorArray[i] = lights[i].LightColor;
    //            NonDirLightColorArray[i].W = lights[i].LightIntensity;
    //            NonDirLightParamsArray[i] = new Vector4(lights[i].Direction, (lights[i].Type == Light.LightType.Point) ? 0f : 1f);
    //            NonDirLightParamsArray[i].Z = NonDirLightParamsArray[i].Z > 0 ? CosOfCone : -CosOfCone;
    //        }

    //        BufferDescription LightBufferDesc = new BufferDescription()
    //        {
    //            Usage = ResourceUsage.Default,
    //            SizeInBytes = sizeof(float) * 4 * MAX_NUM_LIGHTS,
    //            BindFlags = BindFlags.ShaderResource,
    //            CpuAccessFlags = CpuAccessFlags.Write,
    //        };

    //        ShaderResourceViewDescription LightSRVDesc = new ShaderResourceViewDescription()
    //        {
    //            Format = Format.R32G32B32A32_Float,
    //            Dimension = ShaderResourceViewDimension.Buffer,
    //            Buffer = new ShaderResourceViewDescription.BufferResource()
    //            {
    //                ElementOffset = 0,
    //                ElementWidth = MAX_NUM_LIGHTS,
    //            },
    //        };

    //        LightCenterAndRadiusBuffer = Buffer.Create(Engine.Instance.Device, NonDirLightCenterAndRadiusArray, LightBufferDesc);
    //        LightCenterAndRadiusSRV = new ShaderResourceView(Engine.Instance.Device, LightCenterAndRadiusBuffer, LightSRVDesc);

    //        LightColorBuffer = Buffer.Create(Engine.Instance.Device, NonDirLightColorArray, LightBufferDesc);
    //        LightSRVDesc.Format = Format.R8G8B8A8_UNorm;
    //        LightColorSRV = new ShaderResourceView(Engine.Instance.Device, LightColorBuffer, LightSRVDesc);

    //        LightParamsBuffer = Buffer.Create(Engine.Instance.Device, NonDirLightParamsArray, LightBufferDesc);
    //        LightSRVDesc.Format = Format.R32G32B32A32_Float;
    //        LightParamsSRV = new ShaderResourceView(Engine.Instance.Device, LightParamsBuffer, LightSRVDesc);
    //    }

    //    private int GetNumTilesX()
    //    {
    //        return (int)((Engine.Instance.DisplayRef.Width + TILE_RES - 1) / (float)TILE_RES);
    //    }

    //    private int GetNumTilesY()
    //    {
    //        return (int)((Engine.Instance.DisplayRef.Height + TILE_RES - 1) / (float)TILE_RES);
    //    }

    //    private const int kAdjustmentMultipier = 32;
    //    private int GetMaxNumLightsPerTile()
    //    {
    //        int uHeight = (Engine.Instance.DisplayRef.Height > 1080) ? 1080 : Engine.Instance.DisplayRef.Height;

    //        return (MAX_NUM_LIGHTS_PER_TILE - (kAdjustmentMultipier * (uHeight / 120)));
    //    }

    //    private Vector4 floatMaskVal(bool v0, bool v1, bool v2, bool v3)
    //    {
    //        return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
    //    }

    //    #endregion
    //}
}
