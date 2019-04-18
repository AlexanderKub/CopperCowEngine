using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.Collections.Generic;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using static EngineCore.D3D11.SRITypeEnums;

namespace EngineCore.D3D11
{
    internal class ForwardD3D11RenderPath : BaseD3D11RenderPath
    {
        private CommonStructs.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
        private CommonStructs.ConstBufferPerFrameStruct m_PerFrameConstBuffer;
        
        private InputLayout DepthPassLayout;
        private InputLayout QuadLayoyt;

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
        private Buffer ScreenParameters;

        private struct ScreenParametersStruct
        {
            public int CurrentFPS;
            public Vector3 filler;
            //TODO: new and previous camera matrix
        }

        public override void Init(D3D11RenderBackend backend) {
            base.Init(backend);
            // Create shaders and layouts.

            AssetsLoader.GetShader<VertexShader>("ScreenQuadVS", out ShaderSignature vsSignature);
            QuadLayoyt = new InputLayout(RenderBackend.Device, vsSignature, new InputElement[] {
                new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
            });

            AssetsLoader.GetShader<VertexShader>("ForwardPlusPosOnlyVS", out vsSignature);
            DepthPassLayout = new InputLayout(RenderBackend.Device, vsSignature, new InputElement[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            });
            
            //Create constant buffer 
            PerObjConstantBuffer = new Buffer(
                RenderBackend.Device,
                Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            PerFrameConstantBuffer = new Buffer(
                RenderBackend.Device,
                Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            m_LightBuffer = new LightBufferStruct[] { new LightBufferStruct(), new LightBufferStruct(), new LightBufferStruct() };
            LightBuffer = new Buffer(
                RenderBackend.Device,
                Utilities.SizeOf<LightBufferStruct>() * m_LightBuffer.Length,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            ScreenParameters = new Buffer(
                RenderBackend.Device,
                Utilities.SizeOf<ScreenParametersStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            GetContext.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            GetContext.VertexShader.SetConstantBuffer(2, LightBuffer);

            GetContext.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(2, LightBuffer);
            
            Texture2DDescription textureDescription = new Texture2DDescription()
            {
                Width = RenderBackend.DisplayRef.Width,
                Height = RenderBackend.DisplayRef.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R16G16_Float,
                SampleDescription = new SampleDescription()
                {
                    Count = 1,
                },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };

            RenderTargetViewDescription renderTargetDescription = new RenderTargetViewDescription()
            {
                Format = Format.R16G16_Float,
                Dimension = RenderTargetViewDimension.Texture2D,
            };

            ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Format = Format.R16G16_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };
            shaderResourceDescription.Texture2D.MostDetailedMip = 0;
            shaderResourceDescription.Texture2D.MipLevels = 1;

            textureTarget = new Texture2D(RenderBackend.Device, textureDescription);
            velocityRenderTargetView = new RenderTargetView(RenderBackend.Device, textureTarget, renderTargetDescription);
            velocitySRV = new ShaderResourceView(RenderBackend.Device, textureTarget, shaderResourceDescription);

            textureDescription.Format = m_HDRformar;
            renderTargetDescription.Format = m_HDRformar;
            shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Format = m_HDRformar,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };
            shaderResourceDescription.Texture2D.MostDetailedMip = 0;
            shaderResourceDescription.Texture2D.MipLevels = 1;

            hdrTextureTarget = new Texture2D(RenderBackend.Device, textureDescription);
            hdrSRV = new ShaderResourceView(RenderBackend.Device, hdrTextureTarget, shaderResourceDescription);
            hdrRenderTargetView = new RenderTargetView(RenderBackend.Device, hdrTextureTarget, renderTargetDescription);

            textureDescription.Width = textureDescription.Width / 4;
            textureDescription.Height = textureDescription.Height / 4;
            downSamplingTarget = new Texture2D(RenderBackend.Device, textureDescription);
            downSamplingSRV = new ShaderResourceView(RenderBackend.Device, downSamplingTarget, shaderResourceDescription);
            downSamplingTargetView = new RenderTargetView(RenderBackend.Device, downSamplingTarget, renderTargetDescription);
        }

        Texture2D hdrTextureTarget;
        RenderTargetView hdrRenderTargetView;
        ShaderResourceView hdrSRV;
        Texture2D textureTarget;
        RenderTargetView velocityRenderTargetView;
        ShaderResourceView velocitySRV;

        Texture2D downSamplingTarget;
        RenderTargetView downSamplingTargetView;
        ShaderResourceView downSamplingSRV;

        private enum Pass {
            Wireframe,
            DepthPrePass,
            ColourPass,
        }
        private Pass CurrentPass;
        
        public override void Draw(StandardFrameData frameData)
        {
            // Clear
            DepthStencilView depthStencilView = RenderBackend.DisplayRef.DepthStencilViewRef;
            RenderTargetView renderTargetView = RenderBackend.DisplayRef.RenderTargetViewRef;
            GetContext.ClearRenderTargetView(renderTargetView, Color.Gray);
            GetContext.ClearRenderTargetView(hdrRenderTargetView, Color.Gray);
            GetContext.ClearDepthStencilView(
                depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                0.0f, 0
            );

            if (isWireframe)
            {
                WireframePass(frameData, depthStencilView, renderTargetView);
                return;
            }
            //DepthPrePassWithVelocity(frameData, depthStencilView);
            DepthPrePass(frameData, depthStencilView, renderTargetView);
            // TODO: shadow maps draw pass
            ColourPass(frameData, depthStencilView, renderTargetView);
            ScreenQuadPass(frameData, depthStencilView, renderTargetView);
        }

        bool isWireframe = false;
        private void WireframePass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
        {
            CurrentPass = Pass.Wireframe;
            GetContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            SetDepthStencilState(DepthStencilStates.Greater);

            // Bind buffers
            m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
            {
                Projection = frameData.CamerasList[0].Projection,
                ProjectionInv = Matrix.Invert(frameData.CamerasList[0].Projection),
                CameraPos = frameData.CamerasList[0].Position,
                AlphaTest = 0.5f,
                MaxNumLightsPerTile = (uint)0,
                //TODO: provide lights number
                NumLights = (uint)400,
                WindowHeight = (uint)RenderBackend.DisplayRef.Height,
                WindowWidth = (uint)RenderBackend.DisplayRef.Width,
            };
            GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
            
            SetVertexShader("CommonVS");
            SetPixelShader("ForwardPlusPosTexPS");
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            SetRasterizerState(RasterizerStates.WireframeBackCull);
            m_PerObjectConstBuffer.AlbedoColor = new Vector4(0.8f, 0.5f, 0.5f, 1.0f);

            string MeshName = "";
            foreach (var rendererData in frameData.RenderersList)
            {
                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                RenderBackend.DrawIndexedWrapper(m_CachedMesh.IndexCount, 0, 0);
            }
        }
        
        private List<StandardFrameData.RendererData> DynamicMeshes = new List<StandardFrameData.RendererData>();
        private void DepthPrePassWithVelocity(StandardFrameData frameData, DepthStencilView depthStencilView)
        {
            CurrentPass = Pass.DepthPrePass;
            // Setup targets and states
            GetContext.OutputMerger.SetTargets(depthStencilView, (RenderTargetView)null);
            SetDepthStencilState(DepthStencilStates.Greater);
            GetContext.InputAssembler.InputLayout = DepthPassLayout;

            // Setup vertex shader

            SetVertexShader("ForwardPlusPosOnlyVS");

            // Cleanup pixel shader
            GetContext.PixelShader.Set(null);
            for (int i = 0; i < 2; i++)
            {
                GetContext.PixelShader.SetShaderResources(0, i, (ShaderResourceView)null);
            }
            GetContext.PixelShader.SetSamplers(0, 1, (SamplerState)null);

            // Draw opaque
            string MeshName = "";
            int MaterialQueue = -999999;
            DynamicMeshes.Clear();
            foreach (var rendererData in frameData.RenderersList)
            {
                if (rendererData.IsDynamic)
                {
                    DynamicMeshes.Add(rendererData);
                    continue;
                }
                if (MaterialQueue != rendererData.MaterialQueue)
                {
                    MaterialQueue = rendererData.MaterialQueue;

                    // TODO: correct changing
                    //SetMergerStates(AssetsLoader.LoadMaterial(rendererData.MaterialName).MetaMaterial);*/

                    //SetMergerStates(MaterialQueue);
                }

                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                m_PerObjectConstBuffer.PreviousWorldViewProjMatrix = rendererData.PreviousTransformMatrix * frameData.CamerasList[0].PreviousViewProjection;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                RenderBackend.DrawIndexedWrapper(m_CachedMesh.IndexCount, 0, 0);
            }

            GetContext.OutputMerger.SetTargets(depthStencilView, velocityRenderTargetView);
            GetContext.ClearRenderTargetView(velocityRenderTargetView, Color.Yellow);

            // Setup pixel shader
            SetPixelShader("VelocityPS");

            foreach (var rendererData in DynamicMeshes)
            {
                if (MaterialQueue != rendererData.MaterialQueue)
                {
                    MaterialQueue = rendererData.MaterialQueue;

                    // TODO: correct changing
                    //SetMergerStates(AssetsLoader.LoadMaterial(rendererData.MaterialName).MetaMaterial);*/

                    //SetMergerStates(MaterialQueue);
                }

                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                m_PerObjectConstBuffer.PreviousWorldViewProjMatrix = rendererData.PreviousTransformMatrix * frameData.CamerasList[0].PreviousViewProjection;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                RenderBackend.DrawIndexedWrapper(m_CachedMesh.IndexCount, 0, 0);
            }
            //TODO: Alpha to coverage pre-pass
        }

        private void DepthPrePass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
        {
            CurrentPass = Pass.DepthPrePass;
            // Setup targets and states
            GetContext.OutputMerger.SetTargets(depthStencilView, (RenderTargetView)null);

            SetDepthStencilState(DepthStencilStates.Greater);

            SetRasterizerState(RasterizerStates.SolidBackCull);

            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            // Setup vertex shader
            GetContext.VertexShader.Set(AssetsLoader.GetShader<VertexShader>("ForwardPlusPosOnlyVS"));

            // Cleanup pixel shader
            GetContext.PixelShader.Set(null);
            for (int i = 0; i < 2; i++)
            {
                GetContext.PixelShader.SetShaderResources(0, i, (ShaderResourceView)null);
            }
            GetContext.PixelShader.SetSamplers(0, 1, (SamplerState)null);

            m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
            {
                Projection = frameData.CamerasList[0].Projection,
                ProjectionInv = Matrix.Invert(frameData.CamerasList[0].Projection),
                CameraPos = frameData.CamerasList[0].Position,
                AlphaTest = 0.5f,
                MaxNumLightsPerTile = (uint)0,
                //TODO: provide lights number
                NumLights = (uint)400,
                WindowHeight = (uint)RenderBackend.DisplayRef.Height,
                WindowWidth = (uint)RenderBackend.DisplayRef.Width,
            };
            GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);

            string MeshName = "";
            string MaterialName = "";
            bool IsOpaquePass = true;
            foreach (var rendererData in frameData.RenderersList)
            {
                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                if (MaterialName != rendererData.MaterialName)
                {
                    MaterialName = rendererData.MaterialName;
                    SetMaterial(MaterialName, false);
                    if (IsOpaquePass)
                    {
                        if (CurrentMaterialInstance.MetaMaterial.blendMode == ShaderGraph.MetaMaterial.BlendMode.Masked)
                        {
                            GetContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
                            SetRasterizerState(RasterizerStates.SolidNoneCull);
                            GetContext.PixelShader.SetSampler(0,
                                GetSharedItems.GetSamplerState(SamplerType.AnisotropicWrap));
                            //if (msaa) DepthOnlyAlphaToCoverageState
                            SetBlendState(BlendStates.DepthOnlyAlphaTest);

                            GetContext.VertexShader.Set(AssetsLoader.GetShader<VertexShader>("ForwardPlusPosTexVS"));
                            GetContext.PixelShader.Set(AssetsLoader.GetShader<PixelShader>("ForwardPlusPosTexPS"));
                            GetContext.PixelShader.SetShaderResources(0, 1,
                                GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset));
                            GetContext.PixelShader.SetSamplers(0, 1,
                                GetSharedItems.GetSamplerState(SamplerType.BilinearClamp));
                            IsOpaquePass = false;
                        }
                    }
                    else
                    {
                        GetContext.PixelShader.SetShaderResources(0, 1,
                            GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset));
                    }
                    if (CurrentMaterialInstance.MetaMaterial.blendMode > ShaderGraph.MetaMaterial.BlendMode.Masked)
                    {
                        break; // Break on translucent objects
                    }
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                m_PerObjectConstBuffer.textureTiling = CurrentMaterialInstance.PropetyBlock.Tile;
                m_PerObjectConstBuffer.textureShift = CurrentMaterialInstance.PropetyBlock.Shift;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                RenderBackend.DrawIndexedWrapper(m_CachedMesh.IndexCount, 0, 0);
            }
        }
    
