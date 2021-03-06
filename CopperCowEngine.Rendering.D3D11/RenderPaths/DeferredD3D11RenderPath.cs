﻿using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Linq;
using System.Numerics;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.D3D11.Utils;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using InputElement = SharpDX.Direct3D11.InputElement;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    #region Base 
    internal partial class DeferredD3D11RenderPath : BaseD3D11RenderPath
    {
        // TODO: provide right params
        private const float MiddleGray = 7.5f; //0.85f;
        private const float WhiteLum = 12.95f; //1.5f;

        /*public void SetToneMappingParams(float mg, float wl, float adapt)
        {
            var t = new CommonStructs.ConstBufferPostProcessStruct()
            {
                MiddleGrey = mg,
                LumWhiteSqr = (wl * mg) * (wl * mg),
            };
            _adaptationPercent = adapt;
            D3DUtils.WriteToDynamicBuffer(GetContext, _postProcessConstantBuffer, t);
        }*/

        public override void Init(D3D11RenderBackend renderBackend)
        {
            base.Init(renderBackend);
            InitShadersNames();
            InitGBuffer(MsSamplesCount);
            CreateBuffers();
            // ScriptEngine.TestRef = this;
            MeshAssetsLoader.LoadMesh("SkyDomeMesh");
        }

        public override void Draw(StandardFrameData frameData)
        {
            GeometryPass(frameData);
            LightPass(frameData);
            if (_firstTranslucentRendererIndex > -1)
            {
                ForwardPass(frameData);
            }
            if (HdrEnable)
            {
                ScreenQuadPass(frameData);
            }
            ResetShaderResourcesViews();
        }

        private enum Pass : byte
        {
            Geometry,
            Light,
            Forward,
        }

        private Pass _currentPass;

        private void ForwardPass(StandardFrameData frameData)
        {
            _currentPass = Pass.Forward;

            if (frameData.PerCameraLights[0].Count == 0)
            {
                GetContext.OutputMerger.SetRenderTargets(_readonlyDepthStencilView,
                    HdrEnable ? _hdrTarget.TargetView : GetDisplay.RenderTarget);
            }

            SetDepthStencilState(DepthStencilStateType.GreaterAndDisableWrite);
            SetBlendState(BlendStateType.AlphaEnabledBlending);

            SetInputLayout(GetSharedItems.StandardInputLayout);
            SetVertexShader("CommonVS");

            ClearMesh();
            ClearMaterial();

            for (var i = _firstTranslucentRendererIndex; i < frameData.PerCameraRenderers[0].Count; i++)
            {
                var rendererData = frameData.RenderersList[frameData.PerCameraRenderers[0][i]];

                SetMesh(rendererData.MeshGuid);
                SetMaterial(rendererData.MaterialGuid);
                
                Matrix4x4.Invert(rendererData.TransformMatrix, out var worldInverse);

                _perObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDeferredStruct()
                {
                    World = rendererData.TransformMatrix,
                    WorldInverse = Matrix4x4.Transpose(worldInverse),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstantBufferValue);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }

        private void ScreenQuadPass(StandardFrameData frameData)
        {
            GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTarget);

            DownSampling(_hdrTarget, frameData.CamerasList[0].FrameTime);
            // Tone mapping
            // Bloom

            SetBlendState(BlendStateType.Opaque);
            SetRasterizerState(RasterizerStateType.SolidBackCull);

            SetInputLayout(_quadLayout);
            SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

            SetVertexShader("ScreenQuadVS");

            SetPixelShader(_screenQuadPsName);
            GetContext.PixelShader.SetShaderResource(0, _hdrTarget.ResourceView);
            SetSamplerState(0, SamplerStateType.PointClamp);

            GetContext.PixelShader.SetShaderResource(3, CurrentAvgLuminanceSrv);
            DX_Draw(4, 0);
            GetContext.PixelShader.SetShaderResource(0, null);

            GetContext.OutputMerger.ResetTargets();
        }

        public override void Resize()
        {
            // TODO: resize targets
            ResizeGBuffer();
        }

        public override void Dispose()
        {
            _isNullSrv = false;
            ResetShaderResourcesViews();
            base.Dispose();
            // TODO: dispose unmanaged resources
        }
    }
    #endregion

    #region Geometry pass
    internal partial class DeferredD3D11RenderPath
    {
        private void GeometryPass(StandardFrameData frameData)
        {
            _currentPass = Pass.Geometry;
            _firstTranslucentRendererIndex = -1;

            GetContext.ClearDepthStencilView(
                _depthStencilTarget.View,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                0.0f, 0
            );

            foreach (var target in _gBufferTargets)
            {
                GetContext.ClearRenderTargetView(target, Color.CornflowerBlue);
            }
            GetContext.OutputMerger.SetRenderTargets(_depthStencilTarget.View, _gBufferTargets);

            SetDepthStencilState(DepthStencilStateType.Greater);
            SetBlendState(BlendStateType.Opaque);
            SetRasterizerState(RasterizerStateType.SolidBackCull);
            SetInputLayout(GetSharedItems.StandardInputLayout);

            SetVertexShader("FillGBufferVS");
            
            Matrix4x4.Invert(frameData.CamerasList[0].Projection, out var projectionInverse);
            Matrix4x4.Invert(frameData.CamerasList[0].View, out var viewInverse);

            _perFrameConstantBufferValue = new CommonStructs.ConstBufferPerFrameDeferredStruct()
            {
                View = frameData.CamerasList[0].View,
                InverseView = viewInverse,
                Projection = frameData.CamerasList[0].Projection,
                InverseProjection = projectionInverse,
                CameraPosition = frameData.CamerasList[0].Position,
                PerspectiveValues = new Vector4(
                    1 / frameData.CamerasList[0].Projection.M11,
                    1 / frameData.CamerasList[0].Projection.M22,
                    frameData.CamerasList[0].Projection.M43,
                    -frameData.CamerasList[0].Projection.M33),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _perFrameConstantBuffer, _perFrameConstantBufferValue);

            var MaterialGuid = Guid.Empty;
            var isMaskedSubPass = false;

            DrawSkySphere(frameData.CamerasList[0].Position);

            for (var it = 0; it < frameData.PerCameraRenderers[0].Count; it++)
            {
                int index = frameData.PerCameraRenderers[0][it];
                var rendererData = frameData.RenderersList[index];

                SetMesh(rendererData.MeshGuid);

                if (MaterialGuid != rendererData.MaterialGuid)
                {
                    MaterialGuid = rendererData.MaterialGuid;
                    SetMaterial(MaterialGuid);
                    if (!isMaskedSubPass && CachedMaterial.MetaMaterial.BlendMode == MaterialMeta.BlendModeType.Masked)
                    {
                        isMaskedSubPass = true;
                        SetPixelShader("FillGBufferMaskedPS");
                    }
                    if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                    {
                        _firstTranslucentRendererIndex = it;
                        break; // Break on translucent objects
                    }
                }

                Matrix4x4.Invert(rendererData.TransformMatrix, out var worldInvert);
                _perObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDeferredStruct()
                {
                    World = rendererData.TransformMatrix,
                    WorldInverse = Matrix4x4.Transpose(worldInvert),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstantBufferValue);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }
    }
    #endregion

    #region Light pass
    internal partial class DeferredD3D11RenderPath
    {
        private void LightPass(StandardFrameData frameData)
        {
            _currentPass = Pass.Light;

            GetContext.OutputMerger.SetRenderTargets(null,
                HdrEnable ? _hdrTarget.TargetView : GetDisplay.RenderTarget);

            ClearMesh();
            SetInputLayout(_quadLayout);
            SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);

            SetVertexShader("ScreenQuadVS");

            SetPixelShader(_lightPassPsName);
            SetSamplerState(0, SamplerStateType.IBLSampler);
            SetSamplerState(1, SamplerStateType.PreIntegratedSampler);

            GetContext.PixelShader.SetShaderResources(0, 8, _albedoAndMetallicTarget.ResourceView, 
                _emissiveAndRoughnessTarget.ResourceView, _packedNormalsTarget.ResourceView, 
                _specularAoUnlitNonShadowsTarget.ResourceView, _depthStencilTarget.ResourceView, 
                GetSharedItems.LoadTextureShaderResourceView(MaterialInstance.GetSkySphereMaterial().AlbedoMapAsset, true), 
                GetSharedItems.LoadTextureShaderResourceView(MaterialInstance.GetSkySphereMaterial().EmissiveMapAsset, true), 
                GetSharedItems.BRDFxLookUpTable);
            _isNullSrv = false;

            SetRasterizerState(RasterizerStateType.SolidBackCull);

            DX_Draw(4, 0);

            if (frameData.PerCameraLights[0].Count == 0)
            {
                return;
            }

            GetContext.OutputMerger.SetRenderTargets(_readonlyDepthStencilView,
                HdrEnable ? _hdrTarget.TargetView : GetDisplay.RenderTarget);

            SetBlendState(BlendStateType.Additive);

            var currentLightType = LightType.Directional;

            var isTessellationUsed = false;

            foreach (var lightData in frameData.PerCameraLights[0].Select(index => frameData.LightsList[index]))
            {
                _perObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDeferredStruct()
                {
                    World = Matrix4x4.CreateScale(lightData.Radius * 2 * 6) * Matrix4x4.CreateTranslation(lightData.Position),
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstantBufferValue);

                _perLightConstantBufferValue = new CommonStructs.ConstBufferPerLightStruct()
                {
                    Color = lightData.Color * lightData.Color,
                    Direction = Vector3.Normalize(lightData.Direction),
                    Intensity = lightData.Intensity,
                    Position = lightData.Position,
                    Radius = lightData.Radius,
                    Type = (uint)lightData.Type,
                };
                D3DUtils.WriteToDynamicBuffer(GetContext, _perLightConstantBuffer, _perLightConstantBufferValue);

                if (lightData.Type == LightType.Directional)
                {
                    SetPixelShader(_lightPassDirPsName);
                }

                /*if (lightData.Type != currentLightType)
                {
                    currentLightType = lightData.Type;
                    SetPixelShader("LightPassPointPS");
                    SetMesh("Primitives.LVSphere");
                }*/
                currentLightType = lightData.Type;

                if (currentLightType == LightType.Directional)
                {
                    DX_Draw(4, 0);
                    continue;
                }

                if (Vector3.Distance(frameData.CamerasList[0].Position, lightData.Position) < lightData.Radius * 2)
                {
                    // Use SAQuad to process entire G-Buffer
                    SetDepthStencilState(DepthStencilStateType.Disabled);
                    SetRasterizerState(RasterizerStateType.SolidBackCull);
                    SetVertexShader("ScreenQuadVS");
                    SetPixelShader(_lightPassPointQuadPsName);
                    SetNullHullShader();
                    SetNullDomainShader();
                    SetInputLayout(_quadLayout);
                    SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
                    DX_Draw(4, 0);
                    continue;
                }

                SetInputLayout(null);
                GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
                GetContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
                SetPrimitiveTopology(PrimitiveTopology.PatchListWith1ControlPoints);

                SetRasterizerState(RasterizerStateType.SolidBackCull);
                SetDepthStencilState(DepthStencilStateType.GreaterAndDisableWrite);

                SetVertexShader("LightVolumesVS");
                SetHullShader("LightVolumesHS");
                isTessellationUsed = true;

                switch (currentLightType)
                {
                    case LightType.Point:
                        DrawPointLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                    case LightType.Capsule:
                        DrawCapsuleLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                    case LightType.Spot:
                        DrawSpotLightVolume(lightData, frameData.CamerasList[0]);
                        break;
                    case LightType.Directional:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (!isTessellationUsed)
            {
                return;
            }
            SetNullHullShader();
            SetNullDomainShader();
        }

        private void DrawPointLightVolume(LightData lightData, CameraData cameraData)
        {
            SetDomainShader("PointLightVolumeDS");
            // FinalMatrix = LightRangeScale * LightPositionTranslation * ViewTranslation * Projection 
            _domainShaderConstantBufferValue.LightMatrix = Matrix4x4.CreateScale(lightData.Radius * 1.25f) *
                                                           Matrix4x4.CreateTranslation(lightData.Position) * cameraData.ViewProjection;
            D3DUtils.WriteToDynamicBuffer(GetContext, _domainShaderConstantBuffer, _domainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(_lightPassPointPsName);
            DX_Draw(2, 0);
        }

        private void DrawCapsuleLightVolume(LightData lightData, CameraData cameraData)
        {
            SetDomainShader("CapsuleLightVolumeDS");
            // FinalMatrix = LightRangeScale * LightPositionTranslation * ViewTranslation * Projection 
            _domainShaderConstantBufferValue.LightMatrix =
                Matrix4x4.CreateLookAt(Vector3.Zero, lightData.Direction, Vector3.UnitY) *
                Matrix4x4.CreateTranslation(lightData.Position) * cameraData.ViewProjection;
            // HalfSegmentLen
            _domainShaderConstantBufferValue.LightParam1 = 2.5f;
            // CapsuleRadius
            _domainShaderConstantBufferValue.LightParam2 = lightData.Radius;
            D3DUtils.WriteToDynamicBuffer(GetContext, _domainShaderConstantBuffer, _domainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(_lightPassPointPsName);
            DX_Draw(2, 0);
        }

        private void DrawSpotLightVolume(LightData lightData, CameraData cameraData)
        {
            SetDomainShader("SpotLightVolumeDS");
            // FinalMatrix = Scale * Rotate * Translate * View * Projection
            var x = (lightData.Radius * 2 * Math.Tan(MathUtil.Pi / 6)) / Math.Sqrt(0.5);
            _domainShaderConstantBufferValue.LightMatrix =
                Matrix4x4.CreateScale(new Vector3((float)x, (float)x, lightData.Radius * 2 * 2)) *
                Matrix4x4.CreateLookAt(Vector3.Zero, lightData.Direction, Vector3.UnitY) *
                Matrix4x4.CreateTranslation(lightData.Position) * cameraData.ViewProjection;
            // SinAngle
            _domainShaderConstantBufferValue.LightParam1 = 0.5f;
            // CosAngle
            _domainShaderConstantBufferValue.LightParam2 = 0.5f;
            D3DUtils.WriteToDynamicBuffer(GetContext, _domainShaderConstantBuffer, _domainShaderConstantBufferValue);

            //TODO: Provide right params set to pixel shader
            SetPixelShader(_lightPassPointPsName);
            DX_Draw(1, 0);
        }
    }
    #endregion

    #region Render loop helpers
    internal partial class DeferredD3D11RenderPath
    {
        private int _firstTranslucentRendererIndex;


        protected override void OnMaterialChanged()
        {
            // DEBUG
            if (_currentPass == Pass.Geometry)
            {
                if (CachedMaterial.MetaMaterial.BlendMode == MaterialMeta.BlendModeType.Translucent)
                {
                    return;
                }
            }

            SetMaterialCullMode(CachedMaterial.MetaMaterial);

            _perMaterialConstantBufferValue = new CommonStructs.ConstBufferPerMaterialDeferredStruct()
            {
                AlbedoColor = new Vector4(CachedMaterial.PropertyBlock.AlbedoColor,
                    CachedMaterial.PropertyBlock.AlphaValue),
                EmissiveColor = Vector4.Zero,

                MetallicValue = CachedMaterial.PropertyBlock.MetallicValue,
                RoughnessValue = CachedMaterial.PropertyBlock.RoughnessValue,
                SpecularValue = 0,
                Unlit = 0,

                TextureTiling = CachedMaterial.PropertyBlock.Tile,
                TextureShift = CachedMaterial.PropertyBlock.Shift,

                OptionsMask0 = CommonStructs.CommonStructsHelper.FloatMaskValue(
                    CachedMaterial.HasAlbedoMap, CachedMaterial.HasMetallicMap,
                   false, CachedMaterial.HasRoughnessMap),
                OptionsMask1 = CommonStructs.CommonStructsHelper.FloatMaskValue(
                    CachedMaterial.HasNormalMap, false,
                    CachedMaterial.HasOcclusionMap, false),
                AlphaClip = 0.5f,
            };

            // TODO: Translucent Pixel shader
            SetPixelShader(_currentPass == Pass.Forward ? "TestShader" : "FillGBufferPS");

            D3DUtils.WriteToDynamicBuffer(GetContext, _perMaterialConstantBuffer, _perMaterialConstantBufferValue);

            if (CachedMaterial.HasSampler)
            {
                //SetSamplerState(0, _cachedMaterial.GetSamplerType);
                SetSamplerState(0, SamplerStateType.TrilinearWrap);
                var textures = new[]
                {
                    CachedMaterial.HasAlbedoMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset) : null,
                    CachedMaterial.HasMetallicMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.MetallicMapAsset) : null,
                    null, // Emissive
                    CachedMaterial.HasRoughnessMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.RoughnessMapAsset) : null,
                    CachedMaterial.HasNormalMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.NormalMapAsset) : null,
                    null, // Specular
                    CachedMaterial.HasOcclusionMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.OcclusionMapAsset) : null,
                };
                GetContext.PixelShader.SetShaderResources(0, 7, textures);
                _isNullSrv = false;
            }
            else
            {
                ResetShaderResourcesViews();
                SetSamplerState(0, SamplerStateType.TrilinearWrap);
            }
        }

        private bool _isNullSrv = true;

        private readonly ShaderResourceView[] _null7ShaderResourceViews = { null, null, null, null, null, null, null, };

        private void ResetShaderResourcesViews()
        {
            if (_isNullSrv)
            {
                return;
            }
            _isNullSrv = true;
            GetContext.PixelShader.SetShaderResources(0, 7, _null7ShaderResourceViews);
        }

        private static readonly Matrix4x4 SkySphereMatrix = Matrix4x4.CreateScale(Vector3.One * 5000f) *
                                                         Matrix4x4.CreateFromYawPitchRoll(0, MathUtil.Pi * 0.5f, 0);
        private void DrawSkySphere(Vector3 cameraPosition)
        {
            SetMesh(MeshAssetsLoader.GetGuid("SkyDomeMesh"));
            SetPixelShader("FillGBufferSkyboxPS");

            SetSamplerState(0, SamplerStateType.TrilinearWrap);
            var textures = new[]
            {
                GetSharedItems.LoadTextureShaderResourceView(MaterialInstance.GetSkySphereMaterial().AlbedoMapAsset, true),
                null, null, // Emissive
                null, null, null, // Specular
                null,
            };

            _isNullSrv = false;
            GetContext.PixelShader.SetShaderResources(0, 7, textures);

            _perMaterialConstantBufferValue = new CommonStructs.ConstBufferPerMaterialDeferredStruct()
            {
                Unlit = 1,
                OptionsMask0 = CommonStructs.CommonStructsHelper.FloatMaskValue(true, false, false, false),
                OptionsMask1 = CommonStructs.CommonStructsHelper.FloatMaskValue(false, false, false, true),
            };

            Matrix4x4.Invert(SkySphereMatrix * Matrix4x4.CreateTranslation(cameraPosition), out var worldInvert);

            _perObjectConstantBufferValue = new CommonStructs.ConstBufferPerObjectDeferredStruct()
            {
                World = SkySphereMatrix * Matrix4x4.CreateTranslation(cameraPosition),
                WorldInverse = Matrix4x4.Transpose(worldInvert),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _perObjectConstantBuffer, _perObjectConstantBufferValue);
            D3DUtils.WriteToDynamicBuffer(GetContext, _perMaterialConstantBuffer, _perMaterialConstantBufferValue);
            DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
        }
    }
    #endregion

    #region G-Buffer
    internal partial class DeferredD3D11RenderPath
    {
        private RenderTargetPack _albedoAndMetallicTarget;
        private RenderTargetPack _emissiveAndRoughnessTarget;
        private RenderTargetPack _packedNormalsTarget;
        private RenderTargetPack _specularAoUnlitNonShadowsTarget;
        private RenderTargetPack _hdrTarget;
        private DepthStencilTargetPack _depthStencilTarget;
        private DepthStencilView _readonlyDepthStencilView;

        private RenderTargetView[] _gBufferTargets;

        private void InitGBuffer(int samples)
        {
            _albedoAndMetallicTarget = GetSharedItems.CreateRenderTarget("AlbedoAndMetallic",
                GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            _emissiveAndRoughnessTarget = GetSharedItems.CreateRenderTarget("EmissiveAndRoughness",
                GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);
            _packedNormalsTarget = GetSharedItems.CreateRenderTarget("PackedNormals",
                GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);//R11G11B10_Float
            _specularAoUnlitNonShadowsTarget = GetSharedItems.CreateRenderTarget("SpecularAOUnlitNonShadows",
                GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);

            if (HdrEnable)
            {
                _hdrTarget = GetSharedItems.CreateRenderTarget("HDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            }

            _depthStencilTarget = GetSharedItems.CreateDepthRenderTarget("DepthStencil",
                GetDisplay.Width, GetDisplay.Height, samples);

            ToDispose(_readonlyDepthStencilView = new DepthStencilView(GetDevice, _depthStencilTarget.Map, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = samples > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil,
            }));
            _readonlyDepthStencilView.DebugName = "ReadonlyDepthStencilView";

            _gBufferTargets = new[]
            {
                _albedoAndMetallicTarget.TargetView,
                _emissiveAndRoughnessTarget.TargetView,
                _packedNormalsTarget.TargetView,
                _specularAoUnlitNonShadowsTarget.TargetView,
            };

            InitDownsamplingResources();
        }

        private void ResizeGBuffer()
        {
            _albedoAndMetallicTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _emissiveAndRoughnessTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _packedNormalsTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _specularAoUnlitNonShadowsTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _depthStencilTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);

            _readonlyDepthStencilView.Dispose();
            ToDispose(_readonlyDepthStencilView = new DepthStencilView(GetDevice, _depthStencilTarget.Map, new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = MsSamplesCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.ReadOnlyDepth | DepthStencilViewFlags.ReadOnlyStencil,
            }));
            _readonlyDepthStencilView.DebugName = "ReadonlyDepthStencilView";

            _gBufferTargets = new[]
            {
                _albedoAndMetallicTarget.TargetView,
                _emissiveAndRoughnessTarget.TargetView,
                _packedNormalsTarget.TargetView,
                _specularAoUnlitNonShadowsTarget.TargetView,
            };
            _hdrTarget.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            InitDownsamplingSizableResources();
        }
    }
    #endregion

    #region Buffers and shaders names
    internal partial class DeferredD3D11RenderPath
    {
        private CommonStructs.ConstBufferPerFrameDeferredStruct _perFrameConstantBufferValue;
        private CommonStructs.ConstBufferPerObjectDeferredStruct _perObjectConstantBufferValue;
        private CommonStructs.ConstBufferPerMaterialDeferredStruct _perMaterialConstantBufferValue;
        private CommonStructs.ConstBufferPerLightStruct _perLightConstantBufferValue;
        private CommonStructs.ConstBufferLightVolumeDomainShader _domainShaderConstantBufferValue;

        private Buffer _perFrameConstantBuffer;
        private Buffer _perObjectConstantBuffer;
        private Buffer _perMaterialConstantBuffer;
        private Buffer _perLightConstantBuffer;
        private Buffer _postProcessConstantBuffer;
        private Buffer _domainShaderConstantBuffer;

        private InputLayout _quadLayout;

        private void CreateBuffers()
        {
            var bufferDescription = new BufferDescription
            {
                Usage = ResourceUsage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
                SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameDeferredStruct>(),
            };

            ToDispose(_perFrameConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perFrameConstantBuffer.DebugName = "PerFrameConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectDeferredStruct>();
            ToDispose(_perObjectConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perObjectConstantBuffer.DebugName = "PerObjectConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerMaterialDeferredStruct>();
            ToDispose(_perMaterialConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perMaterialConstantBuffer.DebugName = "PerMaterialConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerLightStruct>();
            ToDispose(_perLightConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _perLightConstantBuffer.DebugName = "PerLightConstantBuffer";

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPostProcessStruct>();
            ToDispose(_postProcessConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _postProcessConstantBuffer.DebugName = "PostProcessConstantBuffer";

            var postProcessConstantBufferValue = new CommonStructs.ConstBufferPostProcessStruct()
            {
                MiddleGrey = MiddleGray,
                LumWhiteSqr = (WhiteLum * MiddleGray) * (WhiteLum * MiddleGray),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _postProcessConstantBuffer, postProcessConstantBufferValue);

            var mainBuffersBatch = new[] {
                _perFrameConstantBuffer,
                _perObjectConstantBuffer,
                _perMaterialConstantBuffer,
                _perLightConstantBuffer,
                _postProcessConstantBuffer,
            };

            GetContext.VertexShader.SetConstantBuffers(0, mainBuffersBatch.Length, mainBuffersBatch);
            GetContext.PixelShader.SetConstantBuffers(0, mainBuffersBatch.Length, mainBuffersBatch);

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferLightVolumeDomainShader>();
            ToDispose(_domainShaderConstantBuffer = new Buffer(GetDevice, bufferDescription));
            _domainShaderConstantBuffer.DebugName = "DomainShaderConstantBuffer";
            GetContext.DomainShader.SetConstantBuffer(0, _domainShaderConstantBuffer);

            D3D11ShaderLoader.GetShader<VertexShader>("ScreenQuadVS", out var vsSignature);
            ToDispose(_quadLayout = new InputLayout(GetDevice, vsSignature, new[] {
                new InputElement("SV_VertexID", 0, Format.R32_UInt, 0, 0),
            }));
            _quadLayout.DebugName = "QuadLayout";
        }

        private string _lightPassPsName;
        private string _lightPassDirPsName;
        private string _lightPassPointPsName;
        private string _lightPassPointQuadPsName;
        private string _lightPassSpotPsName;
        private string _lightPassSpotQuadPsName;
        private string _screenQuadPsName;

        private void InitShadersNames()
        {
            _lightPassPsName = MsaaEnable ? "MSAA_LightPassPS" : "LightPassPS";
            _lightPassDirPsName = MsaaEnable ? "MSAA_LightPassDirectionalPS" : "LightPassDirectionalPS";
            _lightPassPointPsName = MsaaEnable ? "MSAA_LightPassPointPS" : "LightPassPointPS";
            _lightPassPointQuadPsName = MsaaEnable ? "MSAA_LightPassPointQuadPS" : "LightPassPointQuadPS";
            // LightPassSpotPSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassSpotQuadPSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassCapsulePSName = EnabledMSAA ? "MSAA_" : "";
            // LightPassCapsuleQuadPSName = EnabledMSAA ? "MSAA_" : "";

            _screenQuadPsName = MsaaEnable ? "MSAA_ScreenQuadPS" : "ScreenQuadPS";
        }
    }
    #endregion
}
