using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EngineCore.D3D11.SRITypeEnums;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EngineCore.D3D11
{
    public class PostProcessSettings
    {
        public bool MotionBlurEnabled;
    }

    internal class ForwardBaseD3D11RenderPath : BaseD3D11RenderPath
    {
        protected PostProcessSettings postProcessSettings;

        #region Targets
        SharedRenderItemsStorage.RenderTargetPack VelocityTarget;
        SharedRenderItemsStorage.RenderTargetPack ScreenQuadTarget;
        SharedRenderItemsStorage.DepthStencilTargetPack ShadowMapsAtlasDepthTarget;
        SharedRenderItemsStorage.RenderTargetPack ShadowMapsAtlasTarget;

        private int ShadowAtlasSize = 2048;//8192;
        protected virtual void InitTargets(int samples)
        {
            if (EnabledHDR)
            {
                ScreenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadHDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            }
            else
            {
                ScreenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadLDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);
            }

            if (postProcessSettings.MotionBlurEnabled)
            {
                VelocityTarget = GetSharedItems.CreateRenderTarget("Velocity",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16_Float, samples);
            }
            ShadowMapsAtlasTarget = GetSharedItems.CreateRenderTarget("ShadowMapsAtlas",
                    ShadowAtlasSize, ShadowAtlasSize, Format.R32G32B32A32_Float, 1);
            ShadowMapsAtlasDepthTarget = GetSharedItems.CreateDepthRenderTarget("ShadowMapsAtlas", 
                ShadowAtlasSize, ShadowAtlasSize, 1);
        }

        public override void Resize()
        {
            ScreenQuadTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            VelocityTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
        }
        #endregion

        #region Buffers
        private CommonStructs.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
        private CommonStructs.ConstBufferPerFrameStruct m_PerFrameConstBuffer;
        private CommonStructs.ConstBufferShadowMapLightStruct[] m_ShadowLightsDataBuffer;
        private CommonStructs.ConstBufferShadowDepthStruct m_PerObjectShadowPassConstBuffer;

        protected Buffer PerObjConstantBuffer;
        protected Buffer PerFrameConstantBuffer;
        protected Buffer ShadowLightsDataBuffer;
        protected Buffer PerObjectShadowPassConstBuffer;
        protected InputLayout QuadLayoyt;


        protected virtual void CreateBuffers()
        {
            BufferDescription bufferDescription = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>();
            ToDispose(PerObjConstantBuffer = new Buffer(GetDevice, bufferDescription));

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>();
            ToDispose(PerFrameConstantBuffer = new Buffer(GetDevice, bufferDescription));

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferShadowDepthStruct>();
            ToDispose(PerObjectShadowPassConstBuffer = new Buffer(GetDevice, bufferDescription));

            m_ShadowLightsDataBuffer = new CommonStructs.ConstBufferShadowMapLightStruct[3];
            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferDirLightStruct>() * 3;
            ToDispose(ShadowLightsDataBuffer = new Buffer(GetDevice, bufferDescription));

            AssetsLoader.GetShader<VertexShader>("ScreenQuadVS", out ShaderSignature vsSignature);
            ToDispose(QuadLayoyt = new InputLayout(GetDevice, vsSignature, new InputElement[] {
                new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
            }));
        }

        public override void Dispose()
        {
            base.Dispose();
        }
        #endregion

        public override void Init(D3D11RenderBackend renderBackend)
        {
            postProcessSettings = new PostProcessSettings()
            {
                MotionBlurEnabled = false,
            };
            base.Init(renderBackend);
            int samples = 1;
            switch (renderBackend.EngineRef.CurrentConfig.EnableMSAA) {
                case Engine.EngineConfiguration.MSAAEnabled.x4:
                    samples = 4;
                    break;
                case Engine.EngineConfiguration.MSAAEnabled.x8:
                    samples = 8;
                    break;
            }
            InitTargets(samples);
            CreateBuffers();
        }

        public override void Draw(StandardFrameData frameData)
        {
            ShadowMapsPass(frameData);
            DepthPrePass(frameData);
            ColourPass(frameData);
            ScreenQuadPass(frameData);
        }

        protected enum Pass
        {
            ShadowMapsPass,
            DepthPrePass,
            ColourPass,
            ScreenQuadPass
        }
        protected Pass CurrentPass;

        protected void ShadowMapsPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.ShadowMapsPass;

            GetContext.ClearRenderTargetView(ShadowMapsAtlasTarget.View, Color.White);
            GetContext.ClearDepthStencilView(
                ShadowMapsAtlasDepthTarget.View,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f, 0
            );
            GetContext.OutputMerger.SetTargets(ShadowMapsAtlasDepthTarget.View, ShadowMapsAtlasTarget.View);

            SetDepthStencilState(DepthStencilStates.Less);
            // TODO: select right cull mode
            SetRasterizerState(RasterizerStates.SolidFrontCull);

            Viewport atlasViewport = new Viewport(0, 0, ShadowAtlasSize, ShadowAtlasSize, 0.0f, 1.0f);
            GetContext.Rasterizer.SetViewport(atlasViewport);

            SetVertexShader("DepthShadowsVS");
            GetContext.VertexShader.SetConstantBuffer(0, PerObjectShadowPassConstBuffer);
            SetPixelShader("DepthShadowsPS");
            //SetNullPixelShader();

            string MeshName = "";
            string MaterialName = "";
            bool IsMaskedSubPass = false;

            StandardFrameData.RendererData rendererData;
            foreach (var light in frameData.LightsList)
            {
                if (!light.IsCastShadows)
                {
                    continue;
                }
                foreach (int index in frameData.PerLightRenderersList[light.index])
                {
                    rendererData = frameData.RenderersList[index];

                    if (MeshName != rendererData.MeshName)
                    {
                        MeshName = rendererData.MeshName;
                        SetMesh(MeshName);
                    }

                    if (MaterialName != rendererData.MaterialName)
                    {
                        MaterialName = rendererData.MaterialName;
                        SetMaterial(MaterialName);
                        /*
                        if (!IsMaskedSubPass)
                        {
                            if (m_CachedMaterial.MetaMaterial.blendMode == ShaderGraph.MetaMaterial.BlendMode.Masked)
                            {
                                GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, GetDisplay.RenderTargetViewRef);
                                if (postProcessSettings.UsingMSAA) {
                                    SetBlendState(SharedRenderItemsStorage.BlendStates.DepthOnlyAlphaToCoverage);
                                } else {
                                    SetBlendState(SharedRenderItemsStorage.BlendStates.DepthOnlyAlphaTest);
                                }
                                //DepthMaskedSubPath(false);
                                IsMaskedSubPass = true;
                            }
                        }
                        else
                        {
                            GetContext.PixelShader.SetShaderResource(0,
                                GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset));
                        }
                        */
                        if (m_CachedMaterial.MetaMaterial.blendMode > ShaderGraph.MetaMaterial.BlendMode.Masked)
                        {
                            break; // Break on translucent objects
                        }
                    }

                    m_PerObjectShadowPassConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                    m_PerObjectShadowPassConstBuffer.ViewProjectionMatrix = light.ViewProjection;
                    GetContext.UpdateSubresource(ref m_PerObjectShadowPassConstBuffer, PerObjectShadowPassConstBuffer);

                    DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
                }
                break;
            }

            GetContext.Rasterizer.SetViewport(new Viewport(
                0, 0,
                GetDisplay.Width,
                GetDisplay.Height,
                0.0f, 1.0f
            ));
        }

        private List<StandardFrameData.RendererData> DynamicMeshes = new List<StandardFrameData.RendererData>();
        protected void DepthPrePass(StandardFrameData frameData)
        {
            CurrentPass = Pass.DepthPrePass;

            GetContext.ClearRenderTargetView(GetDisplay.RenderTargetViewRef, Color.CornflowerBlue);
            GetContext.ClearDepthStencilView(
                GetDisplay.DepthStencilViewRef,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                0.0f, 0
            );

            GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, (RenderTargetView)null);
            SetDepthStencilState(DepthStencilStates.Greater);
            SetRasterizerState(RasterizerStates.SolidBackCull);

            SetVertexShader("ForwardPlusPosOnlyVS");
            SetNullPixelShader();
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
            {
                Projection = frameData.CamerasList[0].Projection,
                ProjectionInv = Matrix.Invert(frameData.CamerasList[0].Projection),
                ViewInv = Matrix.Invert(frameData.CamerasList[0].View),
                PreviousView = frameData.CamerasList[0].PreviousView,
                CameraPos = frameData.CamerasList[0].Position,
                CameraForward = new Vector4(frameData.CamerasList[0].Forward, 1),
                AlphaTest = 0.5f,
                WindowWidth = (uint)GetDisplay.Width,
                WindowHeight = (uint)GetDisplay.Height,
                CurrentFPS = (uint)(1.0f / RenderBackend.EngineRef.Time.DeltaTime),
            };
            GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);

            GetContext.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            //GetContext.PixelShader.SetConstantBuffer(2, LightsDataBuffer);
            GetContext.PixelShader.SetConstantBuffer(3, ShadowLightsDataBuffer);
            GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);

            string MeshName = "";
            string MaterialName = "";
            bool IsMaskedSubPass = false;
            if (postProcessSettings.MotionBlurEnabled)
            {
                GetContext.ClearRenderTargetView(VelocityTarget.View, Color.Yellow);
                DynamicMeshes.Clear();
            }

            StandardFrameData.RendererData rendererData;
            foreach (var index in frameData.PerCameraRenderersList[0])
            {
                rendererData = frameData.RenderersList[index];
                if (postProcessSettings.MotionBlurEnabled && rendererData.IsDynamic)
                {
                    DynamicMeshes.Add(rendererData);
                    continue;
                }

                if (MeshName != rendererData.MeshName)
                {
                    MeshName = rendererData.MeshName;
                    SetMesh(MeshName);
                }

                if (MaterialName != rendererData.MaterialName)
                {
                    MaterialName = rendererData.MaterialName;
                    SetMaterial(MaterialName);
                    if (!IsMaskedSubPass)
                    {
                        if (m_CachedMaterial.MetaMaterial.blendMode == ShaderGraph.MetaMaterial.BlendMode.Masked)
                        {
                            GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, GetDisplay.RenderTargetViewRef);
                            if (EnabledMSAA) {
                                SetBlendState(BlendStates.DepthOnlyAlphaToCoverage);
                            } else {
                                SetBlendState(BlendStates.DepthOnlyAlphaTest);
                            }
                            DepthMaskedSubPath(false);
                            IsMaskedSubPass = true;
                        }
                    }
                    else
                    {
                        GetContext.PixelShader.SetShaderResource(0,
                            GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset));
                    }
                    if (m_CachedMaterial.MetaMaterial.blendMode > ShaderGraph.MetaMaterial.BlendMode.Masked)
                    {
                        break; // Break on translucent objects
                    }
                }

                m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                m_PerObjectConstBuffer.textureTiling = m_CachedMaterial.PropetyBlock.Tile;
                m_PerObjectConstBuffer.textureShift = m_CachedMaterial.PropetyBlock.Shift;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
            }

            if (!postProcessSettings.MotionBlurEnabled || DynamicMeshes.Count == 0)
            {
                return;
            }

            GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, VelocityTarget.View);
            SetBlendState(BlendStates.Opaque);
            SetRasterizerState(RasterizerStates.SolidBackCull);

            SetVertexShader("ForwardPlusPosOnlyVS");
            SetPixelShader("VelocityPS");

            IsMaskedSubPass = false;
            foreach (var dRendererData in DynamicMeshes)
            {
                if (MeshName != dRendererData.MeshName)
                {
                    MeshName = dRendererData.MeshName;
                    SetMesh(MeshName);
                }

                if (MaterialName != dRendererData.MaterialName)
                {
                    MaterialName = dRendererData.MaterialName;
                    SetMaterial(MaterialName);

                    if (!IsMaskedSubPass)
                    {
                        if (m_CachedMaterial.MetaMaterial.blendMode == ShaderGraph.MetaMaterial.BlendMode.Masked)
                        {
                            DepthMaskedSubPath(true);
                            IsMaskedSubPass = true;
                        }
                    }
                    else
                    {
                        GetContext.PixelShader.SetShaderResource(0,
                            GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset));
                    }

                    if (m_CachedMaterial.MetaMaterial.blendMode > ShaderGraph.MetaMaterial.BlendMode.Masked)
                    {
                        break; // Break on translucent objects
                    }
                }

                m_PerObjectConstBuffer.WorldMatrix = dRendererData.TransformMatrix;
                m_PerObjectConstBuffer.WorldViewMatrix = dRendererData.TransformMatrix * frameData.CamerasList[0].View;
                m_PerObjectConstBuffer.WorldViewProjMatrix = dRendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                m_PerObjectConstBuffer.PreviousWorldViewProjMatrix = dRendererData.PreviousTransformMatrix * frameData.CamerasList[0].PreviousViewProjection;
                GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);

                DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
            }
        }

        protected void DepthMaskedSubPath(bool isVelocityPass)
        {
            SetRasterizerState(RasterizerStates.SolidNoneCull);
            GetContext.PixelShader.SetSampler(0,
                GetSharedItems.GetSamplerState(SamplerType.AnisotropicWrap));

            SetVertexShader("ForwardPlusPosTexVS");
            if (!isVelocityPass) {
                SetPixelShader("ForwardPlusPosTexPS");
            } else {
                SetPixelShader("MaskedVelocityPS");
            }

            GetContext.PixelShader.SetShaderResource(0,
                GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset));
            GetContext.PixelShader.SetSampler(0,
                GetSharedItems.GetSamplerState(SamplerType.BilinearClamp));
        }

        protected void ColourPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.ColourPass;
            GetContext.OutputMerger.SetRenderTargets(GetDisplay.DepthStencilViewRef, ScreenQuadTarget.View);
            SetDepthStencilState(DepthStencilStates.EqualAndDisableWrite);

            SetVertexShader("CommonVS");
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            if (EnabledHDR)
            {
            } else {
            }

            // Draw scene
            string MeshName = "";
            string MaterialName = "";
            int MaterialQueue = -999999;

            m_ShadowLightsDataBuffer[0].LightViewProjectionMatrix = frameData.LightsList[0].ViewProjection;
            m_ShadowLightsDataBuffer[0].LeftTop = Vector2.Zero;
            m_ShadowLightsDataBuffer[0].RightBottom = Vector2.One;
            GetContext.UpdateSubresource(m_ShadowLightsDataBuffer, ShadowLightsDataBuffer);
            StandardFrameData.RendererData rendererData;
            foreach (var index in frameData.PerCameraRenderersList[0])
            {
                rendererData = frameData.RenderersList[index];
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

                DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
            }
        }

        protected void ScreenQuadPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.ScreenQuadPass;
            GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTargetViewRef);

            if (postProcessSettings.MotionBlurEnabled)
            {
                // TODO: shader with motion blur
                GetContext.PixelShader.SetShaderResource(0, ScreenQuadTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(1, VelocityTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(2, GetDisplay.DepthStencilSRVRef);
            } else
            {
                SetVertexShader("ScreenQuadVS");
                GetContext.InputAssembler.InputLayout = QuadLayoyt;
                SetPixelShader("ScreenQuadPS");
                GetContext.PixelShader.SetShaderResource(0, ScreenQuadTarget.ResourceView);
                //GetContext.PixelShader.SetShaderResource(0, ShadowMapsAtlasTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(2, GetDisplay.DepthStencilSRVRef);
            }

            GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(SamplerType.BilinearClamp));

            DX_Draw(4, 0);
        }

        #region Render loop helpers
        protected SharedRenderItemsStorage.CachedMesh m_CachedMesh;
        protected void SetMesh(string meshName)
        {
            m_CachedMesh = GetSharedItems.GetMesh(meshName);
            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_CachedMesh.vertexBuffer, 96, 0));
            GetContext.InputAssembler.SetIndexBuffer(m_CachedMesh.indexBuffer, Format.R32_UInt, 0);
            GetContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        protected Material m_CachedMaterial;
        protected void SetMaterial(string materialName, bool changeStates = false)
        {
            if (materialName == "SkySphereMaterial")
            {
                m_CachedMaterial = Material.GetSkySphereMaterial();
            }
            else
            {
                m_CachedMaterial = AssetsLoader.LoadMaterial(materialName);
            }

            if (CurrentPass == Pass.DepthPrePass || CurrentPass == Pass.ShadowMapsPass)
            {
                return;
            }

            if (changeStates)
            {
                SetMergerStates(m_CachedMaterial.MetaMaterial);
            }

            // TODO: setup shader from material meta. bind buffers and textures by shader.
            if (materialName == "SkySphereMaterial")
            {
                SetPixelShader("FwdSkySpherePS");
            }
            else
            {
                if (m_CachedMaterial.MetaMaterial.blendMode >= ShaderGraph.MetaMaterial.BlendMode.Translucent)
                {
                    SetPixelShader("TestShader");
                }
                else
                {
                    if (SetPixelShader("PBRForwardPS"))
                    {
                        GetContext.PixelShader.SetShaderResource(5, GetSharedItems.PreFilteredMap);
                        GetContext.PixelShader.SetShaderResource(6, GetSharedItems.IrradianceMap);
                        GetContext.PixelShader.SetShaderResource(7, ShadowMapsAtlasTarget.ResourceView);
                        GetContext.PixelShader.SetSampler(1, GetSharedItems.GetSamplerState(SamplerType.ShadowMap));
                    }
                }
            }

            m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct
            {
                textureTiling = m_CachedMaterial.PropetyBlock.Tile,
                textureShift = m_CachedMaterial.PropetyBlock.Shift,

                AlbedoColor = new Vector4(m_CachedMaterial.PropetyBlock.AlbedoColor, m_CachedMaterial.PropetyBlock.AlphaValue),
                RoughnessValue = m_CachedMaterial.PropetyBlock.RoughnessValue,
                MetallicValue = m_CachedMaterial.PropetyBlock.MetallicValue,

                optionsMask0 = CommonStructs.FloatMaskValue(
                    m_CachedMaterial.HasAlbedoMap, m_CachedMaterial.HasNormalMap,
                    m_CachedMaterial.HasRoughnessMap, m_CachedMaterial.HasMetallicMap),
                optionsMask1 = CommonStructs.FloatMaskValue(
                    m_CachedMaterial.HasOcclusionMap, false, false, false),
                filler = Vector2.Zero,
            };

            if (materialName == "SkySphereMaterial")
            {
                m_PerObjectConstBuffer.optionsMask1 = CommonStructs.FloatMaskValue(
                    m_CachedMaterial.HasOcclusionMap, true, true, false);
            }

            if (m_CachedMaterial.HasSampler)
            {
                GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(m_CachedMaterial.GetSamplerType));
                ShaderResourceView[] textures = new ShaderResourceView[]
                {
                    m_CachedMaterial.HasAlbedoMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset) : null,
                    m_CachedMaterial.HasNormalMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.NormalMapAsset) : null,
                    m_CachedMaterial.HasRoughnessMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.RoughnessMapAsset) : null,
                    m_CachedMaterial.HasMetallicMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.MetallicMapAsset) : null,
                    m_CachedMaterial.HasOcclusionMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.OcclusionMapAsset) : null,
                };
                GetContext.PixelShader.SetShaderResources(0, 5, textures);
            }
            else
            {
                GetContext.PixelShader.SetSampler(0, null);
                GetContext.PixelShader.SetShaderResources(0, 5, new ShaderResourceView[5] { null, null, null, null, null });
            }
        }

        protected void SetMergerStates(ShaderGraph.MetaMaterial meta)
        {
            switch (meta.blendMode)
            {
                case ShaderGraph.MetaMaterial.BlendMode.Opaque:
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
        #endregion
    }
}
