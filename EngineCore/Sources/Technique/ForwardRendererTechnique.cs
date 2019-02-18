using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore.Technique
{
    internal class ForwardRendererTechnique: BaseRendererTechnique
    {

        private struct TransformMatrixStruct
        {
            public Matrix transformViewProj;
            public Matrix transformWorld;
            public Matrix lightViewProjMatrix;
            public Vector4 eyeWorldPosition;
            public Vector2 textureTiling;
            public Vector2 textureShift;
        }

        internal struct LightBufferStruct
        {
            public Vector4 ambientColor;
            public Vector4 diffuseColor;
            public Vector4 specularColor;
            public float type;
            public Vector3 position;
            public Vector3 direction;
            public float distanceSqr;
            public float hasNormalMap;
            public float hasRoughnessMap;
            public float hasAOMap;
            public float recieveShadows;
        }

        private ShaderBytecodePack shaderBytecodePack;
        private VertexShader VertexColorVS;
        private PixelShader VertexColorPS;
        private InputLayout VertexColorLayout;

        private VertexShader TexturedVS;
        private PixelShader TexturedPS;
        private InputLayout TexturedLayout;

        private VertexShader DepthShadowsVS;
        private InputLayout DepthShadowsLayout;

        private BufferDescription VertexBufferDescription;
        private BufferDescription IndexBufferDescription;

        private Buffer ContantBuffer;
        private Buffer LightBuffer;

        public override void Init() {
            //create shaders and layouts
            shaderBytecodePack = AssetsLoader.GetShaderPack("VertexColor");
            VertexColorVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            VertexColorPS = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);
            VertexColorLayout = new InputLayout(
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

            shaderBytecodePack = AssetsLoader.GetShaderPack("Textured");
            TexturedVS = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);
            TexturedPS = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);
            TexturedLayout = new InputLayout(
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

            //Create buffer descriptions
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

            //Create light buffer 
            LightBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<LightBufferStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
        }

        public override void Draw() {
            Engine.Instance.MainCamera.Update();
            //For shadow map
            //Engine.Instance.MainLight.Draw();
            DepthStencilView depthStencilView = Engine.Instance.DisplayRef.DepthStencilViewRef;
            RenderTargetView renderTargetView = Engine.Instance.DisplayRef.RenderTargetViewRef;

            Engine.Instance.Context.ClearDepthStencilView(
                depthStencilView, 
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 
                1f, 0
            );

            Engine.Instance.Context.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            Engine.Instance.Context.ClearRenderTargetView(renderTargetView, Engine.Instance.ClearColor);

            Engine.Instance.GameObjects.ForEach((x) => {
                x.Draw();
            });

            Engine.Instance.MainLight.Draw();
        }

        public override void Resize() {
            Engine.Instance.ResetTargets();
        }

        public override void InitRenderer(Renderer renderer) {
            //Set shaders and layout
            if (renderer.RendererMaterial.HasAlbedoMap) {
                renderer.VertexShader = TexturedVS;
                renderer.PixelShader = TexturedPS;
                renderer.layout = TexturedLayout;
            } else {
                renderer.VertexShader = VertexColorVS;
                renderer.PixelShader = VertexColorPS;
                renderer.layout = VertexColorLayout;
            }
            
            //Create vertex buffer
            renderer.VertexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Points, VertexBufferDescription);

            //Create index buffer
            renderer.IndexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Indexes, IndexBufferDescription);
        }

        private TransformMatrixStruct m_TransformMatrix;
        private LightBufferStruct m_lightInfo;

        public override void RenderItem(Renderer renderer) {
            renderer.DrawToStreamOutput();

            if (Engine.Instance.MainCamera != null) {
                m_TransformMatrix = new TransformMatrixStruct {
                    transformViewProj = Engine.Instance.MainCamera.ViewProjectionMatrix,
                    transformWorld = renderer.gameObject.transform.TransformMatrix,
                    lightViewProjMatrix = Engine.Instance.MainLight != null
                        ? Engine.Instance.MainLight.ViewProjMatrix : Matrix.Zero,
                    eyeWorldPosition = new Vector4(Engine.Instance.MainCamera.gameObject.transform.Position, 0),
                    textureTiling = renderer.GetPropetyBlock.Tile,
                    textureShift = renderer.GetPropetyBlock.Shift,
                };
            }
            
            m_lightInfo = new LightBufferStruct() {
                ambientColor = Engine.Instance.MainLight.ambientColor,
                diffuseColor = Engine.Instance.MainLight.diffuseColor,
                specularColor = Engine.Instance.MainLight.specularColor,
                type = (float)Engine.Instance.MainLight.Type,
                position = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.gameObject.transform.Position : Vector3.Zero,
                direction = Engine.Instance.MainLight != null
                    ? Matrix.RotationQuaternion(Engine.Instance.MainLight.gameObject.transform.Rotation).Forward
                    : Vector3.Zero,
                distanceSqr = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.radius * Engine.Instance.MainLight.radius : 0,
                hasNormalMap = renderer.RendererMaterial.HasNormalMap ? 1 : 0,
                hasRoughnessMap = renderer.RendererMaterial.HasRoughnessMap ? 1 : 0,
                hasAOMap = renderer.RendererMaterial.HasOcclusionMap ? 1 : 0,
                recieveShadows = 0,
            };

            Engine.Instance.Context.GeometryShader.Set(null);
            Engine.Instance.Context.InputAssembler.InputLayout = renderer.layout;

            Engine.Instance.Context.UpdateSubresource(ref m_TransformMatrix, ContantBuffer);
            Engine.Instance.Context.UpdateSubresource(ref m_lightInfo, LightBuffer);

            if (renderer.VertexShader != null) {
                Engine.Instance.Context.VertexShader.SetConstantBuffer(0, ContantBuffer);
                Engine.Instance.Context.VertexShader.SetConstantBuffer(1, LightBuffer);
                Engine.Instance.Context.VertexShader.Set(renderer.VertexShader);
            }

            if (renderer.PixelShader != null) {
                Engine.Instance.Context.PixelShader.SetConstantBuffer(0, ContantBuffer);
                Engine.Instance.Context.PixelShader.SetConstantBuffer(1, LightBuffer);
                Engine.Instance.Context.PixelShader.Set(renderer.PixelShader);

                Engine.Instance.Context.PixelShader.SetSampler(0,
                    Engine.Instance.MainLight.SamplerState);
                Engine.Instance.Context.PixelShader.SetShaderResource(0,
                    Engine.Instance.MainLight.ShaderResourceView);

                if (renderer.RendererMaterial.HasSampler) {
                    Engine.Instance.Context.PixelShader.SetSampler(1,
                        renderer.RendererMaterial.MaterialSampler);
                    if (renderer.RendererMaterial.HasAlbedoMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(1,
                            renderer.RendererMaterial.albedoMapView);
                    }
                    if (renderer.RendererMaterial.HasNormalMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(2,
                            renderer.RendererMaterial.normalMapView);
                    }
                    if (renderer.RendererMaterial.HasRoughnessMap) {
                        Engine.Instance.Context.PixelShader.SetShaderResource(3,
                            renderer.RendererMaterial.roughnessMapView);
                    }
                    if (renderer.RendererMaterial.HasMetallicMap)
                    {
                        Engine.Instance.Context.PixelShader.SetShaderResource(4,
                            renderer.RendererMaterial.metallicMapView);
                    }
                    if (renderer.RendererMaterial.HasOcclusionMap)
                    {
                        Engine.Instance.Context.PixelShader.SetShaderResource(5,
                            renderer.RendererMaterial.occlusionMapView);
                    }
                }
            }
            //Draw Geometry
            Engine.Instance.Context.InputAssembler.PrimitiveTopology = renderer.Topology;
            Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderer.VertexBuffer, 96, 0));
            Engine.Instance.Context.InputAssembler.SetIndexBuffer(renderer.IndexBuffer, Format.R32_UInt, 0);
            Engine.Instance.Context.DrawIndexed(renderer.Geometry.Indexes.Length, 0, 0);
        }

        /*********************************LIGHT SECTION*********************************/
        private struct TransformMatrixLightStruct
        {
            public Matrix lightViewProjMatrix;
            public Matrix transformWorldMatrix;
        }

        private Matrix m_LightProjMatrix = Matrix.OrthoLH(40f, 40f, 0.1f, 200f);
        private RasterizerState m_light_RasterizerState;

        private TransformMatrixLightStruct lightViewProjMatrix = new TransformMatrixLightStruct();
        public override void InitLight(Light light) {
            m_light_RasterizerState = new RasterizerState(Engine.Instance.Device,
               new RasterizerStateDescription {
                   CullMode = CullMode.Back,
                   FillMode = FillMode.Solid,
                   DepthBias = 1000,
                   DepthBiasClamp = 0.0f,
                   SlopeScaledDepthBias = 1.5f,
               }
            );

            if (!light.EnableShadows) {
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

            var depthBufferTextureDescription = new Texture2DDescription {
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
            light.DepthStencilView = new DepthStencilView(Engine.Instance.Device, depthShadowBuffer, new DepthStencilViewDescription() {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
            });

            light.InputLayout = DepthShadowsLayout;

            light.ShaderResourceView = new ShaderResourceView(
                Engine.Instance.Device,
                depthShadowBuffer,
                new ShaderResourceViewDescription() {
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Format = Format.R32_Float,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource() {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                });

            light.SamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription() {
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

        public override void RenderItemLight(Light light) {
            if (!light.EnableShadows) {
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
                if (objectRenderer != null) {
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
