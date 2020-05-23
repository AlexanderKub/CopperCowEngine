using System;
using System.Numerics;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Extensions;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;

namespace CopperCowEngine.EditorApp.AssetsEditor.Views
{
    internal class MeshAssetView: BaseAssetView
    {
        private Entity _previewEntity;

        private MaterialInfo _wireframeMaterialInfo;

        private float _yaw;
        private float _pitch;

        public override void Init(PreviewEngine engine)
        {
            base.Init(engine);
            var sphereMeshInfo = MeshAssetsLoader.GetMeshInfo(PrimitivesMesh.Sphere);

            var meta = new MaterialMeta
            {
                Wireframe = true,
                BlendMode = MaterialMeta.BlendModeType.Opaque,
                CullMode = MaterialMeta.CullModeType.None,
                MaterialDomain = MaterialMeta.MaterialDomainType.Surface,
                ShadingMode = MaterialMeta.ShadingModeType.Unlit,
                OpacityMaskClipValue = 0,
            };

            var material = new MaterialInstance(meta)
            {
                Name = "WireframeMaterial",
                PropertyBlock = MaterialPropertyBlock.Default,
            };
            material.PropertyBlock.AlbedoColor = Vector3.UnitY;
            _wireframeMaterialInfo = MaterialLoader.GetMaterialInfo(material);

            var materialInfo = MaterialLoader.GetMaterialInfo(MaterialInstance.DefaultMaterial);
            _previewEntity = EngineRef.EcsContext.CreateRenderedEntity(sphereMeshInfo, _wireframeMaterialInfo, new Vector3(0, 0, 1.5f), 
                Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0));
        }

        //public override void Init(PreviewEngine engine) {
        //    base.Init(engine);

        //    PreviewGO = EngineRef.AddGameObject("TestMesh");
        //    PreviewGO.transform.Scale = Vector3.One * 0.002f;

        //    PreviewRenderer = PreviewGO.GetComponent<DeprecatedRenderer>();
        //    PreviewRenderer.SetMeshAndMaterial(Primitives.Sphere(30), AssetsLoader.LoadMaterial("CopperMaterial"));
        //    PreviewGO.AddComponent(new PreviewBehaviour());
        //    PreviewGO.SelfActive = false;
        //    InitBounds();
        //    InitCeilAndAxis();
        //}

        private void InitCeilAndAxis()
        {
           /* ViewGO = EngineRef.AddGameObject("MeshView", true);
            ViewGO.AddComponent(new FollowPreviewBehaviour(PreviewGO.transform));

            Primitives.CeilSizeX = 12;
            Primitives.CeilSizeY = 12;
            GameObject Ceil = EngineRef.AddGameObject("Ceil");
            Ceil.transform.Parent = ViewGO.transform;
            Ceil.transform.RelativeScale = Vector3.One * 6.6666f * 0.075f;
            Ceil.GetComponent<DeprecatedRenderer>().Topology = PrimitiveTopology.LineList;
            Ceil.GetComponent<DeprecatedRenderer>().SpecificType = DeprecatedRenderer.SpecificTypeEnum.Unlit;
            Ceil.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = Vector3.One * 0.2f,
            };
            Ceil.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Ceil, Material.DefaultMaterial);

            GameObject GO = EngineRef.AddGameObject("MeshView");
            GO.transform.Parent = ViewGO.transform;
            GO.transform.RelativeScale = Vector3.One * 0.075f;
            GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = Vector3.One,
            };
            GO.GetComponent<DeprecatedRenderer>().SpecificType = DeprecatedRenderer.SpecificTypeEnum.Unlit;
            GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Sphere(16), Material.DefaultMaterial);

            string[] axis = new string[] { "OX", "OY", "OZ" };
            Vector3[] Offsets = new Vector3[] { Vector3.Right, Vector3.Up, Vector3.ForwardLH };
            for (int i = 0; i < 3; i++)
            {
                GO = EngineRef.AddGameObject("MeshView" + axis);
                GO.transform.Parent = ViewGO.transform;
                GO.transform.RelativeScale = new Vector3(i == 0 ? 4f : 0.4f, i == 1 ? 4f : 0.4f, i == 2 ? 4f : 0.4f) * 0.075f;
                GO.transform.RelativePosition = Offsets[i] / GO.transform.Parent.Scale * 0.4f;
                GO.GetComponent<DeprecatedRenderer>().CustomPropertyBlock = new MaterialPropetyBlock()
                {
                    AlbedoColor = Offsets[i],
                };
                GO.GetComponent<DeprecatedRenderer>().SpecificType = DeprecatedRenderer.SpecificTypeEnum.Unlit;
                GO.GetComponent<DeprecatedRenderer>().SetMeshAndMaterial(Primitives.Cube(), Material.DefaultMaterial);
            }
            ViewGO.SelfActive = false;*/
        }

