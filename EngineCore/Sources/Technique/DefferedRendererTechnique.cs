using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.Collections.Generic;

namespace EngineCore.Technique
{
    class DefferedRendererTechnique: BaseRendererTechnique
    {
        private GBuffer gBuffer;
        private List<DefferedQuadRenderer> m_QuadRenderers;
      
        public override void Init() {
            if (Engine.Instance.DisplayRef.Width != 0) {
                gBuffer = new GBuffer();
            }
            m_QuadRenderers = new List<DefferedQuadRenderer>();
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), 10));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f), 1));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), 2));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), 3));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 4));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 5));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 6));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 7));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 8));
            m_QuadRenderers.Add(new DefferedQuadRenderer(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 9));
            InitMainObjectsForRenderers();
        }

        public override void Draw() {
            Engine.Instance?.MainCamera.Update();
            //Shadow Map Pass
            //Engine.Instance.MainLight.Draw();

            DepthStencilView depthStencilView = Engine.Instance.DisplayRef.DepthStencilViewRef;
            RenderTargetView renderTargetView = Engine.Instance.DisplayRef.RenderTargetViewRef;

            Engine.Instance.Context.ClearDepthStencilView(
                depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1f, 0
            );

            Engine.Instance.Context.OutputMerger.SetRenderTargets(depthStencilView, gBuffer.renderTargetViews);
            for (int i = 0; i < gBuffer.TargetsCount; i++) {
                Engine.Instance.Context.ClearRenderTargetView(gBuffer.renderTargetViews[i], Engine.Instance.ClearColor);
            }

            Engine.Instance.GameObjects.ForEach((x) => {
                x.Draw();
            });

            Engine.Instance.Context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            Engine.Instance.Context.ClearRenderTargetView(renderTargetView, Engine.Instance.ClearColor);
            int j = 0;
            m_QuadRenderers.ForEach((m_QuadRenderer) => {
                if ((!m_DebugFlag && j == 0) || (m_DebugFlag && j > 0)) {
                    if (m_DebugIndex == 0 || (m_DebugIndex > 0 && m_DebugIndex == j)) {
                        m_QuadRenderer.Draw(gBuffer);
                    }
                }
                j++;
            });
        }

        public override void OnChangeRender() {
            int j = 0;
            m_DebugIndex = m_DebugIndex > 9 ? 0 : m_DebugIndex;
            m_QuadRenderers.ForEach((m_QuadRenderer) => {
                if (m_DebugIndex == j) {
                    m_QuadRenderer.FullScreen = true;
                } else {
                    m_QuadRenderer.FullScreen = false;
                }
                j++;
            });
            string[] TypeRNames = {
                "Deffered Target",
                "Albedo Target",
                "Positions Target",
                "Normals Target",
                "Roughness Target",
                "Metallic Target",
                "Depth Target",
                "Occlusion Target",
                "Unlit Target",
                "NonShadows Target",
            };
            Engine.Log(TypeRNames[m_DebugIndex]);
        }

        public override void Resize() {
            gBuffer?.Dispose();
            gBuffer = new GBuffer();
        }

        /*********************************RENDERERS SECTION*********************************/

        private struct TransformMatrixStruct
        {
            public Matrix transformViewProj;
            public Matrix transformWorld;
            public Vector4 cameraPosition;
            public Vector2 textureTiling;
            public Vector2 textureShift;

            public Vector4 AlbedoColor;
            public float RoughnessValue;
            public float MetallicValue; 
            public Vector2 filler;

            //x hasAlbedoMap;
            //y hasNormalMap;
            //z hasRoughnessMap;
            //w hasMetallicMap;
            public Vector4 optionsMask0;
            //x hasOcclusionMap;
            //y unlit;
            //z nonRecieveShadows;
            //w empty
            public Vector4 optionsMask1;
        }
        private TransformMatrixStruct m_TransformMatrix;

        private BufferDescription VertexBufferDescription;
        private BufferDescription IndexBufferDescription;

        private Buffer ContantBuffer;

        private ShaderBytecodePack shaderBytecodePack;
        private VertexShader DefferedVS;
        private PixelShader DefferedPS;
        private VertexShader SkySphereVS;
        private PixelShader SkySpherePS;
        private VertexShader ReflectionSphereVS;
        private PixelShader ReflectionSpherePS;
        private InputLayout DefferedLayout;

        private void InitMainObjectsForRenderers() {
            //create shaders and layouts
            shaderBytecodePack = AssetsLoader.GetShaderPack("DefferedPBRShader");
            DefferedVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            DefferedPS = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);

            shaderBytecodePack = AssetsLoader.GetShaderPack("SkySphereShader");
            SkySphereVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            SkySpherePS = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);

            shaderBytecodePack = AssetsLoader.GetShaderPack("ReflectionShader");
            ReflectionSphereVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            ReflectionSpherePS = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);

            DefferedLayout = new InputLayout(
                Engine.Instance.Device,
                ShaderSignature.GetInputSignature(shaderBytecodePack.VertexShaderByteCode),
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 32, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 48, 0),
                    new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, 64, 0),
                    new InputElement("BINORMAL", 0, Format.R32G32B32A32_Float, 80, 0),
                }
            );

            //Create buffer descriptions for renderers
            VertexBufferDescription = new BufferDescription() {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };
            IndexBufferDescription = new BufferDescription() {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };

            //Create constant buffer 
            ContantBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<TransformMatrixStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            shaderBytecodePack = AssetsLoader.GetShaderPack("DepthShadows");
            DepthShadowsVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            DepthShadowsLayout = new InputLayout(
                Engine.Instance.Device,
                ShaderSignature.GetInputSignature(shaderBytecodePack.VertexShaderByteCode),
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 32, 0),
                }
            );
        }

        public override void InitRenderer(Renderer renderer) {
            //Set shaders and layout
            switch (renderer.SpecificType)
            {
                case Renderer.SpecificTypeEnum.SkySphere:
                    renderer.VertexShader = SkySphereVS;
                    renderer.PixelShader = SkySpherePS;
                    renderer.layout = DefferedLayout;
                    break;
                case Renderer.SpecificTypeEnum.ReflectionSphere:
                    renderer.VertexShader = ReflectionSphereVS;
                    renderer.PixelShader = ReflectionSpherePS;
                    renderer.layout = DefferedLayout;
                    break;
                default:
                    renderer.VertexShader = DefferedVS;
                    renderer.PixelShader = DefferedPS;
                    renderer.layout = DefferedLayout;
                    break;
            }

            //Create vertex buffer
            renderer.VertexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Points, VertexBufferDescription);

            //Create index buffer
            renderer.IndexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Indexes, IndexBufferDescription);

            // TODO: Init Render

            Engine.Instance.SetSolidRender();
        }

        private Vector4 floatMaskVal(bool v0, bool v1, bool v2, bool v3) {
            return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
        }

        public override void RenderItem(Renderer renderer) {
            // TODO: Render Item
            if (Engine.Instance.MainCamera != null) {
                Material mref = renderer.RendererMaterial;
                m_TransformMatrix = new TransformMatrixStruct {
                    transformViewProj = Engine.Instance.MainCamera.ViewProjectionMatrix,
                    transformWorld = renderer.gameObject.transform.TransformMatrix,
                    cameraPosition = new Vector4(Engine.Instance.MainCamera.gameObject.transform.Position, 0),
                    textureTiling = renderer.GetPropetyBlock.Tile,
                    textureShift = renderer.GetPropetyBlock.Shift,

                    AlbedoColor = new Vector4(renderer.GetPropetyBlock.AlbedoColor, 1.0f),
                    RoughnessValue = renderer.GetPropetyBlock.RoughnessValue,
                    MetallicValue = renderer.GetPropetyBlock.MetallicValue,

                    optionsMask0 = floatMaskVal(mref.HasAlbedoMap, mref.HasNormalMap, mref.HasRoughnessMap, mref.HasMetallicMap),
                    optionsMask1 = floatMaskVal(mref.HasOcclusionMap, 
                        renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere || renderer.SpecificType == Renderer.SpecificTypeEnum.Unlit, 
                        renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere, false),
                };
            }

            Engine.Instance.Context.InputAssembler.InputLayout = renderer.layout;
            Engine.Instance.Context.UpdateSubresource(ref m_TransformMatrix, ContantBuffer);

            if (renderer.VertexShader != null) {
                Engine.Instance.Context.VertexShader.SetConstantBuffer(0, ContantBuffer);
                Engine.Instance.Context.VertexShader.Set(renderer.VertexShader);
            }

            if (renderer.PixelShader != null) {
                Engine.Instance.Context.PixelShader.SetConstantBuffer(0, ContantBuffer);
                Engine.Instance.Context.PixelShader.Set(renderer.PixelShader);

                // TODO: Change textures behavior
                if (renderer.RendererMaterial.HasSampler) {
                    Engine.Instance.Context.PixelShader.SetSampler(0, renderer.RendererMaterial.MaterialSampler);
                    if (renderer.RendererMaterial.HasAlbedoMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(0, renderer.RendererMaterial.albedoMapView);
                    } else {
                        Engine.Instance.Context.PixelShader.SetShaderResource(0, null);
                    }
                    if (renderer.RendererMaterial.HasNormalMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(1, renderer.RendererMaterial.normalMapView);
                    } else {
                        Engine.Instance.Context.PixelShader.SetShaderResource(1, null);
                    }
                    if (renderer.RendererMaterial.HasRoughnessMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(2, renderer.RendererMaterial.roughnessMapView);
                    } else {
                        Engine.Instance.Context.PixelShader.SetShaderResource(2, null);
                    }
                    if (renderer.RendererMaterial.HasMetallicMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(3, renderer.RendererMaterial.metallicMapView);
                    } else {
                        Engine.Instance.Context.PixelShader.SetShaderResource(3, null);
                    }
                    if (renderer.RendererMaterial.HasOcclusionMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(4, renderer.RendererMaterial.occlusionMapView);
                    } else {
                        Engine.Instance.Context.PixelShader.SetShaderResource(4, null);
                    }
                } else {
                    Engine.Instance.Context.PixelShader.SetSampler(0, null);
                    Engine.Instance.Context.PixelShader.SetShaderResource(0, null);
                    Engine.Instance.Context.PixelShader.SetShaderResource(1, null);
                    Engine.Instance.Context.PixelShader.SetShaderResource(2, null);
                    Engine.Instance.Context.PixelShader.SetShaderResource(3, null);
                    Engine.Instance.Context.PixelShader.SetShaderResource(4, null);
                }

                //Draw Geometry
                Engine.Instance.Context.InputAssembler.PrimitiveTopology = renderer.Topology;
                Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderer.VertexBuffer, 96, 0));
                Engine.Instance.Context.InputAssembler.SetIndexBuffer(renderer.IndexBuffer, Format.R32_UInt, 0);
                Engine.Instance.Context.DrawIndexed(renderer.Geometry.Indexes.Length, 0, 0);
            }
        }

        /*********************************LIGHT SECTION*********************************/
        private VertexShader DepthShadowsVS;
        private InputLayout DepthShadowsLayout;

        private struct TransformMatrixLightStruct
        {
            public Matrix lightViewProjMatrix;
            public Matrix transformWorldMatrix;
        }

        private Matrix m_LightProjMatrix = Matrix.OrthoLH(40f, 40f, 0.1f, 200f);
        private RasterizerState m_light_RasterizerState;

        private TransformMatrixLightStruct lightViewProjMatrix = new TransformMatrixLightStruct();
        public override void InitLight(Light light)
        {
            m_light_RasterizerState = new RasterizerState(Engine.Instance.Device,
               new RasterizerStateDescription
               {
                   CullMode = CullMode.Back,
                   FillMode = FillMode.Solid,
                   DepthBias = 1000,
                   DepthBiasClamp = 0.0f,
                   SlopeScaledDepthBias = 1.75f,
               }
            );

            if (!light.EnableShadows)
            {
                return;
            }

            light.ProjMatrix = m_LightProjMatrix;
            light.VertexShader = DepthShadowsVS;

            light.ContantBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<TransformMatrixLightStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            var depthBufferTextureDescription = new Texture2DDescription
            {
                Format = Format.R32_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = Light.ShadowMapResolution,
                Height = Light.ShadowMapResolution,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Texture2D depthShadowBuffer = new Texture2D(Engine.Instance.Device, depthBufferTextureDescription);
            light.DepthStencilView = new DepthStencilView(Engine.Instance.Device, depthShadowBuffer, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
            });

            light.InputLayout = DepthShadowsLayout;

            light.ShaderResourceView = new ShaderResourceView(
                Engine.Instance.Device,
                depthShadowBuffer,
                new ShaderResourceViewDescription()
                {
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Format = Format.R32_Float,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                });

            light.SamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.ComparisonMinMagMipLinear,
                ComparisonFunction = Comparison.Less,
                BorderColor = Color.White,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
            });
        }

        public override void RenderItemLight(Light light)
        {
            if (!light.EnableShadows)
            {
                return;
            }

            Engine.Instance.Context.Rasterizer.State = m_light_RasterizerState;
            Engine.Instance.Context.ClearDepthStencilView(light.DepthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

            Engine.Instance.Context.VertexShader.Set(light.VertexShader);
            Engine.Instance.Context.PixelShader.Set(null);

            Engine.Instance.Context.Rasterizer.SetViewport(new Viewport(
                0, 0,
                Light.ShadowMapResolution, Light.ShadowMapResolution,
                0.0f, 1.0f
            ));

            Engine.Instance.Context.OutputMerger.SetRenderTargets(light.DepthStencilView, (RenderTargetView)null);
            Engine.Instance.Context.InputAssembler.InputLayout = light.InputLayout;

            Renderer objectRenderer;
            Engine.Instance.GameObjects.ForEach((x) => {
                objectRenderer = x.GetComponent<Renderer>();
                if (objectRenderer != null)
                {
                    lightViewProjMatrix.lightViewProjMatrix = light.ViewProjMatrix;
                    lightViewProjMatrix.transformWorldMatrix = x.transform.TransformMatrix;
                    Engine.Instance.Context.UpdateSubresource(ref lightViewProjMatrix, light.ContantBuffer);
                    Engine.Instance.Context.VertexShader.SetConstantBuffer(0, light.ContantBuffer);

                    Engine.Instance.Context.InputAssembler.PrimitiveTopology = objectRenderer.Topology;
                    Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(objectRenderer.VertexBuffer, 96, 0));
                    Engine.Instance.Context.InputAssembler.SetIndexBuffer(objectRenderer.IndexBuffer, Format.R32_UInt, 0);
                    Engine.Instance.Context.DrawIndexed(objectRenderer.Geometry.Indexes.Length, 0, 0);
                }
            });

            Engine.Instance.ResetTargets();
        }
    }
}