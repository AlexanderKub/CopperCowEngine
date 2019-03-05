using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor.Components;
using EngineCore;
using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Editor.AssetsEditor.Views
{
    internal class MeshAssetView: BaseAssetView
    {
        private GameObject PreviewGO;
        private Renderer PreviewRenderer;
        private GameObject ViewGO;

        public override void Init(PreviewEngine engine) {
            base.Init(engine);

            PreviewGO = EngineRef.AddGameObject("TestMesh");
            PreviewGO.transform.WorldPosition = Vector3.Zero;
            PreviewGO.transform.WorldRotation = Quaternion.Identity;
            PreviewGO.transform.WorldScale = Vector3.One * 0.002f;

            PreviewRenderer = PreviewGO.GetComponent<Renderer>();
            PreviewRenderer.SetMeshAndMaterial(Primitives.Sphere(30), AssetsLoader.LoadMaterial("CopperMaterial"));
            PreviewGO.AddComponent(new PreviewBehaviour());
            PreviewGO.SelfActive = false;

            ViewGO = EngineRef.AddGameObject("MeshView", true);
            ViewGO.transform.WorldPosition = Vector3.Zero;
            ViewGO.transform.WorldRotation = Quaternion.Identity;
            ViewGO.transform.WorldScale = Vector3.One;
            ViewGO.AddComponent(new FollowPreviewBehaviour(PreviewGO.transform));

            Primitives.CeilSizeX = 12;
            Primitives.CeilSizeY = 12;
            GameObject Ceil = EngineRef.AddGameObject("Ceil");
            Ceil.transform.LocalRotation = Quaternion.Identity;
            Ceil.transform.LocalScale = Vector3.One * 6.6666f * 0.075f;
            Ceil.transform.LocalPosition = Vector3.Zero;
            Ceil.transform.Parent = ViewGO.transform;
            Ceil.GetComponent<Renderer>().Topology = PrimitiveTopology.LineList;
            Ceil.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            Ceil.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                AlbedoColor = Vector3.One * 0.2f,
            };
            Ceil.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Ceil, Material.DefaultMaterial);

            GameObject GO = EngineRef.AddGameObject("MeshView");
            GO.transform.LocalPosition = Vector3.Zero;
            GO.transform.LocalRotation = Quaternion.Identity;
            GO.transform.LocalScale = Vector3.One * 0.075f;
            GO.transform.Parent = ViewGO.transform;
            GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                AlbedoColor = Vector3.One,
            };
            GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Sphere(16), Material.DefaultMaterial);

            string[] axis = new string[] { "OX", "OY", "OZ" };
            Vector3[] Offsets = new Vector3[] { Vector3.Right, Vector3.Up, Vector3.ForwardLH };
            for (int i = 0; i < 3; i++) {
                GO = EngineRef.AddGameObject("MeshView" + axis);
                GO.transform.LocalPosition = Offsets[i] * 2f * 0.075f;
                GO.transform.LocalRotation = Quaternion.Identity;
                GO.transform.LocalScale = new Vector3(i == 0 ? 4f : 0.4f, i == 1 ? 4f : 0.4f, i == 2 ? 4f : 0.4f) * 0.075f;
                GO.transform.Parent = ViewGO.transform;
                GO.GetComponent<Renderer>().CustomPropertyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = Offsets[i],
                };
                GO.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.Unlit;
                GO.GetComponent<Renderer>().SetMeshAndMaterial(Primitives.Cube(), Material.DefaultMaterial);
            }
            ViewGO.SelfActive = false;
        }

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, false);
            PreviewGO.GetComponent<PreviewBehaviour>().Reset();
            EngineRef.MainCamera.gameObject.transform.WorldPosition = new Vector3(0f, 1.5f, 2.5f);
            EngineRef.MainCamera.gameObject.transform.WorldRotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, MathUtil.Pi * 0.15f, 0);

            PreviewGO.transform.WorldPosition = Vector3.Zero;
            PreviewGO.GetComponent<Renderer>().UpdateMesh(AssetsLoader.LoadMesh(assetName));

            PreviewGO.SelfActive = true;
            ViewGO.SelfActive = true;
        }

        public override void Hide() {
            base.Hide();
            PreviewGO.SelfActive = false;
            ViewGO.SelfActive = false;
            // ChangePivotAndFileScale(MeshAssetRef.Pivot, MeshAssetRef.FileScale);
        }

        /*public override void SaveChanging() {
            MeshAssetRef.Pivot = PreviewRenderer.Geometry.Pivot;
            MeshAssetRef.FileScale = PreviewRenderer.Geometry.FileScale;
            AssetsManagerInstance.GetManager().SaveAssetChanging(MeshAssetRef);
        }*/

        public void ChangePivotAndFileScale(Vector3 pivot, float scale) {
            PreviewRenderer.Geometry.Pivot = pivot;
            PreviewRenderer.Geometry.FileScale = scale;
            PreviewRenderer.UpdateMesh();
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