        private void InitBounds()
        {
            /*PreviewBounds = EngineRef.AddGameObject("TestMeshBounds");
            PreviewBounds.transform.Parent = PreviewGO.transform;
            PreviewBounds.transform.RelativeRotation = Quaternion.Identity;
            PreviewBoundsRenderer = PreviewBounds.GetComponent<DeprecatedRenderer>();
            PreviewBoundsRenderer.SpecificType = DeprecatedRenderer.SpecificTypeEnum.Wireframe;
            PreviewBoundsRenderer.SetMeshAndMaterial(Primitives.Cube(), Material.DefaultMaterial);
            PreviewBoundsRenderer.CustomPropertyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = Vector3.Up,
            };
            PreviewBounds.SelfActive = false;*/
        }

        //private void UpdateBounds()
        //{
        //    PreviewBounds.transform.RelativeScale = PreviewRenderer.MeshBounds.Size + Vector3.One * 0.002f;
        //    PreviewBounds.transform.RelativePosition = PreviewRenderer.MeshBounds.Center / PreviewBounds.transform.Scale;
        //}

        public override void Show(string assetName) {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, false);

            var meshGuid = MeshAssetsLoader.LoadMesh(assetName);
            EngineRef.EcsContext.GetComponent<Mesh>(_previewEntity) =
                MeshAssetsLoader.GetMeshInfo(meshGuid).CreateMesh();

            /* PreviewGO.GetComponent<PreviewBehaviour>().Reset();
             EngineRef.MainCamera.transform.Position = new Vector3(0f, 1.5f, 2.5f);
             EngineRef.MainCamera.transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.Pi, MathUtil.Pi * 0.15f, 0);
 
             PreviewGO.GetComponent<DeprecatedRenderer>().UpdateMesh(AssetsLoader.LoadMesh(assetName));
             UpdateBounds();
             PreviewGO.transform.Position = -PreviewRenderer.MeshBounds.Center + PreviewRenderer.MeshBounds.Extent.Y * Vector3.Up;
             PreviewGO.transform.RotateAroundPivot = PreviewRenderer.MeshBounds.Center - PreviewRenderer.MeshBounds.Extent.Y * Vector3.Up;
 
             PreviewGO.SelfActive = true;
             PreviewBounds.SelfActive = true;
             ViewGO.SelfActive = true;*/
        }

        //public override void Hide() {
        //    base.Hide();
        //    PreviewGO.SelfActive = false;
        //    PreviewBounds.SelfActive = false;
        //    ViewGO.SelfActive = false;
        //    // ChangePivotAndFileScale(MeshAssetRef.Pivot, MeshAssetRef.FileScale);
        //}

        ///*public override void SaveChanging() {
        //    MeshAssetRef.Pivot = PreviewRenderer.Geometry.Pivot;
        //    MeshAssetRef.FileScale = PreviewRenderer.Geometry.FileScale;
        //    AssetsManagerInstance.GetManager().SaveAssetChanging(MeshAssetRef);
        //}*/

        //public void ChangePivotAndFileScale(Vector3 pivot, float scale) {
        //    PreviewRenderer.Geometry.Pivot = pivot;
        //    PreviewRenderer.Geometry.FileScale = scale;
        //    PreviewRenderer.UpdateMesh();
        //    UpdateBounds();
        //}

        public override void ChangeZoom(float value) 
        {
            EngineRef.EcsContext.GetComponent<Scale>(_previewEntity).Value = value;
        }

        public override void ChangeYaw(float value) 
        {
            _yaw = MathF.PI * 2f - value;
            EngineRef.EcsContext.GetComponent<Rotation>(_previewEntity).Value = 
                Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0);
        }

        public override void ChangePitch(float value) 
        {
            _pitch = MathF.PI * 2f - value;
            EngineRef.EcsContext.GetComponent<Rotation>(_previewEntity).Value = 
                Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0);
        }
    }
}
