using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore.RenderTechnique
{
    public class GBuffer
    {
        //public int TargetsCount = 5;
        ////* SCHEMA:                     *//
        ////* 0 Albedo                    *//
        ////* 1 Positions                 *//
        ////* 2 Normals                   *//
        ////* 3 RoughnessMetallicDepth *//
        ////* 4 Occlusion Unlit NonShadows*//

        //public Texture2D[] texturesTargets;
        //public RenderTargetView[] renderTargetViews;
        //public ShaderResourceView[] shaderResourceViews;

        //public GBuffer() {
        //    texturesTargets = new Texture2D[TargetsCount];
        //    renderTargetViews = new RenderTargetView[TargetsCount];
        //    shaderResourceViews = new ShaderResourceView[TargetsCount];

        //    Texture2DDescription textureDescription = new Texture2DDescription() {
        //        Width = Engine.Instance.DisplayRef.Width,
        //        Height = Engine.Instance.DisplayRef.Height,
        //        MipLevels = 1,
        //        ArraySize = 1,
        //        Format = Format.R32G32B32A32_Float,
        //        SampleDescription = new SampleDescription() {
        //            Count = 1,
        //        },
        //        Usage = ResourceUsage.Default,
        //        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        //    };

        //    RenderTargetViewDescription renderTargetDescription = new RenderTargetViewDescription() {
        //        Format = Format.R32G32B32A32_Float,
        //        Dimension = RenderTargetViewDimension.Texture2D,
        //    };
        //    //InteropImage
        //    //renderTargetDescription.Texture2D.MipSlice = 0;

        //    ShaderResourceViewDescription shaderResourceDescription = new ShaderResourceViewDescription() {
        //        Format = Format.R32G32B32A32_Float,
        //        Dimension = ShaderResourceViewDimension.Texture2D,
        //    };

        //    shaderResourceDescription.Texture2D.MostDetailedMip = 0;
        //    shaderResourceDescription.Texture2D.MipLevels = 1;

        //    for (int i = 0; i < TargetsCount; i++) {
        //        texturesTargets[i] = new Texture2D(Engine.Instance.Device, textureDescription);
        //        renderTargetViews[i] = new RenderTargetView(Engine.Instance.Device, texturesTargets[i], renderTargetDescription);
        //        shaderResourceViews[i] = new ShaderResourceView(Engine.Instance.Device, texturesTargets[i], shaderResourceDescription);
        //    }
        //}

        //public void Dispose() {
        //    for (int i = 0; i < TargetsCount; i++) {
        //        texturesTargets[i].Dispose();
        //        renderTargetViews[i].Dispose();
        //        shaderResourceViews[i].Dispose();
        //    }
        //}
    }
}
