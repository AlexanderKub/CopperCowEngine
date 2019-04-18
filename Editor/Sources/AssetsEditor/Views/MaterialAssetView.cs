using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using EngineCore.ECS;
using EngineCore.ECS.Components;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class MaterialAssetView : BaseAssetView
    {
        private Entity PreviewEntity;
        private Renderer RendererComponent;
        private PreviewBehaviourComponent previewBehaviour;

        private AssetsLoader.MeshInfo SphereMeshInfo;
        private AssetsLoader.MeshInfo CubeMeshInfo;

        public override void Init(PreviewEngine engine)
        {
            base.Init(engine);
            SphereMeshInfo = AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Sphere);
            CubeMeshInfo = AssetsLoader.LoadMeshInfo(AssetsLoader.PrimitivesMesh.Cube);

            PreviewEntity = EngineRef.ECSWorld.CreateEntityWith<Transform, Renderer>("TestMaterialEntity");
            PreviewEntity.GetComponent<Transform>().SetTransformations(Vector3.Zero, Quaternion.Identity, Vector3.One);
            RendererComponent = PreviewEntity.GetComponent<Renderer>();
            RendererComponent.SetMeshAndMaterial(SphereMeshInfo, 
                AssetsLoader.LoadMaterialInfo(Material.DefaultMaterial));

            previewBehaviour = PreviewEntity.AddComponent<PreviewBehaviourComponent>();
            EngineRef.ECSWorld.Refresh();
        }

        public override void Show(string assetName)
        {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, true);

            EngineRef.CameraEntity.GetComponent<Transform>().SetTransformations(new Vector3(0f, 0, -1.5f),
                Quaternion.RotationYawPitchRoll(MathUtil.Pi, 0, 0));
            RendererComponent.SetMaterial(AssetsLoader.LoadMaterialInfo(assetName));

            PreviewEntity.SetActive(true);
            EngineRef.ECSWorld.Refresh();
        }

        static private string NoneTexture = "--NONE--";
        private string TrimEmptyTexture(string map)
        {
            return map == NoneTexture ? "" : map;
        }

        public override void Update(BaseAsset asset)
        {
            MaterialAsset materialAsset = (asset as MaterialAsset);
            var matInfo = AssetsLoader.LoadMaterialInfo(asset.Name);
            var mat = AssetsLoader.LoadMaterial(asset.Name);

            mat.PropetyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = materialAsset.AlbedoColor,
                AlphaValue = materialAsset.AlphaValue,
                MetallicValue = materialAsset.MetallicValue,
                RoughnessValue = materialAsset.RoughnessValue,
                Shift = materialAsset.Shift,
                Tile = materialAsset.Tile,
            };
            mat.AlbedoMapAsset = TrimEmptyTexture(materialAsset.AlbedoMapAsset);
            mat.MetallicMapAsset = TrimEmptyTexture(materialAsset.MetallicMapAsset);
            mat.NormalMapAsset = TrimEmptyTexture(materialAsset.NormalMapAsset);
            mat.OcclusionMapAsset = TrimEmptyTexture(materialAsset.OcclusionMapAsset);
            mat.RoughnessMapAsset = TrimEmptyTexture(materialAsset.RoughnessMapAsset);

            RendererComponent.SetMaterial(matInfo);
        }

        public override void Hide()
        {
            base.Hide();
            PreviewEntity.SetActive(false);
            EngineRef.ECSWorld.Refresh();
        }

        public override void ChangePosView(int v)
        {
            switch (v) {
                case 0:
                    RendererComponent.SetMesh(SphereMeshInfo);
                    break;
                case 1:
                    RendererComponent.SetMesh(CubeMeshInfo);
                    break;
                default:
                    break;
            }
        }

        public override void ChangeZoom(float value)
        {
            previewBehaviour.ScaleOffset = value;
        }

        public override void ChangeYaw(float value)
        {
            previewBehaviour.Yaw = MathUtil.Pi * 2f - value;
        }

        public override void ChangePitch(float value)
        {
            previewBehaviour.Pitch = MathUtil.Pi * 2f - value;
        }
    }
}