        private void ColourPass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
        {
            CurrentPass = Pass.ColourPass;
            // Setup targets and states
            // renderTargetView
            GetContext.OutputMerger.SetRenderTargets(depthStencilView, hdrRenderTargetView);
            SetDepthStencilState(DepthStencilStates.EqualAndDisableWrite);

            // Setup vertex shader
            SetVertexShader("CommonVS");
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            // Bind buffers
            m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
            {
                Projection = frameData.CamerasList[0].Projection,
                ProjectionInv = Matrix.Invert(frameData.CamerasList[0].Projection),
                CameraPos = frameData.CamerasList[0].Position,
                AlphaTest = 0.5f,
                MaxNumLightsPerTile = (uint)0,
                //TODO: provide lights number
                NumLights = (uint)400,
                WindowHeight = (uint)RenderBackend.DisplayRef.Height,
                WindowWidth = (uint)RenderBackend.DisplayRef.Width,
            };

            int n = frameData.LightsList.Count;
            n = n > 3 ? 3 : n;
            // Lights buffer max x3
            StandardFrameData.LightData light;
            for (int i = 0; i < n; i++)
            {
                light = frameData.LightsList[i];
                m_LightBuffer[i].viewProjMatrix = light.ViewProjection;
                m_LightBuffer[i].lightTint = Vector4.One;
                m_LightBuffer[i].type = (float)light.Type;
                m_LightBuffer[i].position = light.Position;
                m_LightBuffer[i].direction = light.Direction;
                m_LightBuffer[i].distanceSqr = light.Radius * light.Radius;
            }

            GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
            GetContext.UpdateSubresource(m_LightBuffer, LightBuffer);

            // Draw scene
            string MeshName = "";
            string MaterialName = "";
            int MaterialQueue = -999999;
            foreach (var rendererData in frameData.RenderersList)
            {
                if (MaterialName != rendererData.MaterialName)
                {
                    MaterialName = rendererData.MaterialName;
                    SetMaterial(MaterialName, MaterialQueue != rendererData.MaterialQueue);
                    MaterialQueue = rendererData.MaterialQueue;
                }

                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                RenderBackend.DrawIndexedWrapper(m_CachedMesh.IndexCount, 0, 0);
            }
        }

