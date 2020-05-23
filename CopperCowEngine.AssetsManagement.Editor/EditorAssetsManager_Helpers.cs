using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor.FileSystemWorker;
using CopperCowEngine.Rendering;

namespace CopperCowEngine.AssetsManagement.Editor
{
    public partial class EditorAssetsManager
    {
        public bool CreateMaterialAsset()
        {
            return EditorFileSystemWorker.CreateAssetFile(new MaterialAsset()
            {
                Name = "NewMaterial",
            });
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset)
        {
            BaseAsset asset = new MaterialAsset()
            {
                Name = name,
                AlbedoMapAsset = albedoAsset,
                NormalMapAsset = normalAsset,
                MetallicValue = 0.05f,
                RoughnessValue = 0.95f,
            };
            return EditorFileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset, string roughnessAsset, string metallicAsset, string occlusionAsset = "")
        {
            BaseAsset asset = new MaterialAsset()
            {
                Name = name,
                AlbedoMapAsset = albedoAsset,
                NormalMapAsset = normalAsset,
                RoughnessMapAsset = roughnessAsset,
                MetallicMapAsset = metallicAsset,
                OcclusionMapAsset = occlusionAsset,
            };
            return EditorFileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMeshAsset(string path, string name, float fileScale = 1f)
        {
            var ext = path.Split('.').Last().ToLower();
            var asset = new MeshAsset()
            {
                Name = name,
            };
            asset.ImportAsset(path, ext, fileScale);
            return EditorFileSystemWorker.CreateAssetFile(asset, true);
        }
    }
}
