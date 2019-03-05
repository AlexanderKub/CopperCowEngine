﻿using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore.RenderTechnique
{
    internal class ForwardRendererTechnique: BaseRendererTechnique
    {
        private CommonStructs.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
        private CommonStructs.ConstBufferPerFrameStruct m_PerFrameConstBuffer;
        
        private VertexShader CommonVS;
        private PixelShader TexturedPS;
        private PixelShader SkySpherePS;
        private InputLayout TexturedLayout;

        private BufferDescription VertexBufferDescription;
        private BufferDescription IndexBufferDescription;

        internal struct LightBufferStruct
        {
            public Matrix viewProjMatrix;
            public Vector4 lightTint;
            public float type;
            public Vector3 position;
            public Vector3 direction;
            public float distanceSqr;
        }
        private LightBufferStruct[] m_LightBuffer;
        private Buffer PerObjConstantBuffer;
        private Buffer PerFrameConstantBuffer;
        private Buffer LightBuffer;

        public override void Init() {
            //create shaders and layouts
            CommonVS = AssetsLoader.GetShader<VertexShader>("CommonVS", out ShaderSignature vsSignature);
            TexturedLayout = new InputLayout(
                Engine.Instance.Device,
                vsSignature,
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                }
            );

            TexturedPS = AssetsLoader.GetShader<PixelShader>("PBRForwardPS");
            SkySpherePS = AssetsLoader.GetShader<PixelShader>("FwdSkySpherePS");

            /*DepthShadowsVS = AssetsLoader.GetShader<VertexShader>("DepthShadowsVS", out ShaderSignature dsvsSignature);
            DepthShadowsLayout = new InputLayout(
                Engine.Instance.Device,
                dsvsSignature,
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 32, 0),
                }
            );*/

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
            PerObjConstantBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
            PerFrameConstantBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
            m_LightBuffer = new LightBufferStruct[] { new LightBufferStruct(), new LightBufferStruct(), new LightBufferStruct() };
            LightBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<LightBufferStruct>() * m_LightBuffer.Length,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
        }

        public override void Draw() {
            Engine.Instance.MainCamera.Update();
            Engine.Instance.MainLight.Draw();

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
        }

        public override void Resize() {
            Engine.Instance.ResetTargets();
        }

        public override void InitRenderer(Renderer renderer) {
            //Set shaders and layout
            renderer.VertexShader = CommonVS;
            renderer.PixelShader = TexturedPS;
            renderer.layout = TexturedLayout;

            if (renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere) {
                renderer.PixelShader = SkySpherePS;
            }
            //Create vertex buffer
            renderer.VertexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Points, VertexBufferDescription);

            //Create index buffer
            renderer.IndexBuffer = Buffer.Create(Engine.Instance.Device, renderer.Geometry.Indexes, IndexBufferDescription);
        }

        private Vector4 floatMaskVal(bool v0, bool v1, bool v2, bool v3) {
            return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
        }

        public override void RenderItem(Renderer renderer) {
            if (renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere) {
            }

            if (Engine.Instance.MainCamera) {
                Material mref = renderer.RendererMaterial;
                m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct
                {
                    WorldMatrix = renderer.gameObject.transform.TransformMatrix,
                    WorldViewMatrix = renderer.gameObject.transform.TransformMatrix * Engine.Instance.MainCamera.View,
                    WorldViewProjMatrix = renderer.gameObject.transform.TransformMatrix * Engine.Instance.MainCamera.ViewProjectionMatrix,
                    textureTiling = renderer.GetPropetyBlock.Tile,
                    textureShift = renderer.GetPropetyBlock.Shift,

                    AlbedoColor = new Vector4(renderer.GetPropetyBlock.AlbedoColor, renderer.GetPropetyBlock.AlphaValue),
                    RoughnessValue = renderer.GetPropetyBlock.RoughnessValue,
                    MetallicValue = renderer.GetPropetyBlock.MetallicValue,

                    optionsMask0 = floatMaskVal(mref.HasAlbedoMap, mref.HasNormalMap, mref.HasRoughnessMap, mref.HasMetallicMap),
                    optionsMask1 = floatMaskVal(mref.HasOcclusionMap, 
                        renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere || renderer.SpecificType == Renderer.SpecificTypeEnum.Unlit, 
                        renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere, false),
                    filler = Vector2.Zero,
                };
                m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
                {
                    Projection = Engine.Instance.MainCamera.Projection,
                    ProjectionInv = Matrix.Invert(Engine.Instance.MainCamera.Projection),
                    CameraPos = Engine.Instance.MainCamera.gameObject.transform.WorldPosition,
                    AlphaTest = 0.5f,
                    MaxNumLightsPerTile = (uint)0,
                    //TODO: provide lights number
                    NumLights = (uint)400,
                    WindowHeight = (uint)Engine.Instance.DisplayRef.Height,
                    WindowWidth = (uint)Engine.Instance.DisplayRef.Width,
                };
            }

            if (Engine.Instance.MainLight)
            {
                m_LightBuffer[0].viewProjMatrix = Engine.Instance.MainLight.ViewProjMatrix;
                m_LightBuffer[0].lightTint = Engine.Instance.MainLight.LightColor;
                m_LightBuffer[0].type = (float)Engine.Instance.MainLight.Type;
                m_LightBuffer[0].position = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.gameObject.transform.WorldPosition : Vector3.Zero;
                m_LightBuffer[0].direction = Engine.Instance.MainLight != null
                    ? Matrix.RotationQuaternion(Engine.Instance.MainLight.gameObject.transform.WorldRotation).Forward
                    : Vector3.Zero;
                m_LightBuffer[0].distanceSqr = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.radius * Engine.Instance.MainLight.radius : 0;
            }

            Engine.Instance.Context.InputAssembler.InputLayout = renderer.layout;
            Engine.Instance.Context.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);
            Engine.Instance.Context.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
            Engine.Instance.Context.UpdateSubresource(m_LightBuffer, LightBuffer);

            if (renderer.VertexShader != null)
            {
                Engine.Instance.Context.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
                Engine.Instance.Context.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
                Engine.Instance.Context.VertexShader.SetConstantBuffer(2, LightBuffer);
                Engine.Instance.Context.VertexShader.Set(renderer.VertexShader);
            }

            if (renderer.PixelShader != null) {
                Engine.Instance.Context.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
                Engine.Instance.Context.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
                Engine.Instance.Context.PixelShader.SetConstantBuffer(2, LightBuffer);
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
                //Radiance
                Engine.Instance.Context.PixelShader.SetShaderResource(5, Material.GetSkySphereMaterial().albedoMapView);
                //Irradiance
                Engine.Instance.Context.PixelShader.SetShaderResource(6, Material.IrradianceMap);

                Engine.Instance.Context.PixelShader.SetSampler(1,
                    Engine.Instance.MainLight.SamplerState);
                Engine.Instance.Context.PixelShader.SetShaderResource(7,
                    Engine.Instance.MainLight.ShaderResourceView);

                //Draw Geometry
                Engine.Instance.Context.InputAssembler.PrimitiveTopology = renderer.Topology;
                Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(renderer.VertexBuffer, 96, 0));
                Engine.Instance.Context.InputAssembler.SetIndexBuffer(renderer.IndexBuffer, Format.R32_UInt, 0);
                Engine.Instance.Context.DrawIndexed(renderer.Geometry.Indexes.Length, 0, 0);
            }
        }

        /*********************************LIGHT SECTION*********************************/
        private struct TransformMatrixLightStruct
        {
            public Matrix lightViewProjMatrix;
            public Matrix transformWorldMatrix;
        }

        private RasterizerState m_light_RasterizerState;

        private TransformMatrixLightStruct lightViewProjMatrix = new TransformMatrixLightStruct();
        public override void InitLight(Light light)
        {
            return;
            if (!light.EnableShadows) {
                return;
            }

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
                Format = Format.R24G8_Typeless,
                Width = Light.ShadowMapResolution,
                Height = Light.ShadowMapResolution,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Texture2D depthShadowBuffer = new Texture2D(Engine.Instance.Device, depthBufferTextureDescription);
            DepthStencilViewDescription depthStencilViewDesc = new DepthStencilViewDescription()
            {
                Format = Format.D24_UNorm_S8_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
            };
            depthStencilViewDesc.Texture2D.MipSlice = 0;
            light.DepthStencilView = new DepthStencilView(Engine.Instance.Device, depthShadowBuffer, depthStencilViewDesc);

            light.ShaderResourceView = new ShaderResourceView(
                Engine.Instance.Device,
                depthShadowBuffer,
                new ShaderResourceViewDescription()
                {
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Format = Format.R24_UNorm_X8_Typeless,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0,
                    },
                }
            );

            light.SamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                BorderColor = Color.Black,
                Filter = Filter.ComparisonMinMagMipLinear,
                ComparisonFunction = Comparison.Less,
            });

            m_light_RasterizerState = new RasterizerState(Engine.Instance.Device,
               new RasterizerStateDescription
               {
                   CullMode = CullMode.Back,
                   FillMode = FillMode.Solid,
                   DepthBias = 10000,
                   DepthBiasClamp = 1.75f,
                   SlopeScaledDepthBias = 0.25f,
               }
            );
        }

        public override void RenderItemLight(Light light) {
            return;
            if (!light.EnableShadows) {
                return;
            }

            Engine.Instance.Context.Rasterizer.State = m_light_RasterizerState;
            Engine.Instance.Context.Rasterizer.SetViewport(new Viewport(
                0, 0,
                Light.ShadowMapResolution, Light.ShadowMapResolution,
                0.0f, 1.0f
            ));

            Engine.Instance.Context.InputAssembler.InputLayout = light.InputLayout;
            Engine.Instance.Context.VertexShader.Set(light.VertexShader);
            Engine.Instance.Context.PixelShader.Set(null);

            Engine.Instance.Context.ClearDepthStencilView(light.DepthStencilView, 
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);
            Engine.Instance.Context.OutputMerger.SetRenderTargets(light.DepthStencilView, (RenderTargetView)null);

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