using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopperCowEngine.AssetsManagement.AssetsMeta;

namespace CopperCowEngine.AssetsManagement
{
    public partial class AssetsManager
    {
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
            return FileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset, string roughnessAsset, string metallicAsset)
        {
            return CreateMaterialAsset(name, albedoAsset, normalAsset, roughnessAsset, metallicAsset, "");
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset, string roughnessAsset, string metallicAsset, string occlusionAsset)
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
            return FileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMeshAsset(string path, string name, float fileScale)
        {
            var ext = path.Split('.').Last().ToLower();
            var asset = new MeshAsset()
            {
                Name = name,
            };
            asset.ImportAsset(path, ext, fileScale);
            return FileSystemWorker.CreateAssetFile(asset, true);
        }
    }
}