        private ScreenParametersStruct ScreenParametersBuffer;
        private void ScreenQuadPass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
        {
            GetContext.OutputMerger.SetRenderTargets(null, downSamplingTargetView);
            SetDepthStencilState(DepthStencilStates.Greater);
            SetBlendState(BlendStates.Opaque);
            GetContext.Rasterizer.SetViewport(new Viewport(
                0, 0,
                RenderBackend.DisplayRef.Width / 4,
                RenderBackend.DisplayRef.Height / 4,
                0.0f, 1.0f
            ));

            SetVertexShader("ScreenQuadVS");
            //GetContext.VertexShader.SetConstantBuffer(0, null);
            //GetContext.VertexShader.SetConstantBuffer(1, null);
            GetContext.InputAssembler.InputLayout = QuadLayoyt;
            //GetContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            SetPixelShader("DownSamplingPS");
            GetContext.PixelShader.SetConstantBuffer(0, ScreenParameters);
            ScreenParametersBuffer.CurrentFPS = (int)(1.0f / RenderBackend.EngineRef.Time.DeltaTime);
            GetContext.UpdateSubresource(ref ScreenParametersBuffer, ScreenParameters);
            GetContext.PixelShader.SetShaderResource(0, hdrSRV);
            GetContext.PixelShader.SetShaderResource(1, null);
            GetContext.PixelShader.SetShaderResource(2, null);
            GetContext.PixelShader.SetSampler(0, RenderBackend.SharedRenderItems.GetSamplerState(SamplerType.BilinearClamp));
            RenderBackend.DrawWrapper(4, 0);

            GetContext.Rasterizer.SetViewport(new Viewport(
                0, 0,
                RenderBackend.DisplayRef.Width,
                RenderBackend.DisplayRef.Height,
                0.0f, 1.0f
            ));
            GetContext.OutputMerger.SetRenderTargets(null, renderTargetView);

            SetPixelShader("ScreenQuadPS");
            GetContext.PixelShader.SetConstantBuffer(0, ScreenParameters);
            ScreenParametersBuffer.CurrentFPS = (int)(1.0f / RenderBackend.EngineRef.Time.DeltaTime);
            GetContext.UpdateSubresource(ref ScreenParametersBuffer, ScreenParameters);
            GetContext.PixelShader.SetShaderResource(0, hdrSRV);
            GetContext.PixelShader.SetShaderResource(1, velocitySRV);
            GetContext.PixelShader.SetShaderResource(2, RenderBackend.DisplayRef.DepthStencilSRVRef);
            GetContext.PixelShader.SetSampler(0, RenderBackend.SharedRenderItems.GetSamplerState(SamplerType.BilinearClamp));
            RenderBackend.DrawWrapper(4, 0);

            GetContext.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
        }

