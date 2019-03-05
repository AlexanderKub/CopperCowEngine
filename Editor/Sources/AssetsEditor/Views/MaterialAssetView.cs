using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class MaterialAssetView : BaseAssetView
    {
        private GameObject PreviewGO;
        private Renderer PreviewRenderer;

        private AssetsManager.Loaders.ModelGeometry SphereMesh;
        private AssetsManager.Loaders.ModelGeometry CubeMesh;

        public override void Init(PreviewEngine engine) {
            base.Init(engine);
            SphereMesh = Primitives.Sphere(46);
            //CubeMesh = AssetsLoader.LoadMesh("Cube1mMesh");
            CubeMesh = Primitives.Cube();

            PreviewGO = EngineRef.AddGameObject("TestMaterialMesh");
            PreviewGO.transform.WorldPosition = Vector3.Zero;
            PreviewGO.transform.WorldRotation = Quaternion.Identity;
            PreviewGO.transform.WorldScale = Vector3.One;
            PreviewRenderer = PreviewGO.GetComponent<Renderer>();
            PreviewRenderer.SetMeshAndMaterial(SphereMesh , Material.DefaultMaterial);
            PreviewGO.AddComponent(new PreviewBehaviour());
            PreviewGO.SelfActive = false;
        }

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, true);
            PreviewGO.GetComponent<PreviewBehaviour>().Reset();
            EngineRef.MainCamera.gameObject.transform.WorldPosition = new Vector3(0f, 0, 1.5f);
            EngineRef.MainCamera.gameObject.transform.WorldRotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0);
            PreviewGO.SelfActive = true;

            PreviewRenderer.RendererMaterial = AssetsLoader.LoadMaterial(assetName);
        }

        static private string NoneTexture = "--NONE--";
        private string TrimEmptyTexture(string map) {
            return map == NoneTexture ? "" : map;
        }

        public override void Update(BaseAsset asset) {
            MaterialAsset materialAsset = (asset as MaterialAsset);
            PreviewRenderer.RendererMaterial.PropetyBlock.AlbedoColor = materialAsset.AlbedoColor;
            PreviewRenderer.RendererMaterial.PropetyBlock.MetallicValue = materialAsset.MetallicValue;
            PreviewRenderer.RendererMaterial.PropetyBlock.RoughnessValue = materialAsset.RoughnessValue;
            PreviewRenderer.RendererMaterial.PropetyBlock.Tile = materialAsset.Tile;
            PreviewRenderer.RendererMaterial.PropetyBlock.Shift = materialAsset.Shift;

            PreviewRenderer.RendererMaterial.AlbedoMapAsset = TrimEmptyTexture(materialAsset.AlbedoMapAsset);
            PreviewRenderer.RendererMaterial.MetallicMapAsset = TrimEmptyTexture(materialAsset.MetallicMapAsset);
            PreviewRenderer.RendererMaterial.NormalMapAsset = TrimEmptyTexture(materialAsset.NormalMapAsset);
            PreviewRenderer.RendererMaterial.OcclusionMapAsset = TrimEmptyTexture(materialAsset.OcclusionMapAsset);
            PreviewRenderer.RendererMaterial.RoughnessMapAsset = TrimEmptyTexture(materialAsset.RoughnessMapAsset);
            PreviewRenderer.RendererMaterial.LoadMapsAndInitSampler();
        }

        public override void Hide() {
            base.Hide();
            PreviewGO.SelfActive = false;
        }

        public override void ChangePosView(int v) {
            switch (v) {
                case 0:
                    PreviewRenderer.UpdateMesh(SphereMesh);
                    break;
                case 1:
                    PreviewRenderer.UpdateMesh(CubeMesh);
                    break;
                default:
                    break;
            }
        }
        public override void ChangeZoom(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().ScaleOffset = value;
        }
        public override void ChangeYaw(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Yaw = MathUtil.Pi * 2f - value;
        }
        public override void ChangePitch(float value) {
            PreviewGO.GetComponent<PreviewBehaviour>().Pitch = MathUtil.Pi * 2f - value;
        }
    }
}
