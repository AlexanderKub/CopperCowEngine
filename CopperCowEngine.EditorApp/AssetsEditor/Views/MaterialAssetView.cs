using System;
using System.Numerics;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Extensions;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.EditorApp.AssetsEditor.Views
{
    internal class MaterialAssetView : BaseAssetView
    {
        private const string NoneTexture = "--NONE--";
        
        private Entity _previewEntity;
        private MeshInfo _sphereMeshInfo;
        private MeshInfo _cubeMeshInfo;

        private float _yaw;
        private float _pitch;

        public override void Init(PreviewEngine engine)
        {
            base.Init(engine);
            _sphereMeshInfo = MeshAssetsLoader.GetMeshInfo(PrimitivesMesh.Sphere);
            _cubeMeshInfo = MeshAssetsLoader.GetMeshInfo(PrimitivesMesh.Cube);

            var copperMaterial = MaterialLoader.LoadMaterial("CopperMaterial");
            var materialInfo = MaterialLoader.GetMaterialInfo(copperMaterial);

            _previewEntity = EngineRef.EcsContext.CreateRenderedEntity(_sphereMeshInfo, materialInfo, new Vector3(0, 0, 1.5f), 
                Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0));
        }

        public override void Show(string assetName)
        {
            base.Show(assetName);
            EngineRef.SetViewsControlsEnabled(true, true, true, false, true);
            
            EngineRef.EcsContext.GetComponent<Scale>(_previewEntity).Value = 1f;
            EngineRef.EcsContext.GetComponent<Rotation>(_previewEntity).Value = 
                Quaternion.CreateFromYawPitchRoll(MathF.PI, 0, 0);
            
            var materialGuid = MaterialLoader.LoadMaterial(assetName);
            var materialInfo = MaterialLoader.GetMaterialInfo(materialGuid);
            EngineRef.EcsContext.GetComponent<Material>(_previewEntity) = materialInfo.CreateMaterial();
        }

        private static string TrimEmptyTexture(string map)
        {
            return map == NoneTexture ? "" : map;
        }

        public override void Update(BaseAsset asset)
        {
            if (!(asset is MaterialAsset materialAsset))
            {
                return;
            }
            
            var matGuid = MaterialLoader.LoadMaterial(asset.Name);
            var matInfo = MaterialLoader.GetMaterialInfo(matGuid);
            var mat = MaterialLoader.GetMaterialInstance(matGuid);

            mat.PropertyBlock = new MaterialPropertyBlock
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

            EngineRef.EcsContext.GetComponent<Material>(_previewEntity) = matInfo.CreateMaterial();
        }

        public override void Hide()
        {
            base.Hide();
            //EngineRef.EcsContext.RemoveComponent<Mesh>(_previewEntity);
        }

        public override void ChangePosView(int v)
        {
            switch (v) {
                case 0:
                    EngineRef.EcsContext.GetComponent<Mesh>(_previewEntity) = _sphereMeshInfo.CreateMesh();
                    break;
                case 1:
                    EngineRef.EcsContext.GetComponent<Mesh>(_previewEntity) = _cubeMeshInfo.CreateMesh();
                    break;
                default:
                    break;
            }
        }

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