        Material CurrentMaterialInstance;
        private void SetMaterial(string materialName, bool changeMergerStates)
        {
            if (materialName == "SkySphereMaterial"){
                CurrentMaterialInstance = Material.GetSkySphereMaterial();
            } else {
                CurrentMaterialInstance = AssetsLoader.LoadMaterial(materialName);
            }

            if (changeMergerStates)
            {
                SetMergerStates(CurrentMaterialInstance.MetaMaterial);
            }
            // TODO: shader selector

            m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct
            {
                textureTiling = CurrentMaterialInstance.PropetyBlock.Tile,
                textureShift = CurrentMaterialInstance.PropetyBlock.Shift,

                AlbedoColor = new Vector4(CurrentMaterialInstance.PropetyBlock.AlbedoColor, CurrentMaterialInstance.PropetyBlock.AlphaValue),
                RoughnessValue = CurrentMaterialInstance.PropetyBlock.RoughnessValue,
                MetallicValue = CurrentMaterialInstance.PropetyBlock.MetallicValue,

                optionsMask0 = floatMaskVal(CurrentMaterialInstance.HasAlbedoMap, CurrentMaterialInstance.HasNormalMap, CurrentMaterialInstance.HasRoughnessMap, CurrentMaterialInstance.HasMetallicMap),
                optionsMask1 = floatMaskVal(CurrentMaterialInstance.HasOcclusionMap, false, false, false),
                                /*renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere || renderer.SpecificType == Renderer.SpecificTypeEnum.Unlit
                                    || renderer.SpecificType == Renderer.SpecificTypeEnum.Wireframe,
                                renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere, false),*/
                filler = Vector2.Zero,
            };

            if (materialName == "SkySphereMaterial")
            {
                m_PerObjectConstBuffer.optionsMask1 = floatMaskVal(CurrentMaterialInstance.HasOcclusionMap, true, true, false);
            }

            // TODO: Change textures binding
            if (CurrentMaterialInstance.HasSampler)
            {
                GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(CurrentMaterialInstance.GetSamplerType));
                ShaderResourceView[] textures = new ShaderResourceView[]
                {
                    CurrentMaterialInstance.HasAlbedoMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset) : null,
                    CurrentMaterialInstance.HasNormalMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.NormalMapAsset) : null,
                    CurrentMaterialInstance.HasRoughnessMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.RoughnessMapAsset) : null,
                    CurrentMaterialInstance.HasMetallicMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.MetallicMapAsset) : null,
                    CurrentMaterialInstance.HasOcclusionMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.OcclusionMapAsset) : null,
                };
                GetContext.PixelShader.SetShaderResources(0, 5, textures);
            }
            else
            {
                GetContext.PixelShader.SetSampler(0, null);
                GetContext.PixelShader.SetShaderResources(0, 5, (ShaderResourceView[])null);
            }
            // PreFiltered
            GetContext.PixelShader.SetShaderResource(5, GetSharedItems.PreFilteredMap);
            // Irradiance
            GetContext.PixelShader.SetShaderResource(6, RenderBackend.SharedRenderItems.IrradianceMap);

            //Shadow maps
            /*GetContext.PixelShader.SetSampler(1,
                Engine.Instance.MainLight.SamplerState);
            GetContext.PixelShader.SetShaderResource(7,
                Engine.Instance.MainLight.ShaderResourceView);*/
            
            if (materialName == "SkySphereMaterial") {
                SetPixelShader("FwdSkySpherePS");
            } else if (CurrentMaterialInstance.MetaMaterial.blendMode == ShaderGraph.MetaMaterial.BlendMode.Translucent) {
                SetPixelShader("TestShader");
            } else {
                SetPixelShader("PBRForwardPS");
            }
        }
        
        private SharedRenderItemsStorage.CachedMesh m_CachedMesh;
        private void SetMesh(string meshName)
        {
            m_CachedMesh = RenderBackend.SharedRenderItems.GetMesh(meshName);

            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_CachedMesh.vertexBuffer, 96, 0));
            GetContext.InputAssembler.SetIndexBuffer(m_CachedMesh.indexBuffer, Format.R32_UInt, 0);
            GetContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
        
        //TODO: cache old meta values
        private void SetMergerStates(ShaderGraph.MetaMaterial meta)
        {
            switch (meta.blendMode)
            {
                case ShaderGraph.MetaMaterial.BlendMode.Opaque:
                    if (CurrentPass == Pass.DepthPrePass)
                    {
                        return;
                    }
                    SetDepthStencilState(DepthStencilStates.EqualAndDisableWrite);
                    SetBlendState(BlendStates.Opaque);
                    break;
                case ShaderGraph.MetaMaterial.BlendMode.Masked:
                    SetDepthStencilState(DepthStencilStates.EqualAndDisableWrite);
                    SetBlendState(BlendStates.AlphaEnabledBlending);
                    break;
                case ShaderGraph.MetaMaterial.BlendMode.Translucent:
                    SetDepthStencilState(DepthStencilStates.GreaterAndDisableWrite);
                    SetBlendState(BlendStates.AlphaEnabledBlending);
                    break;
                case ShaderGraph.MetaMaterial.BlendMode.Additive:
                    break;
                case ShaderGraph.MetaMaterial.BlendMode.Modulate:
                    break;
                default:
                    break;
            }

            switch (meta.cullMode)
            {
                case ShaderGraph.MetaMaterial.CullMode.Front:
                    SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeFrontCull
                        : RasterizerStates.SolidFrontCull);
                    break;
                case ShaderGraph.MetaMaterial.CullMode.Back:
                    SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeBackCull
                        : RasterizerStates.SolidBackCull);
                    break;
                case ShaderGraph.MetaMaterial.CullMode.None:
                    SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeNoneCull
                        : RasterizerStates.SolidNoneCull);
                    break;
                default:
                    break;
            }
        }

        //private Format m_HDRformar = Format.R32G32B32A32_Float;
        private Format m_HDRformar = Format.R16G16B16A16_Float;
        public override void Resize() {
            Texture2DDescription textureDescription = new Texture2DDescription()
            {
                Width = RenderBackend.DisplayRef.Width,
                Height = RenderBackend.DisplayRef.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R16G16_Float,
                SampleDescription = new SampleDescription()
                {
                    Count = 1,
                },
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };

            RenderTargetViewDescription renderTargetDescription = new RenderTargetViewDescription()
            {
                Format = Format.R16G16_Float,
                Dimension = RenderTargetViewDimension.Texture2D,
            };

            ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Format = Format.R16G16_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };
            shaderResourceDescription.Texture2D.MostDetailedMip = 0;
            shaderResourceDescription.Texture2D.MipLevels = 1;

            textureTarget?.Dispose();
            velocityRenderTargetView?.Dispose();
            velocitySRV?.Dispose();

            textureTarget = new Texture2D(RenderBackend.Device, textureDescription);
            velocityRenderTargetView = new RenderTargetView(RenderBackend.Device, textureTarget, renderTargetDescription);
            velocitySRV = new ShaderResourceView(RenderBackend.Device, textureTarget, shaderResourceDescription);

            textureDescription.Format = m_HDRformar;
            renderTargetDescription.Format = m_HDRformar;
            shaderResourceDescription = new ShaderResourceViewDescription()
            {
                Format = m_HDRformar,
                Dimension = ShaderResourceViewDimension.Texture2D,
            };
            shaderResourceDescription.Texture2D.MostDetailedMip = 0;
            shaderResourceDescription.Texture2D.MipLevels = 1;

            hdrTextureTarget?.Dispose();
            hdrRenderTargetView?.Dispose();
            hdrSRV?.Dispose();

            hdrTextureTarget = new Texture2D(RenderBackend.Device, textureDescription);
            hdrSRV = new ShaderResourceView(RenderBackend.Device, hdrTextureTarget, shaderResourceDescription);
            hdrRenderTargetView = new RenderTargetView(RenderBackend.Device, hdrTextureTarget, renderTargetDescription);
        }

        public override void Dispose()
        {
            PerObjConstantBuffer?.Dispose();
            PerFrameConstantBuffer?.Dispose();
            LightBuffer?.Dispose();
            textureTarget?.Dispose();
            velocityRenderTargetView?.Dispose();
            hdrTextureTarget?.Dispose();
            hdrRenderTargetView?.Dispose();
            hdrSRV?.Dispose();
            velocitySRV?.Dispose();
            downSamplingTarget?.Dispose();
            downSamplingTargetView?.Dispose();
            downSamplingSRV?.Dispose();
        }

        private Vector4 floatMaskVal(bool v0, bool v1, bool v2, bool v3)
        {
            return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
        }
    }
}
