using System;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal abstract partial class BaseD3D11RenderPath
    {
        protected void SetMergerStates(MaterialMeta meta)
        {
            switch (meta.BlendMode)
            {
                case MaterialMeta.BlendModeType.Opaque:
                    SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
                    SetBlendState(BlendStateType.Opaque);
                    break;
                case MaterialMeta.BlendModeType.Masked:
                    SetDepthStencilState(DepthStencilStateType.EqualAndDisableWrite);
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
