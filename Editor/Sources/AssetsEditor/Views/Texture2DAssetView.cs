using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class Texture2DAssetView : BaseAssetView
    {
        private GameObject PreviewGO;
        private Material TestMaterial;
        private float ratio = 1f;

        public override void Init(PreviewEngine engine) {
            base.Init(engine);
            TestMaterial = new Material() {
                Name = "M_TextureTest",
                AlbedoMapAsset = "",
                PropetyBlock = new MaterialPropetyBlock() {
                    MetallicValue = 0.5f,
                    RoughnessValue = 0.75f,
                    Tile = Vector2.One,
                },
            };
            TestMaterial.LoadMapsAndInitSampler();
            PreviewGO = EngineRef.AddGameObject("TestQuad");
            PreviewGO.transform.WorldPosition = Vector3.Zero;
            PreviewGO.transform.WorldRotation = Quaternion.Identity;
            PreviewGO.transform.WorldScale = (Vector3.Right + Vector3.Up) * 3.85f + Vector3.ForwardLH * 0.1f;
            PreviewGO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            PreviewGO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.PlaneWithUV, TestMaterial);
            PreviewGO.SelfActive = false;
        }

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(false, false, false, false, false);
            EngineRef.MainCamera.gameObject.transform.WorldPosition = new Vector3(0f, 0, 2f);
            EngineRef.MainCamera.gameObject.transform.WorldRotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
            PreviewGO.SelfActive = true;

            Texture2DAsset asset = AssetsManagerInstance.GetManager().LoadAsset<Texture2DAsset>(assetName);
            ratio = (float)asset.Data.Width / asset.Data.Height;
            Vector3 scale;
            if (ratio >= 1) {
                scale = (Vector3.Right + Vector3.Up / ratio) * 3.85f;
            } else {
                scale = (Vector3.Right + Vector3.Up * ratio) * 3.85f;
            }
            scale += Vector3.ForwardLH * 0.1f;
            PreviewGO.transform.WorldScale = scale;

            TestMaterial.AlbedoMapAsset = assetName;
            TestMaterial.LoadMapsAndInitSampler();
        }

        public override void Hide() {
            base.Hide();
            PreviewGO.SelfActive = false;
        }

    }
}
