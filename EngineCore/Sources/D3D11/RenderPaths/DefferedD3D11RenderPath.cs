using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using static EngineCore.D3D11.SRITypeEnums;
using BlendMode = EngineCore.ShaderGraph.MetaMaterial.BlendMode;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace EngineCore.D3D11
{
    #region Base 
    internal partial class DefferedD3D11RenderPath : BaseD3D11RenderPath
    {
        // TODO: provide right params
        private static float MidleGray = 7.5f;//0.85f;
        private static float WhiteLum = 12.95f;//1.5f;
        private static float AdaptationPercent = 1f;

        public void SetToneMappingParams(float mg, float wl, float adapt)
        {
            var t = new CommonStructs.ConstBufferPostProcessStruct()
            {
                MiddleGrey = mg,
                LumWhiteSqr = (wl * mg) * (wl * mg),
            };
            AdaptationPercent = adapt;
            D3DUtils.WriteToDynamicBuffer(GetContext, PostProcessConstantBuffer, t);
        }

        public override void Init(D3D11RenderBackend renderBackend)
        {
            base.Init(renderBackend);
            InitShadersNames();
            InitGBuffer(MsSamplesCount);
            CreateBuffers();
            ScriptEngine.TestRef = this;
        }

        public override void Draw(StandardFrameData frameData)
        {
            GeometryPass(frameData);
            LightPass(frameData);
            if (FirstTranslucentRendererIndex > -1) {
                ForwardPass(frameData);
            }
            if (EnabledHdr) {
                ScreenQuadPass();
            }
            ResetShaderResourcesViews();
        }

        private enum Pass
        {
            Geometry,
            Light,
            Forward,
        }
        private Pass CurrentPass;

        private void ForwardPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.Forward;

            if (frameData.PerCameraLightsList[0].Count == 0) {
                if (EnabledHdr) {
                    GetContext.OutputMerger.SetRenderTargets(ReadonlyDepthStencilView, HDRTarget.View);
                } else {
                    GetContext.OutputMerger.SetRenderTargets(ReadonlyDepthStencilView, GetDisplay.RenderTargetViewRef);
                }
            }

            SetDepthStencilState(DepthStencilStates.GreaterAndDisableWrite);
            SetBlendState(BlendStates.AlphaEnabledBlending);

            SetInputLayout(GetSharedItems.StandardInputLayout);
            SetVertexShader("CommonVS");

            string MaterialName = "";
            StandardFrameData.RendererData rendererData;
            for (int i = FirstTranslucentRendererIndex; i < frameData.PerCameraRenderersList[0].Count; i++) {
                rendererData = frameData.RenderersList[frameData.PerCameraRenderersList[0][i]];

                SetMesh(rendererData.MeshName);

                if (MaterialName != rendererData.MaterialName) {
                    MaterialName = rendererData.MaterialName;
                    SetMaterial(MaterialName, true);
                }

                m_PerObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDefferedStruct()
                {
                    World = rendererData.TransformMatrix,
                    WorldInverse = Matrix.Transpose(Matrix.Invert(rendererData.TransformMatrix)),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, PerObjectConstantBuffer, m_PerObjectConstantBufferValue);

                DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
            }
        }

        private void ScreenQuadPass()
        {
            GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTargetViewRef);

            DownSampling();
            // Tone mapping
            // Bloom

            SetBlendState(BlendStates.Opaque);
            SetInputLayout(QuadLayout);
            SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

            SetVertexShader("ScreenQuadVS");

            SetPixelShader(ScreenQuadPSName);
            GetContext.PixelShader.SetShaderResource(0, HDRTarget.ResourceView);
            SetSamplerState(0, SamplerType.PointClamp);

            if (DwnSmplPrev) {
                GetContext.PixelShader.SetShaderResource(3, CSAvgLuminanceSRV);
            } else {
                GetContext.PixelShader.SetShaderResource(3, CSPrevAvgLuminanceSRV);
            }
            DX_Draw(4, 0);
            GetContext.PixelShader.SetShaderResource(0, null);

            GetContext.OutputMerger.ResetTargets();
        }

        private bool DwnSmplPrev = false;
        private void DownSampling()
        {
            DownScaleConstStruct constData = new DownScaleConstStruct()
            {
                ResX = (uint)(GetDisplay.Width / 4),
                ResY = (uint)(GetDisplay.Height / 4),
                Domain = (uint)((GetDisplay.Width * GetDisplay.Height) / 16),
                GroupSize = (uint)((GetDisplay.Width * GetDisplay.Height) / (16 * 1024)),
                Adaptation = Math.Min(RenderBackend.EngineRef.Time.DeltaTime / AdaptationPercent, 0.9999f),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, DownScaleConstantsBuffer, constData);

            SetComputeShader(DownsamplingFirsPassCSName);

            GetContext.ComputeShader.SetShaderResource(0, HDRTarget.ResourceView);
            GetContext.ComputeShader.SetUnorderedAccessView(0, CSLuminanceUAV);
            if (DwnSmplPrev) {
                GetContext.ComputeShader.SetShaderResource(1, CSAvgLuminanceSRV);
            } else {
                GetContext.ComputeShader.SetShaderResource(1, CSPrevAvgLuminanceSRV);
            }

            GetContext.Dispatch((GetDisplay.Width * GetDisplay.Height) / (16 * 1024), 1, 1);

            GetContext.ComputeShader.SetShaderResource(0, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);

            GetContext.ComputeShader.SetShaderResource(2, CSLuminanceSRV);
            if (DwnSmplPrev) {
                GetContext.ComputeShader.SetUnorderedAccessView(0, CSPrevAvgLuminanceUAV);
            } else {
                GetContext.ComputeShader.SetUnorderedAccessView(0, CSAvgLuminanceUAV);
            }
            SetComputeShader(DownsamplingSecondPassCSName);
            GetContext.Dispatch(1, 1, 1);

            GetContext.ComputeShader.SetShaderResource(1, null);
            GetContext.ComputeShader.SetShaderResource(2, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);
            DwnSmplPrev = !DwnSmplPrev;
        }

        public override void Resize()
        {
            // TODO: resize targets
            ResizeGBuffer();
        }

        public override void Dispose()
        {
            IsNullSRV = false;
            ResetShaderResourcesViews();
            base.Dispose();
            // TODO: dispose unmanaged resources
        }
    }
    #endregion

    #region Geometry pass
    internal partial class DefferedD3D11RenderPath
    {
        private void GeometryPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.Geometry;
            FirstTranslucentRendererIndex = -1;

            GetContext.ClearDepthStencilView(
                DepthStencilTarget.View,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                0.0f, 0
            );

            for (int i = 0; i < GBufferTargets.Length; i++) {
                GetContext.ClearRenderTargetView(GBufferTargets[i], Color.CornflowerBlue);
            }
            GetContext.OutputMerger.SetRenderTargets(DepthStencilTarget.View, GBufferTargets);

            SetDepthStencilState(DepthStencilStates.Greater);
            SetBlendState(BlendStates.Opaque);
            SetRasterizerState(RasterizerStates.SolidBackCull);
            SetInputLayout(GetSharedItems.StandardInputLayout);

            SetVertexShader("FillGBufferVS");

            m_PerFrameConstantBufferValue = new CommonStructs.ConstBufferPerFrameDefferedStruct()
            {
                View = frameData.CamerasList[0].View,
                InverseView = Matrix.Invert(frameData.CamerasList[0].View),
                Projection = frameData.CamerasList[0].Projection,
                InverseProjection = Matrix.Invert(frameData.CamerasList[0].Projection),
                CameraPosition = frameData.CamerasList[0].Position,
                PerspectiveValues = new Vector4(
                    1 / frameData.CamerasList[0].Projection.M11,
                    1 / frameData.CamerasList[0].Projection.M22,
                    frameData.CamerasList[0].Projection.M43,
                    -frameData.CamerasList[0].Projection.M33),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, PerFrameConstantBuffer, m_PerFrameConstantBufferValue);

            string MaterialName = "";
            bool IsMaskedSubPass = false;
            StandardFrameData.RendererData rendererData;

            DrawSkysphere(frameData.CamerasList[0].Position);

            int index;
            for (int it = 0; it < frameData.PerCameraRenderersList[0].Count; it++) {
                index = frameData.PerCameraRenderersList[0][it];
                rendererData = frameData.RenderersList[index];

                SetMesh(rendererData.MeshName);

                if (MaterialName != rendererData.MaterialName) {
                    MaterialName = rendererData.MaterialName;
                    SetMaterial(MaterialName);
                    if (!IsMaskedSubPass && m_CachedMaterial.MetaMaterial.blendMode == BlendMode.Masked) {
                        IsMaskedSubPass = true;
                        SetPixelShader("FillGBufferMaskedPS");
                    }
                    if (m_CachedMaterial.MetaMaterial.blendMode > BlendMode.Masked) {
                        FirstTranslucentRendererIndex = it;
                        break; // Break on translucent objects
                    }
                }

                m_PerObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDefferedStruct()
                {
                    World = rendererData.TransformMatrix,
                    WorldInverse = Matrix.Transpose(Matrix.Invert(rendererData.TransformMatrix)),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, PerObjectConstantBuffer, m_PerObjectConstantBufferValue);

                DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
            }
        }
    }
    #endregion

    #region Light pass
    internal partial class DefferedD3D11RenderPath
    {
        private void LightPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.Light;

            if (EnabledHdr) {
                GetContext.OutputMerger.SetRenderTargets(null, HDRTarget.View);
            } else {
                GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTargetViewRef);
            }

            ClearMesh();
            SetInputLayout(QuadLayout);
            SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

            SetVertexShader("ScreenQuadVS");

            SetPixelShader(LightPassPSName);
            SetSamplerState(0, SamplerType.IBLSampler);
            SetSamplerState(1, SamplerType.PreIntegratedSampler);

            GetContext.PixelShader.SetShaderResources(0, 8, new ShaderResourceView[] {
                AlbedoAndMetallicTarget.ResourceView,
                EmissiveAndRoughnessTarget.ResourceView,
                PackedNormalsTarget.ResourceView,
                SpecularAOUnlitNonShadowsTarget.ResourceView,
                DepthStencilTarget.ResourceView,
                GetSharedItems.PreFilteredMap,
                GetSharedItems.IrradianceMap,
                GetSharedItems.BRDFxLUT,
            });
            IsNullSRV = false;

            DX_Draw(4, 0);

            if (frameData.PerCameraLightsList[0].Count == 0) {
                return;
            }

            if (EnabledHdr) {
                GetContext.OutputMerger.SetRenderTargets(ReadonlyDepthStencilView, HDRTarget.View);
            } else {
                GetContext.OutputMerger.SetRenderTargets(ReadonlyDepthStencilView, GetDisplay.RenderTargetViewRef);
            }

            SetBlendState(BlendStates.Additive);
            SetRasterizerState(RasterizerStates.SolidBackCull);

            StandardFrameData.LightData lightData;
            ECS.Components.Light.LightType CurrentType = ECS.Components.Light.LightType.Directional;

            bool IsTesselationUsed = false;
            bool IsShaderBinded = false;

            foreach (var index in frameData.PerCameraLightsList[0]) {
                lightData = frameData.LightsList[index];

                m_PerObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDefferedStruct()
                {
                    World = Matrix.Scaling(lightData.Radius * 2 * 6) * Matrix.Translation(lightData.Position),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, PerObjectConstantBuffer, m_PerObjectConstantBufferValue);

                m_PerLightConstantBufferValue = new CommonStructs.ConstBufferPerLightStruct()
                {
                    Color = lightData.Color * lightData.Color,
                    Direction = Vector3.Normalize(lightData.Direction),
                    Intensity = lightData.Intensity,
                    Position = lightData.Position,
                    Radius = lightData.Radius,
                    Type = (uint)lightData.Type,
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, PerLightConstantBuffer, m_PerLightConstantBufferValue);

                if (!IsShaderBinded && lightData.Type == ECS.Components.Light.LightType.Directional) {
                    SetPixelShader(LightPassDirPSName);
                }

                if (lightData.Type != CurrentType) {
                    CurrentType = lightData.Type;
                    //SetPixelShader("LightPassPointPS");
                    //SetMesh("Primitives.LVSphere");
                }

                if (CurrentType == ECS.Components.Light.LightType.Directional) {
                    DX_Draw(4, 0);
                    continue;
                }

                if (Vector3.Distance(frameData.CamerasList[0].Position, lightData.Position) < lightData.Radius * 2) {
                    // Use SAQuad to process entire G-Buffer
                    SetDepthStencilState(DepthStencilStates.Disabled);
                    SetRasterizerState(RasterizerStates.SolidBackCull);
                    SetVertexShader("ScreenQuadVS");
                    SetPixelShader(LightPassPointQuadPSName);
                    SetNullHullShader();
                    SetNullDomainShader();
                    SetInputLayout(QuadLayout);
                    SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
                    DX_Draw(4, 0);
                    continue;
                }

                SetInputLayout(null);
                GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
                GetContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
                SetPrimitiveTopology(PrimitiveTopology.PatchListWith1ControlPoints);

                SetRasterizerState(RasterizerStates.SolidBackCull);
                SetDepthStencilState(DepthStencilStates.GreaterAndDisableWrite);

                SetVertexShader("LightVolumesVS");
                SetHullShader("LightVolumesHS");
                IsTesselationUsed = true;

                switch (CurrentType) {
                    case ECS.Components.Light.LightType.Point:
                        DrawPointLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                    case ECS.Components.Light.LightType.Capsule:
                        DrawCapsuleLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                    case ECS.Components.Light.LightType.Spot:
                        DrawSpotLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                }

                /*if (0.001 < viewSpacePos.Z + lightData.Radius)
                {
                    SetRasterizerState(SharedRenderItemsStorage.RasterizerStates.SolidBackCull);
                    SetDepthStencilState(SharedRenderItemsStorage.DepthStencilStates.GreaterAndDisableWrite);
                } else {
                    SetRasterizerState(SharedRenderItemsStorage.RasterizerStates.SolidFrontCull);
                    SetDepthStencilState(SharedRenderItemsStorage.DepthStencilStates.LessEqualAndDisableWrite);
                }*/
            }

            if (IsTesselationUsed) {
                SetNullHullShader();
                SetNullDomainShader();
            }
        }

        private void DrawPointLightVolume(StandardFrameData.LightData lightData,
            StandardFrameData.CameraData cameraData)
        {
            SetDomainShader("PointLightVolumeDS");
            // FinalMatrix = LightRangeScale * LightPositionTranslation * ViewTranslation * Projection 
            m_DomainShaderConstantBufferValue.LightMatrix = Matrix.Scaling(lightData.Radius * 1.25f) *
                Matrix.Translation(lightData.Position) * cameraData.ViewProjection;
            D3DUtils.WriteToDynamicBuffer(GetContext, DomainShaderConstantBuffer, m_DomainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(LightPassPointPSName);
            DX_Draw(2, 0);
        }

        private void DrawCapsuleLightVolume(StandardFrameData.LightData lightData,
            StandardFrameData.CameraData cameraData)
        {
            SetDomainShader("CapsuleLightVolumeDS");
            // FinalMatrix = LightRangeScale * LightPositionTranslation * ViewTranslation * Projection 
            m_DomainShaderConstantBufferValue.LightMatrix =
                Matrix.LookAtLH(Vector3.Zero, lightData.Direction, Vector3.Up) *
                Matrix.Translation(lightData.Position) * cameraData.ViewProjection;
            // HalfSegmentLen
            m_DomainShaderConstantBufferValue.LightParam1 = 2.5f;
            // CapsuleRadius
            m_DomainShaderConstantBufferValue.LightParam2 = lightData.Radius;
            D3DUtils.WriteToDynamicBuffer(GetContext, DomainShaderConstantBuffer, m_DomainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(LightPassPointPSName);
            DX_Draw(2, 0);
        }

        private void DrawSpotLightVolume(StandardFrameData.LightData lightData,
            StandardFrameData.CameraData cameraData)
        {
            SetDomainShader("SpotLightVolumeDS");
            // FinalMatrix = Scale * Rotate * Translate * View * Projection
            double x = (lightData.Radius * 2 * Math.Tan(MathUtil.Pi / 6)) / Math.Sqrt(0.5);
            m_DomainShaderConstantBufferValue.LightMatrix =
                Matrix.Scaling(new Vector3((float)x, (float)x, lightData.Radius * 2 * 2)) *
                Matrix.LookAtLH(Vector3.Zero, lightData.Direction, Vector3.Up) *
                Matrix.Translation(lightData.Position) * cameraData.ViewProjection;
            // SinAngle
            m_DomainShaderConstantBufferValue.LightParam1 = 0.5f;
            // CosAngle
            m_DomainShaderConstantBufferValue.LightParam2 = 0.5f;
            D3DUtils.WriteToDynamicBuffer(GetContext, DomainShaderConstantBuffer, m_DomainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(LightPassPointPSName);
            DX_Draw(1, 0);
        }
    }
    #endregion

    #region Render loop helpers
    internal partial class DefferedD3D11RenderPath
    {
        private int FirstTranslucentRendererIndex;
        private string CurrentMeshName;
        protected SharedRenderItemsStorage.CachedMesh m_CachedMesh;
        protected void SetMesh(string meshName)
        {
            if (CurrentMeshName == meshName) {
                return;
            }
            CurrentMeshName = meshName;
            m_CachedMesh = GetSharedItems.GetMesh(meshName);
            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_CachedMesh.vertexBuffer, 96, 0));
            GetContext.InputAssembler.SetIndexBuffer(m_CachedMesh.indexBuffer, Format.R32_UInt, 0);
            SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        }

        protected void ClearMesh()
        {
            CurrentMeshName = "";
        }

        protected Material m_CachedMaterial;
        protected void SetMaterial(string materialName, bool changeStates = false)
        {
            m_CachedMaterial = AssetsLoader.LoadMaterial(materialName);

            // DEBUG
            if (CurrentPass == Pass.Geometry) {
                if (m_CachedMaterial.MetaMaterial.blendMode == BlendMode.Translucent) {
                    return;
                }
            }

            SetMaterialCullMode(m_CachedMaterial.MetaMaterial);

            m_PerMaterialConstantBufferValue = new CommonStructs.ConstBufferPerMaterialDefferedStruct()
            {
                AlbedoColor = new Vector4(m_CachedMaterial.PropetyBlock.AlbedoColor,
                    m_CachedMaterial.PropetyBlock.AlphaValue),
                EmissiveColor = Vector4.Zero,

                MetallicValue = m_CachedMaterial.PropetyBlock.MetallicValue,
                RoughnessValue = m_CachedMaterial.PropetyBlock.RoughnessValue,
                SpecularValue = 0,
                Unlit = 0,

                textureTiling = m_CachedMaterial.PropetyBlock.Tile,
                textureShift = m_CachedMaterial.PropetyBlock.Shift,

                optionsMask0 = CommonStructs.FloatMaskValue(
                    m_CachedMaterial.HasAlbedoMap, m_CachedMaterial.HasMetallicMap,
                   false, m_CachedMaterial.HasRoughnessMap),
                optionsMask1 = CommonStructs.FloatMaskValue(
                    m_CachedMaterial.HasNormalMap, false,
                    m_CachedMaterial.HasOcclusionMap, false),
                AlphaClip = 0.5f,
            };

            if (CurrentPass == Pass.Forward) {
                SetPixelShader("TestShader");
            } else {
                SetPixelShader("FillGBufferPS");
            }

            D3DUtils.WriteToDynamicBuffer(GetContext, PerMaterialConstantBuffer, m_PerMaterialConstantBufferValue);

            if (m_CachedMaterial.HasSampler) {
                //SetSamplerState(0, m_CachedMaterial.GetSamplerType);
                SetSamplerState(0, SamplerType.TrilinearWrap);
                ShaderResourceView[] textures = new ShaderResourceView[]
                {
                    m_CachedMaterial.HasAlbedoMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.AlbedoMapAsset) : null,
                    m_CachedMaterial.HasMetallicMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.MetallicMapAsset) : null,
                    null, // Emmisive
                    m_CachedMaterial.HasRoughnessMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.RoughnessMapAsset) : null,
                    m_CachedMaterial.HasNormalMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.NormalMapAsset) : null,
                    null, // Specular
                    m_CachedMaterial.HasOcclusionMap ? GetSharedItems.LoadTextureSRV(m_CachedMaterial.OcclusionMapAsset) : null,
                };
                GetContext.PixelShader.SetShaderResources(0, 7, textures);
                IsNullSRV = false;
                textures = null;
            } else {
                ResetShaderResourcesViews();
                SetSamplerState(0, SamplerType.TrilinearWrap);
            }
        }

        private bool IsNullSRV = true;
        private ShaderResourceView[] Null7ShaderResourceViews = new ShaderResourceView[7] { null, null, null, null, null, null, null, };
        private void ResetShaderResourcesViews()
        {
            if (!IsNullSRV) {
                IsNullSRV = true;
                GetContext.PixelShader.SetShaderResources(0, 7, Null7ShaderResourceViews);
            }
        }

        private void SetMaterialCullMode(ShaderGraph.MetaMaterial meta)
        {
            switch (meta.cullMode) {
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

        private static Matrix SkySphereMatrix = Matrix.Scaling(Vector3.One * 5000f) *
            Matrix.RotationQuaternion(Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0));
        private void DrawSkysphere(Vector3 cameraPosition)
        {
            SetMesh("SkyDomeMesh");
            SetPixelShader("FillGBufferSkyboxPS");

            SetSamplerState(0, SamplerType.TrilinearWrap);
            ShaderResourceView[] textures = new ShaderResourceView[]
            {
                GetSharedItems.LoadTextureSRV(Material.GetSkySphereMaterial().AlbedoMapAsset, true),
                null, null, // Emmisive
                null, null, null, // Specular
                null,
            };

            IsNullSRV = false;
            GetContext.PixelShader.SetShaderResources(0, 7, textures);

            m_PerMaterialConstantBufferValue = new CommonStructs.ConstBufferPerMaterialDefferedStruct()
            {
                Unlit = 1,
                optionsMask0 = CommonStructs.FloatMaskValue(true, false, false, false),
                optionsMask1 = CommonStructs.FloatMaskValue(false, false, false, true),
            };

            m_PerObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDefferedStruct()
            {
                World = SkySphereMatrix * Matrix.Translation(cameraPosition),
                WorldInverse = Matrix.Transpose(Matrix.Invert(SkySphereMatrix * Matrix.Translation(cameraPosition))),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, PerObjectConstantBuffer, m_PerObjectConstantBufferValue);
            D3DUtils.WriteToDynamicBuffer(GetContext, PerMaterialConstantBuffer, m_PerMaterialConstantBufferValue);
            DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
        }
    }
    #endregion

    #region G-Buffer
    internal partial class DefferedD3D11RenderPath
    {
        SharedRenderItemsStorage.RenderTargetPack AlbedoAndMetallicTarget;
        SharedRenderItemsStorage.RenderTargetPack EmissiveAndRoughnessTarget;
        SharedRenderItemsStorage.RenderTargetPack PackedNormalsTarget;
        SharedRenderItemsStorage.RenderTargetPack SpecularAOUnlitNonShadowsTarget;
        SharedRenderItemsStorage.RenderTargetPack HDRTarget;
        SharedRenderItemsStorage.DepthStencilTargetPack DepthStencilTarget;
        DepthStencilView ReadonlyDepthStencilView;

        RenderTargetView[] GBufferTargets;

        private void InitGBuffer(int samples)
        {
            AlbedoAndMetallicTarget = GetSharedItems.CreateRenderTarget("AlbedoAndMetallic",
                GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            EmissiveAndRoughnessTarget = GetSharedItems.CreateRenderTarget("EmissiveAndRoughness",
                GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);
            PackedNormalsTarget = GetSharedItems.CreateRenderTarget("PackedNormals",
                GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);//R11G11B10_Float
            SpecularAOUnlitNonShadowsTarget = GetSharedItems.CreateRenderTarget("SpecularAOUnlitNonShadows",
                GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);

            if (EnabledHdr) {
                HDRTarget = GetSharedItems.CreateRenderTarget("HDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            }

            DepthStencilTarget = GetSharedItems.CreateDepthRenderTarget("DepthStencil",
                GetDisplay.Width, GetDisplay.Height, samples);

            ToDispose(ReadonlyDepthStencilView = new DepthStencilView(GetDevice, DepthStencilTarget.Map, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = samples > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil,
            }));
            ReadonlyDepthStencilView.DebugName = "ReadonlyDepthStencilView";

            GBufferTargets = new RenderTargetView[]
            {
                AlbedoAndMetallicTarget.View,
                EmissiveAndRoughnessTarget.View,
                PackedNormalsTarget.View,
                SpecularAOUnlitNonShadowsTarget.View,
            };

            InitDownsamplingResources();
        }

        private void ResizeGBuffer()
        {
            AlbedoAndMetallicTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            EmissiveAndRoughnessTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            PackedNormalsTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            SpecularAOUnlitNonShadowsTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            DepthStencilTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            GBufferTargets = new RenderTargetView[]
            {
                AlbedoAndMetallicTarget.View,
                EmissiveAndRoughnessTarget.View,
                PackedNormalsTarget.View,
                SpecularAOUnlitNonShadowsTarget.View,
            };
            HDRTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            InitDownsamplingSizableResources();
        }
    }
    #endregion

    #region Buffers and shaders names
    internal partial class DefferedD3D11RenderPath
    {
        CommonStructs.ConstBufferPerFrameDefferedStruct m_PerFrameConstantBufferValue;
        CommonStructs.ConstBufferPerObjectDefferedStruct m_PerObjectConstantBufferValue;
        CommonStructs.ConstBufferPerMaterialDefferedStruct m_PerMaterialConstantBufferValue;
        CommonStructs.ConstBufferPerLightStruct m_PerLightConstantBufferValue;
        CommonStructs.ConstBufferLightVolumeDomainShader m_DomainShaderConstantBufferValue;

        Buffer PerFrameConstantBuffer;
        Buffer PerObjectConstantBuffer;
        Buffer PerMaterialConstantBuffer;
        Buffer PerLightConstantBuffer;
        Buffer PostProcessConstantBuffer;
        Buffer DomainShaderConstantBuffer;

        InputLayout QuadLayout;

        private void CreateBuffers()
        {
            BufferDescription bufferDescription = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
            };

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameDefferedStruct>();
            ToDispose(PerFrameConstantBuffer = new Buffer(GetDevice, bufferDescription));
            PerFrameConstantBuffer.DebugName = "PerFrameConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectDefferedStruct>();
            ToDispose(PerObjectConstantBuffer = new Buffer(GetDevice, bufferDescription));
            PerObjectConstantBuffer.DebugName = "PerObjectConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerMaterialDefferedStruct>();
            ToDispose(PerMaterialConstantBuffer = new Buffer(GetDevice, bufferDescription));
            PerMaterialConstantBuffer.DebugName = "PerMaterialConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerLightStruct>();
            ToDispose(PerLightConstantBuffer = new Buffer(GetDevice, bufferDescription));
            PerLightConstantBuffer.DebugName = "PerLightConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPostProcessStruct>();
            ToDispose(PostProcessConstantBuffer = new Buffer(GetDevice, bufferDescription));
            PostProcessConstantBuffer.DebugName = "PostProcessConstantBuffer";

            var m_PostProcessConstantBufferValue = new CommonStructs.ConstBufferPostProcessStruct()
            {
                MiddleGrey = MidleGray,
                LumWhiteSqr = (WhiteLum * MidleGray) * (WhiteLum * MidleGray),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, PostProcessConstantBuffer, m_PostProcessConstantBufferValue);

            Buffer[] MainBuffersBatch = new Buffer[] {
                PerFrameConstantBuffer,
                PerObjectConstantBuffer,
                PerMaterialConstantBuffer,
                PerLightConstantBuffer,
                PostProcessConstantBuffer,
            };

            GetContext.VertexShader.SetConstantBuffers(0, MainBuffersBatch.Length, MainBuffersBatch);
            GetContext.PixelShader.SetConstantBuffers(0, MainBuffersBatch.Length, MainBuffersBatch);

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferLightVolumeDomainShader>();
            ToDispose(DomainShaderConstantBuffer = new Buffer(GetDevice, bufferDescription));
            DomainShaderConstantBuffer.DebugName = "DomainShaderConstantBuffer";
            GetContext.DomainShader.SetConstantBuffer(0, DomainShaderConstantBuffer);

            AssetsLoader.GetShader<VertexShader>("ScreenQuadVS", out SharpDX.D3DCompiler.ShaderSignature vsSignature);
            ToDispose(QuadLayout = new InputLayout(GetDevice, vsSignature, new InputElement[] {
                new InputElement("SV_VertexID", 0, Format.R32_UInt, 0, 0),
            }));
            QuadLayout.DebugName = "QuadLayout";
        }

        Buffer CSLuminanceBuffer;
        UnorderedAccessView CSLuminanceUAV;
        ShaderResourceView CSLuminanceSRV;

        Buffer CSAvgLuminanceBuffer;
        UnorderedAccessView CSAvgLuminanceUAV;
        ShaderResourceView CSAvgLuminanceSRV;

        Buffer CSPrevAvgLuminanceBuffer;
        UnorderedAccessView CSPrevAvgLuminanceUAV;
        ShaderResourceView CSPrevAvgLuminanceSRV;

        Buffer DownScaleConstantsBuffer;

        private struct DownScaleConstStruct
        {
            public uint ResX;
            public uint ResY;
            public uint Domain;
            public uint GroupSize;
            public float Adaptation;
            private Vector3 Filler;
        }

        private void InitDownsamplingSizableResources()
        {
            CSLuminanceBuffer?.Dispose();
            int ElementsCount = (GetDisplay.Width * GetDisplay.Height) / (16 * 1024);
            ElementsCount = ElementsCount > 0 ? ElementsCount : 1;
            ToDispose(CSLuminanceBuffer = new Buffer(GetDevice, new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4 * ElementsCount,
            }));
            CSLuminanceBuffer.DebugName = "CSLuminanceBuffer";

            CSLuminanceUAV?.Dispose();
            ToDispose(CSLuminanceUAV = new UnorderedAccessView(GetDevice, CSLuminanceBuffer, new UnorderedAccessViewDescription()
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = ElementsCount,
                }
            }));
            CSLuminanceUAV.DebugName = "CSLuminanceUAV";

            CSLuminanceSRV?.Dispose();
            ToDispose(CSLuminanceSRV = new ShaderResourceView(GetDevice, CSLuminanceBuffer, new ShaderResourceViewDescription()
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = ElementsCount,
                }
            }));
            CSLuminanceSRV.DebugName = "CSLuminanceSRV";
        }

        private void InitDownsamplingResources()
        {
            InitDownsamplingSizableResources();
            // Current
            ToDispose(CSAvgLuminanceBuffer = new Buffer(GetDevice, new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4,
            }));
            CSAvgLuminanceBuffer.DebugName = "CSAvgLuminanceBuffer";

            ToDispose(CSAvgLuminanceUAV = new UnorderedAccessView(GetDevice, CSAvgLuminanceBuffer, new UnorderedAccessViewDescription()
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            CSAvgLuminanceUAV.DebugName = "CSAvgLuminanceUAV";

            ToDispose(CSAvgLuminanceSRV = new ShaderResourceView(GetDevice, CSAvgLuminanceBuffer, new ShaderResourceViewDescription()
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            CSAvgLuminanceSRV.DebugName = "CSAvgLuminanceSRV";

            // Prev
            ToDispose(CSPrevAvgLuminanceBuffer = new Buffer(GetDevice, new BufferDescription()
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4,
            }));
            CSPrevAvgLuminanceBuffer.DebugName = "CSPrevAvgLuminanceBuffer";

            ToDispose(CSPrevAvgLuminanceUAV = new UnorderedAccessView(GetDevice, CSPrevAvgLuminanceBuffer, new UnorderedAccessViewDescription()
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            CSPrevAvgLuminanceUAV.DebugName = "CSPrevAvgLuminanceUAV";

            ToDispose(CSPrevAvgLuminanceSRV = new ShaderResourceView(GetDevice, CSPrevAvgLuminanceBuffer, new ShaderResourceViewDescription()
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            CSPrevAvgLuminanceSRV.DebugName = "CSPrevAvgLuminanceSRV";

            ToDispose(DownScaleConstantsBuffer = new Buffer(GetDevice, new BufferDescription()
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<DownScaleConstStruct>(),
            }));
            DownScaleConstantsBuffer.DebugName = "DownScaleConstantsBuffer";

            GetContext.ComputeShader.SetConstantBuffer(0, DownScaleConstantsBuffer);
        }

        private string LightPassPSName;
        private string LightPassDirPSName;
        private string LightPassPointPSName;
        private string LightPassPointQuadPSName;
        private string LightPassSpotPSName;
        private string LightPassSpotQuadPSName;
        private string DownsamplingFirsPassCSName;
        private string DownsamplingSecondPassCSName;
        private string ScreenQuadPSName;

        private void InitShadersNames()
        {
            LightPassPSName = EnabledMsaa ? "MSAA_LightPassPS" : "LightPassPS";
            LightPassDirPSName = EnabledMsaa ? "MSAA_LightPassDirectionalPS" : "LightPassDirectionalPS";
            LightPassPointPSName = EnabledMsaa ? "MSAA_LightPassPointPS" : "LightPassPointPS";
            LightPassPointQuadPSName = EnabledMsaa ? "MSAA_LightPassPointQuadPS" : "LightPassPointQuadPS";
            // LightPassSpotPSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassSpotQuadPSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassCapsulePSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassCapsuleQuadPSName = EnabledMSAA ? "MSAA_" : "";
            DownsamplingFirsPassCSName = "DownsamplingFirstCS";
            DownsamplingSecondPassCSName = "DownsamplingSecondCS";

            ScreenQuadPSName = EnabledMsaa ? "MSAA_ScreenQuadPS" : "ScreenQuadPS";
        }
    }
    #endregion
}
