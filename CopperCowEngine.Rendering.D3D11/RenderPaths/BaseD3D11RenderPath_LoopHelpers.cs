using System;
using System.Runtime.InteropServices;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal abstract partial class BaseD3D11RenderPath
    {
        
        protected SharedRenderItemsStorage.CachedMesh CachedMesh;
        protected MaterialInstance CachedMaterial;

        private Guid _cachedMeshGuid;
        private Guid _cachedMaterialGuid;

        private int? _vertexBufferStride;
        private int VertexBufferStride => _vertexBufferStride ??= Marshal.SizeOf<VertexStruct>();

        protected void ClearMesh()
        {
            _cachedMeshGuid = Guid.Empty;
            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, VertexBufferStride, 0));
            GetContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
        }

        protected void SetMesh(Guid meshGuid, PrimitiveTopology topology = PrimitiveTopology.TriangleList)
        {
            SetPrimitiveTopology(topology);

            if (_cachedMeshGuid == meshGuid)
            {
                return;
            }

            _cachedMeshGuid = meshGuid;
            CachedMesh = GetSharedItems.GetMesh(meshGuid);
            GetContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(CachedMesh.VertexBuffer, VertexBufferStride, 0));
            GetContext.InputAssembler.SetIndexBuffer(CachedMesh.IndexBuffer, Format.R32_UInt, 0);
        }
        
        protected void ClearMaterial()
        {
            _cachedMaterialGuid = Guid.Empty;
        }

        protected void SetMaterial(Guid assetGuid)
        {
            if (_cachedMaterialGuid == assetGuid)
            {
                return;
            }

            _cachedMaterialGuid = assetGuid;

            var oldQueue = CachedMaterial?.ShaderQueue / 100;
            
            CachedMaterial = MaterialInstance.IsSkySphereMaterial(assetGuid) 
                ? MaterialInstance.GetSkySphereMaterial() 
                : MaterialLoader.GetMaterialInstance(assetGuid);
            OnMaterialChanged();
        }

        protected abstract void OnMaterialChanged();

        protected void SetMergerStates(MaterialMeta meta)
        {
            switch (meta.BlendMode)
            {
                case MaterialMeta.BlendModeType.Opaque:
                    SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
                    SetBlendState(BlendStateType.Opaque);
                    break;
                case MaterialMeta.BlendModeType.Masked:
                    SetDepthStencilState(DepthStencilStateType.GreaterEqualAndDisableWrite);
                    SetBlendState(BlendStateType.AlphaEnabledBlending);
                    break;
                case MaterialMeta.BlendModeType.Translucent:
                    SetDepthStencilState(DepthStencilStateType.GreaterAndDisableWrite);
                    SetBlendState(BlendStateType.AlphaEnabledBlending);
                    break;
                case MaterialMeta.BlendModeType.Additive:
                    break;
                case MaterialMeta.BlendModeType.Modulate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (meta.CullMode)
            {
                case MaterialMeta.CullModeType.Front:
                    SetRasterizerState(meta.Wireframe ? RasterizerStateType.WireframeFrontCull
                        : RasterizerStateType.SolidFrontCull);
                    break;
                case MaterialMeta.CullModeType.Back:
                    SetRasterizerState(meta.Wireframe ? RasterizerStateType.WireframeBackCull
                        : RasterizerStateType.SolidBackCull);
                    break;
                case MaterialMeta.CullModeType.None:
                    SetRasterizerState(meta.Wireframe ? RasterizerStateType.WireframeNoneCull
                        : RasterizerStateType.SolidNoneCull);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
