// using SharpDX;
// using SharpDX.DXGI;
// using SharpDX.D3DCompiler;
// using SharpDX.Direct3D11;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using CopperCowEngine.Rendering.D3D11.Shared;
// using CopperCowEngine.Rendering.Data;
// using Buffer = SharpDX.Direct3D11.Buffer;
// using SharpDX.Direct3D;
// using CopperCowEngine.Rendering.ShaderGraph;
//
// namespace CopperCowEngine.Rendering.D3D11.RenderPaths
// {
//     internal class TiledForwardD3D11RenderPath : BaseD3D11RenderPath
//     {
//         private enum Pass
//         {
//             DepthPrePass,
//             LightCulling,
//             Colour,
//         }
//         
//         private Pass CurrentPass;
//         private CommonStructs.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
//         private CommonStructs.ConstBufferPerFrameStruct m_PerFrameConstBuffer;
//         private CommonStructs.ConstBufferDirLightStruct[] m_DirLightsConstBuffer;
//
//         private Buffer PerObjConstantBuffer;
//         private Buffer PerFrameConstantBuffer;
//
//         private Buffer DirLightBuffer;
//         private Buffer LightCenterAndRadiusBuffer;
//         private Buffer LightColorBuffer;
//         private Buffer LightParamsBuffer;
//         private Buffer LightIndexBuffer;
//         private ShaderResourceView LightCenterAndRadiusSRV;
//         private ShaderResourceView LightColorSRV;
//         private ShaderResourceView LightParamsSRV;
//         private ShaderResourceView LightIndexSRV;
//         private UnorderedAccessView LightIndexURV;
//
//         Texture2D hdrTextureTarget;
//         RenderTargetView hdrRenderTargetView;
//         ShaderResourceView hdrSRV;
//
//         #region Tiles
//         private const int MAX_NUM_LIGHTS = 2 * 1024;
//         private const int TILE_RES = 16;
//         private const int MAX_NUM_LIGHTS_PER_TILE = 544;
//
//         private int GetNumTilesX()
//         {
//             return (int)((GetDisplay.Width + TILE_RES - 1) / (float)TILE_RES);
//         }
//
//         private int GetNumTilesY()
//         {
//             return (int)((GetDisplay.Height + TILE_RES - 1) / (float)TILE_RES);
//         }
//
//         private const int kAdjustmentMultipier = 32;
//         private int GetMaxNumLightsPerTile()
//         {
//             int uHeight = (GetDisplay.Height > 1080) ? 1080 : GetDisplay.Height;
//
//             return (MAX_NUM_LIGHTS_PER_TILE - (kAdjustmentMultipier * (uHeight / 120)));
//         }
//         #endregion
//
//         //private Format m_HDRformar = Format.R32G32B32A32_Float;
//         private Format m_HDRformar = Format.R16G16B16A16_Float;
//
//         public override void Init(D3D11RenderBackend renderBackend)
//         {
//             base.Init(renderBackend);
//
//             // Create constant buffers
//             BufferDescription bufferDescription = new BufferDescription()
//             {
//                 SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerObjectStruct>(),
//                 Usage = ResourceUsage.Default,
//                 BindFlags = BindFlags.ConstantBuffer,
//                 CpuAccessFlags = CpuAccessFlags.None,
//                 OptionFlags = ResourceOptionFlags.None,
//                 StructureByteStride = 0,
//             };
//             PerObjConstantBuffer = new Buffer(GetDevice, bufferDescription);
//             bufferDescription.SizeInBytes = Utilities.SizeOf<CommonStructs.ConstBufferPerFrameStruct>();
//             PerFrameConstantBuffer = new Buffer(GetDevice, bufferDescription);
//
//             // Bind constant buffers
//             GetContext.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
//             GetContext.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
//             GetContext.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
//             GetContext.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
//
//             InitLightsBuffers();
//
//             Texture2DDescription textureDescription = new Texture2DDescription()
//             {
//                 Width = GetDisplay.Width,
//                 Height = GetDisplay.Height,
//                 MipLevels = 1,
//                 ArraySize = 1,
//                 Format = Format.R16G16_Float,
//                 SampleDescription = new SampleDescription()
//                 {
//                     Count = 1,
//                 },
//                 Usage = ResourceUsage.Default,
//                 BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
//             };
//             RenderTargetViewDescription renderTargetDescription = new RenderTargetViewDescription()
//             {
//                 Format = Format.R16G16_Float,
//                 Dimension = RenderTargetViewDimension.Texture2D,
//             };
//             ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription()
//             {
//                 Format = Format.R16G16_Float,
//                 Dimension = ShaderResourceViewDimension.Texture2D,
//             };
//
//             shaderResourceDescription.Texture2D.MostDetailedMip = 0;
//             shaderResourceDescription.Texture2D.MipLevels = 1;
//             textureDescription.Format = m_HDRformar;
//             renderTargetDescription.Format = m_HDRformar;
//             shaderResourceDescription = new ShaderResourceViewDescription()
//             {
//                 Format = m_HDRformar,
//                 Dimension = ShaderResourceViewDimension.Texture2D,
//             };
//             shaderResourceDescription.Texture2D.MostDetailedMip = 0;
//             shaderResourceDescription.Texture2D.MipLevels = 1;
//
//             hdrTextureTarget = new Texture2D(GetDevice, textureDescription);
//             hdrSRV = new ShaderResourceView(GetDevice, hdrTextureTarget, shaderResourceDescription);
//             hdrRenderTargetView = new RenderTargetView(GetDevice, hdrTextureTarget, renderTargetDescription);
//         }
//
//         public override void Draw(StandardFrameData frameData)
//         {
//             // Clear
//             DepthStencilView depthStencilView = GetDisplay.DepthStencilViewRef;
//             RenderTargetView renderTargetView = GetDisplay.RenderTargetViewRef;
//             GetContext.ClearRenderTargetView(renderTargetView, Color.Gray);
//             GetContext.ClearDepthStencilView(depthStencilView,
//                 DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0.0f, 0);
//
//             DepthPrePass(frameData, depthStencilView, renderTargetView);
//             LightCullingPass(frameData);
//             ColourPass(frameData, depthStencilView, renderTargetView);
//             //TODO: colour pass
//         }
//
//         private void DepthPrePass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
//         {
//             CurrentPass = Pass.DepthPrePass;
//             // Setup targets and states
//             GetContext.OutputMerger.SetTargets(depthStencilView, (RenderTargetView)null);
//
//             SetDepthStencilState(DepthStencilStateType.Greater);
//
//             SetRasterizerState(RasterizerStates.SolidBackCull);
//
//             GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;
//
//             // Setup vertex shader
//             SetVertexShader("ForwardPlusPosOnlyVS");
//
//             // Cleanup pixel shader
//             SetNullPixelShader();
//
//             m_PerFrameConstBuffer = new CommonStructs.ConstBufferPerFrameStruct()
//             {
//                 Projection = frameData.CamerasList[0].Projection,
//                 ProjectionInv = Matrix.Invert(frameData.CamerasList[0].Projection),
//                 CameraPos = frameData.CamerasList[0].Position,
//                 AlphaTest = 0.5f,
//                 WindowWidth = (uint)GetDisplay.Width,
//                 WindowHeight = (uint)GetDisplay.Height,
//             };
//             GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
//
//             string MeshName = "";
//             string MaterialName = "";
//             bool IsOpaquePass = true;
//             foreach (var rendererData in frameData.RenderersList)
//             {
//                 if (MeshName != rendererData.MeshName)
//                 {
//                     MeshName = rendererData.MeshName;
//                     SetMesh(MeshName);
//                 }
//
//                 if (MaterialName != rendererData.MaterialName)
//                 {
//                     MaterialName = rendererData.MaterialName;
//                     SetMaterial(MaterialName);
//                     if (IsOpaquePass)
//                     {
//                         if (CurrentMaterialInstance.MetaMaterial.blendMode == MaterialMeta.BlendMode.Masked)
//                         {
//                             GetContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
//                             SetRasterizerState(RasterizerStates.SolidNoneCull);
//                             GetContext.PixelShader.SetSampler(0,
//                                 GetSharedItems.GetSamplerState(Material.SamplerType.AnisotropicWrap));
//                             //if (msaa) DepthOnlyAlphaToCoverageState
//                             SetBlendState(BlendStates.DepthOnlyAlphaTest);
//
//                             SetVertexShader("ForwardPlusPosTexVS");
//                             SetPixelShader("ForwardPlusPosTexPS");
//                             GetContext.PixelShader.SetShaderResource(0,
//                                 GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset));
//                             GetContext.PixelShader.SetSampler(0, 
//                                 GetSharedItems.GetSamplerState(Material.SamplerType.BilinearClamp));
//                             IsOpaquePass = false;
//                         } 
//                     } else {
//                         GetContext.PixelShader.SetShaderResource(0,
//                             GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset));
//                     }
//                     if (CurrentMaterialInstance.MetaMaterial.blendMode > MaterialMeta.BlendMode.Masked) {
//                         break; // Break on translucent objects
//                     }
//                 }
//
//                 m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
//                 m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
//                 m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
//                 m_PerObjectConstBuffer.TextureTiling = CurrentMaterialInstance.PropertyBlock.Tile;
//                 m_PerObjectConstBuffer.TextureShift = CurrentMaterialInstance.PropertyBlock.Shift;
//                 GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);
//
//                 DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
//             }
//         }
//
//         private void LightCullingPass(StandardFrameData frameData)
//         {
//             CurrentPass = Pass.LightCulling;
//
//             UpdateLights(frameData.LightsList);
//             m_PerFrameConstBuffer.NumLights = _nonDirLightsCount;
//             m_PerFrameConstBuffer.DirLightsNum = _dirLightsCount;
//             m_PerFrameConstBuffer.MaxNumLightsPerTile = (uint)GetMaxNumLightsPerTile();
//             GetContext.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
//
//             m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct()
//             {
//                 WorldMatrix = Matrix.Identity,
//                 WorldViewMatrix = Matrix.Identity * frameData.CamerasList[0].View,
//                 WorldViewProjMatrix = Matrix.Identity * frameData.CamerasList[0].ViewProjection,
//             };
//             GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);
//             GetContext.ComputeShader.SetConstantBuffer(0, PerObjConstantBuffer);
//             GetContext.ComputeShader.SetConstantBuffer(1, PerFrameConstantBuffer);
//
//             GetContext.OutputMerger.SetRenderTargets(null, (RenderTargetView)null);
//
//             SetNullVertexShader();
//             SetNullPixelShader();
//             SetNullRasterizerState();
//             SetBlendState(BlendStates.Opaque);
//
//             GetContext.ComputeShader.Set(AssetsLoader.GetShader<ComputeShader>("LightCullingCS"));
//             GetContext.ComputeShader.SetShaderResource(0, LightCenterAndRadiusSRV);
//             GetContext.ComputeShader.SetShaderResource(1, GetDisplay.DepthStencilSRVRef);
//             GetContext.ComputeShader.SetUnorderedAccessView(0, LightIndexURV);
//
//             GetContext.Dispatch(GetNumTilesX(), GetNumTilesY(), 1);
//
//             GetContext.ComputeShader.Set(null);
//             GetContext.ComputeShader.SetShaderResource(0, null);
//             GetContext.ComputeShader.SetShaderResource(1, null);
//             GetContext.ComputeShader.SetUnorderedAccessView(0, null);
//         }
//
//         private void ColourPass(StandardFrameData frameData, DepthStencilView depthStencilView, RenderTargetView renderTargetView)
//         {
//             CurrentPass = Pass.Colour;
//
//             GetContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);
//             SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
//             SetRasterizerState(RasterizerStates.SolidBackCull);
//
//             GetContext.InputAssembler.InputLayout = GetSharedItems.StandardInputLayout;
//             SetVertexShader("CommonVS");
//             SetPixelShader("ForwardPlusScenePS");
//             GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(Material.SamplerType.BilinearWrap));
//
//             GetContext.UpdateSubresource(m_DirLightsConstBuffer, DirLightBuffer);
//             GetContext.PixelShader.SetConstantBuffer(2, DirLightBuffer);
//
//             // PreFiltered
//             GetContext.PixelShader.SetShaderResource(5, GetSharedItems.PreFilteredMap);
//             // Irradiance
//             GetContext.PixelShader.SetShaderResource(6, GetSharedItems.IrradianceMap);
//
//             //Lights data
//             GetContext.PixelShader.SetShaderResource(7, LightCenterAndRadiusSRV);
//             GetContext.PixelShader.SetShaderResource(8, LightParamsSRV);
//             GetContext.PixelShader.SetShaderResource(9, LightColorSRV);
//             GetContext.PixelShader.SetShaderResource(10, LightIndexSRV);
//
//             // Draw scene
//             string MeshName = "";
//             string MaterialName = "";
//             int MaterialQueue = -999999;
//             foreach (var rendererData in frameData.RenderersList)
//             {
//                 if (MaterialName != rendererData.MaterialName)
//                 {
//                     MaterialName = rendererData.MaterialName;
//                     SetMaterial(MaterialName, MaterialQueue != rendererData.MaterialQueue);
//                     MaterialQueue = rendererData.MaterialQueue;
//                 }
//
//                 if (MeshName != rendererData.MeshName)
//                 {
//                     MeshName = rendererData.MeshName;
//                     SetMesh(MeshName);
//                 }
//
//                 m_PerObjectConstBuffer.WorldMatrix = rendererData.TransformMatrix;
//                 m_PerObjectConstBuffer.WorldViewMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].View;
//                 m_PerObjectConstBuffer.WorldViewProjMatrix = rendererData.TransformMatrix * frameData.CamerasList[0].ViewProjection;
//                 GetContext.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);
//
//                 DX_DrawIndexed(m_CachedMesh.IndexCount, 0, 0);
//             }
//         }
//
//         private SharedRenderItemsStorage.CachedMesh m_CachedMesh;
//         private void SetMesh(string meshName)
//         {
//             m_CachedMesh = GetSharedItems.GetMesh(meshName);
//
//             GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_CachedMesh.vertexBuffer, 96, 0));
//             GetContext.InputAssembler.SetIndexBuffer(m_CachedMesh.indexBuffer, Format.R32_UInt, 0);
//             GetContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
//         }
//
//         private Material CurrentMaterialInstance;
//         private void SetMaterial(string materialName, bool changeStates = false)
//         {
//             if (materialName == "SkySphereMaterial") {
//                 CurrentMaterialInstance = Material.GetSkySphereMaterial();
//             } else {
//                 CurrentMaterialInstance = AssetsLoader.LoadMaterial(materialName);
//             }
//
//             if (CurrentPass == Pass.DepthPrePass) {
//                 return;
//             }
//
//             m_PerObjectConstBuffer = new CommonStructs.ConstBufferPerObjectStruct
//             {
//                 TextureTiling = CurrentMaterialInstance.PropertyBlock.Tile,
//                 TextureShift = CurrentMaterialInstance.PropertyBlock.Shift,
//
//                 AlbedoColor = new Vector4(CurrentMaterialInstance.PropertyBlock.AlbedoColor, CurrentMaterialInstance.PropertyBlock.AlphaValue),
//                 RoughnessValue = CurrentMaterialInstance.PropertyBlock.RoughnessValue,
//                 MetallicValue = CurrentMaterialInstance.PropertyBlock.MetallicValue,
//
//                 OptionsMask0 = floatMaskVal(CurrentMaterialInstance.HasAlbedoMap, CurrentMaterialInstance.HasNormalMap, CurrentMaterialInstance.HasRoughnessMap, CurrentMaterialInstance.HasMetallicMap),
//                 OptionsMask1 = floatMaskVal(CurrentMaterialInstance.HasOcclusionMap, false, false, false),
//                 /*renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere || renderer.SpecificType == Renderer.SpecificTypeEnum.Unlit
//                     || renderer.SpecificType == Renderer.SpecificTypeEnum.Wireframe,
//                 renderer.SpecificType == Renderer.SpecificTypeEnum.SkySphere, false),*/
//                 Filler = Vector2.Zero,
//             };
//
//             if (changeStates)
//             {
//                 SetMergerStates(CurrentMaterialInstance.MetaMaterial);
//             }
//
//             if (materialName == "SkySphereMaterial")
//             {
//                 m_PerObjectConstBuffer.OptionsMask1 = floatMaskVal(CurrentMaterialInstance.HasOcclusionMap, true, true, false);
//             }
//
//             if (CurrentMaterialInstance.HasSampler)
//             {
//                 GetContext.PixelShader.SetSampler(0, GetSharedItems.GetSamplerState(CurrentMaterialInstance.GetSamplerType));
//                 ShaderResourceView[] textures = new ShaderResourceView[]
//                 {
//                     CurrentMaterialInstance.HasAlbedoMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.AlbedoMapAsset) : null,
//                     CurrentMaterialInstance.HasNormalMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.NormalMapAsset) : null,
//                     CurrentMaterialInstance.HasRoughnessMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.RoughnessMapAsset) : null,
//                     CurrentMaterialInstance.HasMetallicMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.MetallicMapAsset) : null,
//                     CurrentMaterialInstance.HasOcclusionMap ? GetSharedItems.LoadTextureSRV(CurrentMaterialInstance.OcclusionMapAsset) : null,
//                 };
//                 GetContext.PixelShader.SetShaderResources(0, 5, textures);
//             }
//             else
//             {
//                 GetContext.PixelShader.SetSampler(0, null);
//                 GetContext.PixelShader.SetShaderResources(0, 5, (ShaderResourceView[])null);
//             }
//
//             if (materialName == "SkySphereMaterial") {
//                 SetPixelShader("FwdSkySpherePS");
//             } else {
//                 if (CurrentMaterialInstance.MetaMaterial.blendMode >= MaterialMeta.BlendMode.Translucent) {
//                     SetPixelShader("TestShader");
//                 } else {
//                     SetPixelShader("ForwardPlusScenePS");
//                 }
//             }
//         }
//
//         private Vector4 floatMaskVal(bool v0, bool v1, bool v2, bool v3)
//         {
//             return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
//         }
//
//         private void SetMergerStates(MaterialMeta meta)
//         {
//             switch (meta.BlendMode)
//             {
//                 case MaterialMeta.BlendMode.Opaque:
//                     SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
//                     SetBlendState(BlendStates.Opaque);
//                     break;
//                 case MaterialMeta.BlendMode.Masked:
//                     SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
//                     SetBlendState(BlendStates.AlphaEnabledBlending);
//                     break;
//                 case MaterialMeta.BlendMode.Translucent:
//                     SetDepthStencilState(DepthStencilStateType.GreaterAndDisableWrite);
//                     SetBlendState(BlendStates.AlphaEnabledBlending);
//                     GetContext.PixelShader.SetShaderResource(7, null);
//                     GetContext.PixelShader.SetShaderResource(8, null);
//                     GetContext.PixelShader.SetShaderResource(9, null);
//                     GetContext.PixelShader.SetShaderResource(10, null);
//                     break;
//                 case MaterialMeta.BlendMode.Additive:
//                     break;
//                 case MaterialMeta.BlendMode.Modulate:
//                     break;
//                 default:
//                     break;
//             }
//
//             switch (meta.CullMode)
//             {
//                 case MaterialMeta.CullMode.Front:
//                     SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeFrontCull
//                         : RasterizerStates.SolidFrontCull);
//                     break;
//                 case MaterialMeta.CullMode.Back:
//                     SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeBackCull
//                         : RasterizerStates.SolidBackCull);
//                     break;
//                 case MaterialMeta.CullMode.None:
//                     SetRasterizerState(meta.Wireframe ? RasterizerStates.WireframeNoneCull
//                         : RasterizerStates.SolidNoneCull);
//                     break;
//                 default:
//                     break;
//             }
//         }
//
//         public override void Resize()
//         {
//             base.Resize();
//             int NumTiles = GetNumTilesX() * GetNumTilesY();
//             int MaxNumLightsPerTile = GetMaxNumLightsPerTile();
//
//             LightIndexBuffer?.Dispose();
//             LightIndexBuffer = new Buffer(GetDevice, new BufferDescription() {
//                 Usage = ResourceUsage.Default,
//                 SizeInBytes = 4 * MaxNumLightsPerTile * NumTiles,
//                 BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
//             });
//
//             LightIndexSRV?.Dispose();
//             LightIndexSRV = new ShaderResourceView(GetDevice, LightIndexBuffer, new ShaderResourceViewDescription() {
//                 Format = Format.R32_UInt,
//                 Dimension = ShaderResourceViewDimension.Buffer,
//                 Buffer = new ShaderResourceViewDescription.BufferResource()
//                 {
//                     ElementOffset = 0,
//                     ElementWidth = MaxNumLightsPerTile * NumTiles,
//                 },
//             });
//
//             LightIndexURV?.Dispose();
//             LightIndexURV = new UnorderedAccessView(GetDevice, LightIndexBuffer, new UnorderedAccessViewDescription() {
//                 Format = Format.R32_UInt,
//                 Dimension = UnorderedAccessViewDimension.Buffer,
//                 Buffer = new UnorderedAccessViewDescription.BufferResource()
//                 {
//                     FirstElement = 0,
//                     ElementCount = MaxNumLightsPerTile * NumTiles,
//                 },
//             });
//         }
//
//         private Vector4[] NonDirLightCenterAndRadiusArray = new Vector4[MAX_NUM_LIGHTS];
//         private Vector4[] NonDirLightColorArray = new Vector4[MAX_NUM_LIGHTS];
//         private Vector4[] NonDirLightParamsArray = new Vector4[MAX_NUM_LIGHTS];
//         private void InitLightsBuffers()
//         {
//             BufferDescription LightBufferDesc = new BufferDescription()
//             {
//                 Usage = ResourceUsage.Default,
//                 SizeInBytes = sizeof(float) * 4 * MAX_NUM_LIGHTS,
//                 BindFlags = BindFlags.ShaderResource,
//                 CpuAccessFlags = CpuAccessFlags.Write,
//             };
//
//             LightCenterAndRadiusBuffer = Buffer.Create(GetDevice, NonDirLightCenterAndRadiusArray, LightBufferDesc);
//             LightColorBuffer = Buffer.Create(GetDevice, NonDirLightColorArray, LightBufferDesc);
//             LightParamsBuffer = Buffer.Create(GetDevice, NonDirLightParamsArray, LightBufferDesc);
//
//             m_DirLightsConstBuffer = new CommonStructs.ConstBufferDirLightStruct[] {
//                 new CommonStructs.ConstBufferDirLightStruct(),
//                 new CommonStructs.ConstBufferDirLightStruct(),
//                 new CommonStructs.ConstBufferDirLightStruct()
//             };
//             DirLightBuffer = new Buffer(
//                 GetDevice,
//                 Utilities.SizeOf<CommonStructs.ConstBufferDirLightStruct>() * m_DirLightsConstBuffer.Length,
//                 ResourceUsage.Default,
//                 BindFlags.ConstantBuffer,
//                 CpuAccessFlags.None,
//                 ResourceOptionFlags.None, 0
//             );
//
//             int NumTiles = GetNumTilesX() * GetNumTilesY();
//             int MaxNumLightsPerTile = GetMaxNumLightsPerTile();
//             LightIndexBuffer = new Buffer(GetDevice, new BufferDescription()
//             {
//                 Usage = ResourceUsage.Default,
//                 SizeInBytes = 4 * MaxNumLightsPerTile * NumTiles,
//                 BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
//             });
//
//             LightIndexSRV = new ShaderResourceView(GetDevice, LightIndexBuffer, new ShaderResourceViewDescription()
//             {
//                 Format = Format.R32_UInt,
//                 Dimension = ShaderResourceViewDimension.Buffer,
//                 Buffer = new ShaderResourceViewDescription.BufferResource()
//                 {
//                     ElementOffset = 0,
//                     ElementWidth = MaxNumLightsPerTile * NumTiles,
//                 },
//             });
//             LightIndexURV = new UnorderedAccessView(GetDevice, LightIndexBuffer, new UnorderedAccessViewDescription()
//             {
//                 Format = Format.R32_UInt,
//                 Dimension = UnorderedAccessViewDimension.Buffer,
//                 Buffer = new UnorderedAccessViewDescription.BufferResource()
//                 {
//                     FirstElement = 0,
//                     ElementCount = MaxNumLightsPerTile * NumTiles,
//                 },
//             });
//
//         }
//
//         private uint _dirLightsCount;
//         private uint _nonDirLightsCount;
//
//         private void UpdateLights(IReadOnlyList<LightData> lights)
//         {
//             //TODO: get light from scene information
//             /*
//              * NonDirLightCenterAndRadiusArray: xyz - center, w - radius
//              * NonDirLightColorArray: xy - direction, z-sign - direction part, z-value - cone cosine
//              * NonDirLightParamsArray rgb - color, a - intensity:
//              * 
//              * */
//             NonDirLightCenterAndRadiusArray = new Vector4[MAX_NUM_LIGHTS];
//             NonDirLightColorArray = new Vector4[MAX_NUM_LIGHTS];
//             NonDirLightParamsArray = new Vector4[MAX_NUM_LIGHTS];
//
//             var cosOfCone = (float)System.Math.Cos(MathUtil.DegreesToRadians(30));
//
//             var i = 0;
//             _dirLightsCount = 0;
//             _nonDirLightsCount = 0;
//             foreach (var light in lights)
//             {
//                 if (light.Type == LightType.Directional) {
//                     if (_dirLightsCount < 3)
//                     {
//                         m_DirLightsConstBuffer[_dirLightsCount] = new CommonStructs.ConstBufferDirLightStruct()
//                         {
//                             DirLightColor = new Vector4(light.Color, 1),
//                             DirLightDirection = light.Direction,
//                             DirLightIntensity = light.Intensity,
//                         };
//                         _dirLightsCount++;
//                     }
//                     continue;
//                 }
//                 NonDirLightCenterAndRadiusArray[i] = new Vector4(light.Position, light.Radius);
//                 NonDirLightColorArray[i] = new Vector4(light.Color, light.Intensity);
//                 NonDirLightParamsArray[i] = new Vector4(lights[i].Direction, (lights[i].Type == LightType.Point) ? 0f : 1f);
//                 NonDirLightParamsArray[i].Z = NonDirLightParamsArray[i].Z > 0 ? cosOfCone : -cosOfCone;
//                 i++;
//             }
//             _nonDirLightsCount = (uint)i;
//
//             GetContext.UpdateSubresource(NonDirLightCenterAndRadiusArray, LightCenterAndRadiusBuffer);
//             GetContext.UpdateSubresource(NonDirLightColorArray, LightColorBuffer);
//             GetContext.UpdateSubresource(NonDirLightParamsArray, LightParamsBuffer);
//
//             LightCenterAndRadiusSRV?.Dispose();
//             LightCenterAndRadiusSRV = new ShaderResourceView(GetDevice, LightCenterAndRadiusBuffer, new ShaderResourceViewDescription() {
//                 Format = Format.R32G32B32A32_Float,
//                 Dimension = ShaderResourceViewDimension.Buffer,
//                 Buffer = new ShaderResourceViewDescription.BufferResource()
//                 {
//                     ElementOffset = 0,
//                     ElementWidth = MAX_NUM_LIGHTS,
//                 },
//             });
//
//             LightColorSRV?.Dispose();
//             LightColorSRV = new ShaderResourceView(GetDevice, LightColorBuffer, new ShaderResourceViewDescription() {
//                 //TODO: hdr?
//                 Format = Format.R8G8B8A8_UNorm,
//                 Dimension = ShaderResourceViewDimension.Buffer,
//                 Buffer = new ShaderResourceViewDescription.BufferResource()
//                 {
//                     ElementOffset = 0,
//                     ElementWidth = MAX_NUM_LIGHTS,
//                 },
//             });
//
//             LightParamsSRV?.Dispose();
//             LightParamsSRV = new ShaderResourceView(GetDevice, LightParamsBuffer, new ShaderResourceViewDescription() {
//                 Format = Format.R32G32B32A32_Float,
//                 Dimension = ShaderResourceViewDimension.Buffer,
//                 Buffer = new ShaderResourceViewDescription.BufferResource()
//                 {
//                     ElementOffset = 0,
//                     ElementWidth = MAX_NUM_LIGHTS,
//                 },
//             });
//         }
//
//         public override void Dispose()
//         {
//             PerObjConstantBuffer?.Dispose();
//             PerObjConstantBuffer = null;
//             PerFrameConstantBuffer?.Dispose();
//             PerFrameConstantBuffer = null;
//             LightIndexBuffer?.Dispose();
//             LightIndexBuffer = null;
//             LightCenterAndRadiusBuffer?.Dispose();
//             LightCenterAndRadiusBuffer = null;
//             LightColorBuffer?.Dispose();
//             LightColorBuffer = null;
//             LightParamsBuffer?.Dispose();
//             LightParamsBuffer = null;
//             DirLightBuffer?.Dispose();
//             DirLightBuffer = null;
//
//             LightCenterAndRadiusSRV?.Dispose();
//             LightCenterAndRadiusSRV = null;
//             LightColorSRV?.Dispose();
//             LightColorSRV = null;
//             LightParamsSRV?.Dispose();
//             LightParamsSRV = null;
//             LightIndexSRV?.Dispose();
//             LightIndexSRV = null;
//             LightIndexURV?.Dispose();
//             LightIndexURV = null;
//         }
//     }
// }
