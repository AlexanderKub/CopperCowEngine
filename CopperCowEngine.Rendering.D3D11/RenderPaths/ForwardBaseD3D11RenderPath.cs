using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Numerics;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    public class PostProcessSettings
    {
        public bool MotionBlurEnabled;
    }

    internal class ForwardBaseD3D11RenderPath : BaseD3D11RenderPath
    {
        protected PostProcessSettings PostProcessSettings;

        #region Targets

        private RenderTargetPack _velocityTarget;
        private RenderTargetPack _screenQuadTarget;
        private DepthStencilTargetPack _shadowMapsAtlasDepthTarget;
        private RenderTargetPack _shadowMapsAtlasTarget;

        private const int ShadowAtlasSize = 2048; //8192;

        protected virtual void InitTargets(int samples)
        {
            if (EnabledHdr)
            {
                _screenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadHDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16B16A16_Float, samples);
            }
            else
            {
                _screenQuadTarget = GetSharedItems.CreateRenderTarget("ScreenQuadLDR",
                    GetDisplay.Width, GetDisplay.Height, Format.R8G8B8A8_UNorm, samples);
            }

            if (PostProcessSettings.MotionBlurEnabled)
            {
                _velocityTarget = GetSharedItems.CreateRenderTarget("Velocity",
                    GetDisplay.Width, GetDisplay.Height, Format.R16G16_Float, samples);
            }
            _shadowMapsAtlasTarget = GetSharedItems.CreateRenderTarget("ShadowMapsAtlas",
                    ShadowAtlasSize, ShadowAtlasSize, Format.R32G32B32A32_Float, 1);
            _shadowMapsAtlasDepthTarget = GetSharedItems.CreateDepthRenderTarget("ShadowMapsAtlas", 
                ShadowAtlasSize, ShadowAtlasSize, 1);
        }

        public override void Resize()
        {
            _screenQuadTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
            _velocityTarget?.Resize(GetDevice, GetDisplay.Width, GetDisplay.Height);
        }
        #endregion

        #region Buffers
        private CommonStructs.ConstBufferPerObjectStruct _perObjectConstBuffer;
        private CommonStructs.ConstBufferPerFrameStruct _perFrameConstBuffer;
        private CommonStructs.ConstBufferShadowMapLightStruct[] _shadowLightsDataBuffer;
        private CommonStructs.ConstBufferShadowDepthStruct _perObjectShadowPassConstBuffer;

        protected Buffer PerObjConstantBuffer;
        protected Buffer PerFrameConstantBuffer;
        protected Buffer ShadowLightsDataBuffer;
        protected Buffer PerObjectShadowPassConstBuffer;
        protected InputLayout QuadLayout;


        protected virtual void CreateBuffers()
        {
            var bufferDescription = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0,
                SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>(),
            };

            ToDispose(PerObjConstantBuffer = new Buffer(GetDevice, bufferDescription));

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>();
            ToDispose(PerFrameConstantBuffer = new Buffer(GetDevice, bufferDescription));

            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferShadowDepthStruct>();
            ToDispose(PerObjectShadowPassConstBuffer = new Buffer(GetDevice, bufferDescription));

            _shadowLightsDataBuffer = new CommonStructs.ConstBufferShadowMapLightStruct[3];
            bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferDirLightStruct>() * 3;
            ToDispose(ShadowLightsDataBuffer = new Buffer(GetDevice, bufferDescription));

            D3D11ShaderLoader.GetShader<VertexShader>("ScreenQuadVS", out var vsSignature);
            ToDispose(QuadLayout = new InputLayout(GetDevice, vsSignature, new[] {
                new InputElement("SV_VertexID", 0, Format.R32G32B32_Float, 0, 0),
            }));
        }
        #endregion

        public override void Init(D3D11RenderBackend renderBackend)
        {
            PostProcessSettings = new PostProcessSettings()
            {
                MotionBlurEnabled = false,
            };

            base.Init(renderBackend);
            InitTargets(MsSamplesCount);
            CreateBuffers();
        }

        public override void Draw(StandardFrameData frameData)
        {
            //ShadowMapsPass(frameData);
            DepthPrePass(frameData);
            ColourPass(frameData);
            //ScreenQuadPass(frameData);
        }

        protected enum Pass : byte
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

            GetContext.ClearRenderTargetView(_shadowMapsAtlasTarget.View, Color.White);
            GetContext.ClearDepthStencilView(
                _shadowMapsAtlasDepthTarget.View,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f, 0
            );
            GetContext.OutputMerger.SetTargets(_shadowMapsAtlasDepthTarget.View, _shadowMapsAtlasTarget.View);

            SetDepthStencilState(DepthStencilStateType.Less);
            // TODO: select right cull mode
            SetRasterizerState(RasterizerStateType.SolidFrontCull);

            var atlasViewport = new Viewport(0, 0, ShadowAtlasSize, ShadowAtlasSize, 0.0f, 1.0f);
            GetContext.Rasterizer.SetViewport(atlasViewport);

            SetVertexShader("DepthShadowsVS");
            GetContext.VertexShader.SetConstantBuffer(0, PerObjectShadowPassConstBuffer);
            SetPixelShader("DepthShadowsPS");
            //SetNullPixelShader();

            var meshName = "";
            var materialName = "";
            var isMaskedSubPass = false;

            foreach (var light in frameData.LightsList.Where(light => light.IsCastShadows))
            {
                foreach (var rendererData in frameData.PerLightRenderers[light.Index].Select(index => frameData.RenderersList[index]))
                {
                    if (meshName != rendererData.MeshName)
                    {
                        meshName = rendererData.MeshName;
                        SetMesh(meshName);
                    }

                    if (materialName != rendererData.MaterialName)
                    {
                        materialName = rendererData.MaterialName;
                        SetMaterial(materialName);
                        /*
                        if (!IsMaskedSubPass)
                        {
                            if (_cachedMaterial.MetaMaterial.blendMode == MetaMaterial.BlendMode.Masked)
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
                                GetSharedItems.LoadTextureShaderResourceView(_cachedMaterial.AlbedoMapAsset));
                        }
                        */
                        if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                        {
                            break; // Break on translucent objects
                        }
                    }

                    _perObjectShadowPassConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                    _perObjectShadowPassConstBuffer.ViewProjectionMatrix = light.ViewProjection;
                    GetContext.UpdateSubresource(ref _perObjectShadowPassConstBuffer, PerObjectShadowPassConstBuffer);

                    DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
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

        private readonly List<RendererData> _dynamicMeshes = new List<RendererData>();

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
            SetDepthStencilState(DepthStencilStateType.Greater);
            SetRasterizerState(RasterizerStateType.SolidBackCull);

            SetVertexShader("ForwardPlusPosOnlyVS");
            SetNullPixelShader();
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            Matrix4x4.Invert(frameData.CamerasList[0].Projection, out var projectionInverse);
            Matrix4x4.Invert(frameData.CamerasList[0].View, out var viewInverse);

            _perFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
            {
                Projection = frameData.CamerasList[0].Projection,
                ProjectionInv = projectionInverse,
                ViewInv = viewInverse,
                PreviousView = frameData.CamerasList[0].PreviousView,
                CameraPos = frameData.CamerasList[0].Position,
                CameraForward = new Vector4(frameData.CamerasList[0].Forward, 1),
                AlphaTest = 0.5f,
                WindowWidth = (uint)GetDisplay.Width,
                WindowHeight = (uint)GetDisplay.Height,
                CurrentFps = (uint)(1.0f / 0.01f),//Time.DeltaTime
            };
            GetContext.UpdateSubresource(ref _perFrameConstBuffer, PerFrameConstantBuffer);

            GetContext.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
            GetContext.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
            //GetContext.PixelShader.SetConstantBuffer(2, LightsDataBuffer);
            GetContext.PixelShader.SetConstantBuffer(3, ShadowLightsDataBuffer);
            GetContext.UpdateSubresource(ref _perFrameConstBuffer, PerFrameConstantBuffer);

            var meshName = "";
            var materialName = "";
            var isMaskedSubPass = false;
            if (PostProcessSettings.MotionBlurEnabled)
            {
                GetContext.ClearRenderTargetView(_velocityTarget.View, Color.Yellow);
                _dynamicMeshes.Clear();
            }

            foreach (var rendererData in frameData.PerCameraRenderers[0].Select(index => frameData.RenderersList[index]))
            {
                if (PostProcessSettings.MotionBlurEnabled && rendererData.IsDynamic)
                {
                    _dynamicMeshes.Add(rendererData);
                    continue;
                }

                if (meshName != rendererData.MeshName)
                {
                    meshName = rendererData.MeshName;
                    SetMesh(meshName);
                }

                if (materialName != rendererData.MaterialName)
                {
                    materialName = rendererData.MaterialName;
                    SetMaterial(materialName);
                    if (!isMaskedSubPass)
                    {
                        if (CachedMaterial.MetaMaterial.BlendMode == MaterialMeta.BlendModeType.Masked)
                        {
                            GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, GetDisplay.RenderTargetViewRef);
                            SetBlendState(EnabledMsaa
                                ? BlendStateType.DepthOnlyAlphaToCoverage
                                : BlendStateType.DepthOnlyAlphaTest);
                            DepthMaskedSubPath(false);
                            isMaskedSubPass = true;
                        }
                    }
                    else
                    {
                        GetContext.PixelShader.SetShaderResource(0,
                            GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset));
                    }
                    if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                    {
                        break; // Break on translucent objects
                    }
                }

                _perObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                _perObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                _perObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                _perObjectConstBuffer.TextureTiling = CachedMaterial.PropertyBlock.Tile;
                _perObjectConstBuffer.TextureShift = CachedMaterial.PropertyBlock.Shift;
                GetContext.UpdateSubresource(ref _perObjectConstBuffer, PerObjConstantBuffer);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }

            if (!PostProcessSettings.MotionBlurEnabled || _dynamicMeshes.Count == 0)
            {
                return;
            }

            GetContext.OutputMerger.SetTargets(GetDisplay.DepthStencilViewRef, _velocityTarget.View);
            SetBlendState(BlendStateType.Opaque);
            SetRasterizerState(RasterizerStateType.SolidBackCull);

            SetVertexShader("ForwardPlusPosOnlyVS");
            SetPixelShader("VelocityPS");

            isMaskedSubPass = false;
            foreach (var dRendererData in _dynamicMeshes)
            {
                if (meshName != dRendererData.MeshName)
                {
                    meshName = dRendererData.MeshName;
                    SetMesh(meshName);
                }

                if (materialName != dRendererData.MaterialName)
                {
                    materialName = dRendererData.MaterialName;
                    SetMaterial(materialName);

                    if (!isMaskedSubPass)
                    {
                        if (CachedMaterial.MetaMaterial.BlendMode == MaterialMeta.BlendModeType.Masked)
                        {
                            DepthMaskedSubPath(true);
                            isMaskedSubPass = true;
                        }
                    }
                    else
                    {
                        GetContext.PixelShader.SetShaderResource(0,
                            GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset));
                    }

                    if (CachedMaterial.MetaMaterial.BlendMode > MaterialMeta.BlendModeType.Masked)
                    {
                        break; // Break on translucent objects
                    }
                }

                _perObjectConstBuffer.WorldMatrix = dRendererData.TransformMatrix;
                _perObjectConstBuffer.WorldViewMatrix = dRendererData.TransformMatrix * frameData.CamerasList[0].View;
                _perObjectConstBuffer.WorldViewProjMatrix = dRendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                _perObjectConstBuffer.PreviousWorldViewProjMatrix = dRendererData.PreviousTransformMatrix * frameData.CamerasList[0].PreviousViewProjection;
                GetContext.UpdateSubresource(ref _perObjectConstBuffer, PerObjConstantBuffer);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }

        protected void DepthMaskedSubPath(bool isVelocityPass)
        {
            SetRasterizerState(RasterizerStateType.SolidNoneCull);
            GetContext.PixelShader.SetSampler(0,
                GetSharedItems.GetSamplerState(SamplerStateType.AnisotropicWrap));

            SetVertexShader("ForwardPlusPosTexVS");
            SetPixelShader(!isVelocityPass ? "ForwardPlusPosTexPS" : "MaskedVelocityPS");

            GetContext.PixelShader.SetShaderResource(0,
                GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset));
            GetContext.PixelShader.SetSampler(0,
                GetSharedItems.GetSamplerState(SamplerStateType.BilinearClamp));
        }

        protected void ColourPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.ColourPass;
            GetContext.OutputMerger.SetRenderTargets(GetDisplay.DepthStencilViewRef, _screenQuadTarget.View);
            SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);

            SetVertexShader("CommonVS");
            GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;

            // Draw scene
            var meshName = "";
            var materialName = "";
            var materialQueue = -999999;

            //_shadowLightsDataBuffer[0].LightViewProjectionMatrix = frameData.LightsList[0].ViewProjection;
            //_shadowLightsDataBuffer[0].LeftTop = Vector2.Zero;
            //_shadowLightsDataBuffer[0].RightBottom = Vector2.One;
            GetContext.UpdateSubresource(_shadowLightsDataBuffer, ShadowLightsDataBuffer);
            foreach (var rendererData in frameData.PerCameraRenderers[0].Select(index => frameData.RenderersList[index]))
            {
                if (materialName != rendererData.MaterialName)
                {
                    materialName = rendererData.MaterialName;
                    SetMaterial(materialName, materialQueue != rendererData.MaterialQueue);
                    materialQueue = rendererData.MaterialQueue;
                }

                if (meshName != rendererData.MeshName)
                {
                    meshName = rendererData.MeshName;
                    SetMesh(meshName);
                }

                _perObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
                _perObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
                _perObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
                GetContext.UpdateSubresource(ref _perObjectConstBuffer, PerObjConstantBuffer);

                DX_DrawIndexed(CachedMesh.IndexCount, 0, 0);
            }
        }

        protected void ScreenQuadPass(StandardFrameData frameData)
        {
            CurrentPass = Pass.ScreenQuadPass;
            GetContext.OutputMerger.SetRenderTargets(null, GetDisplay.RenderTargetViewRef);

            if (PostProcessSettings.MotionBlurEnabled)
            {
                // TODO: shader with motion blur
                GetContext.PixelShader.SetShaderResource(0, _screenQuadTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(1, _velocityTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(2, GetDisplay.DepthStencilShaderResourceViewRef);
            } else
            {
                SetVertexShader("ScreenQuadVS");
                GetContext.InputAssembler.InputLayout = QuadLayout;
                SetPixelShader("ScreenQuadPS");
                GetContext.PixelShader.SetShaderResource(0, _screenQuadTarget.ResourceView);
                //GetContext.PixelShader.SetShaderResource(0, ShadowMapsAtlasTarget.ResourceView);
                GetContext.PixelShader.SetShaderResource(2, GetDisplay.DepthStencilShaderResourceViewRef);
            }

            GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(SamplerStateType.BilinearClamp));

            DX_Draw(4, 0);
        }

        #region Render loop helpers
        protected SharedRenderItemsStorage.CachedMesh CachedMesh;

        protected void SetMesh(string meshName)
        {
            CachedMesh = GetSharedItems.GetMesh(meshName);
            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(CachedMesh.VertexBuffer, 96, 0));
            GetContext.InputAssembler.SetIndexBuffer(CachedMesh.IndexBuffer, Format.R32_UInt, 0);
            GetContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        protected MaterialInstance CachedMaterial;

        protected void SetMaterial(string materialName, bool changeStates = false)
        {
            CachedMaterial = materialName == "SkySphereMaterial" ? MaterialInstance.GetSkySphereMaterial() : MaterialLoader.LoadMaterial(materialName);

            if (CurrentPass == Pass.DepthPrePass || CurrentPass == Pass.ShadowMapsPass)
            {
                return;
            }

            if (changeStates)
            {
                SetMergerStates(CachedMaterial.MetaMaterial);
            }

            // TODO: setup shader from material meta. bind buffers and textures by shader.
            if (materialName == "SkySphereMaterial")
            {
                SetPixelShader("FwdSkySpherePS");
            }
            else
            {
                if (CachedMaterial.MetaMaterial.BlendMode >= MaterialMeta.BlendModeType.Translucent)
                {
                    SetPixelShader("TestShader");
                }
                else
                {
                    if (SetPixelShader("PBRForwardPS"))
                    {
                        GetContext.PixelShader.SetShaderResource(5, GetSharedItems.PreFilteredMap);
                        GetContext.PixelShader.SetShaderResource(6, GetSharedItems.IrradianceMap);
                        GetContext.PixelShader.SetShaderResource(7, _shadowMapsAtlasTarget.ResourceView);
                        GetContext.PixelShader.SetSampler(1, GetSharedItems.GetSamplerState(SamplerStateType.ShadowMap));
                    }
                }
            }

            _perObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct
            {
                TextureTiling = CachedMaterial.PropertyBlock.Tile,
                TextureShift = CachedMaterial.PropertyBlock.Shift,

                AlbedoColor = new Vector4(CachedMaterial.PropertyBlock.AlbedoColor, CachedMaterial.PropertyBlock.AlphaValue),
                RoughnessValue = CachedMaterial.PropertyBlock.RoughnessValue,
                MetallicValue = CachedMaterial.PropertyBlock.MetallicValue,

                OptionsMask0 = CommonStructs.FloatMaskValue(
                    CachedMaterial.HasAlbedoMap, CachedMaterial.HasNormalMap,
                    CachedMaterial.HasRoughnessMap, CachedMaterial.HasMetallicMap),
                OptionsMask1 = CommonStructs.FloatMaskValue(
                    CachedMaterial.HasOcclusionMap, false, false, false),
                Filler = Vector2.Zero,
            };

            if (materialName == "SkySphereMaterial")
            {
                _perObjectConstBuffer.OptionsMask1 = CommonStructs.FloatMaskValue(
                    CachedMaterial.HasOcclusionMap, true, true, false);
            }

            if (CachedMaterial.HasSampler)
            {
                GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState((int)CachedMaterial.TexturesSampler));
                ShaderResourceView[] textures = {
                    CachedMaterial.HasAlbedoMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.AlbedoMapAsset) : null,
                    CachedMaterial.HasNormalMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.NormalMapAsset) : null,
                    CachedMaterial.HasRoughnessMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.RoughnessMapAsset) : null,
                    CachedMaterial.HasMetallicMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.MetallicMapAsset) : null,
                    CachedMaterial.HasOcclusionMap ? GetSharedItems.LoadTextureShaderResourceView(CachedMaterial.OcclusionMapAsset) : null,
                };
                GetContext.PixelShader.SetShaderResources(0, 5, textures);
            }
            else
            {
                GetContext.PixelShader.SetSampler(0, null);
                GetContext.PixelShader.SetShaderResources(0, 5, null, null, null, null, null);
            }
        }
        #endregion
    }
}
