using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;
using CopperCowEngine.Rendering.ShaderGraph;

namespace CopperCowEngine.AssetsManagement.Loaders
{
    public static class MaterialLoader
    {
        private static readonly Dictionary<Guid, MaterialInstance> CachedMaterials = new Dictionary<Guid, MaterialInstance>();
        private static readonly Dictionary<string, Guid> CachedMaterialsGuidTable = new Dictionary<string, Guid>();

        public static Guid LoadMaterial(string assetName)
        {
            if (CachedMaterialsGuidTable.ContainsKey(assetName))
            {
                return CachedMaterialsGuidTable[assetName];
            }

            var materialAsset = AssetsManager.GetManager().LoadAsset<MaterialAsset>(assetName);
            var mat = new MaterialInstance()
            {
                Name = materialAsset.Name,
                AlbedoMapAsset = materialAsset.AlbedoMapAsset,
                EmissiveMapAsset = materialAsset.EmissiveMapAsset,
                RoughnessMapAsset = materialAsset.RoughnessMapAsset,
                MetallicMapAsset = materialAsset.MetallicMapAsset,
                NormalMapAsset = materialAsset.NormalMapAsset,
                OcclusionMapAsset = materialAsset.OcclusionMapAsset,
                PropertyBlock = new MaterialPropertyBlock()
                {
                    AlbedoColor = materialAsset.AlbedoColor,
                    AlphaValue = materialAsset.AlphaValue,
                    EmissiveColor = materialAsset.EmissiveColor,
                    MetallicValue = materialAsset.MetallicValue,
                    RoughnessValue = materialAsset.RoughnessValue,
                    Shift = materialAsset.Shift,
                    Tile = materialAsset.Tile,
                },
                TexturesSampler = MaterialInstance.SamplerType.BilinearWrap,
            };
            
            var newGuid = Guid.NewGuid();
            CachedMaterialsGuidTable.Add(assetName, newGuid);
            CachedMaterials.Add(newGuid, mat);
            //Debug.Log("AssetManager", "Material " + assetName + " loaded.");

            return newGuid;
        }

        public static Guid GetGuid(string assetName)
        {
            return CachedMaterialsGuidTable[assetName];
        }

        public static MaterialInstance GetMaterialInstance(Guid assetGuid)
        {
            return CachedMaterials.TryGetValue(assetGuid, out var instance) ? instance : null;
        }

        public static MaterialInfo GetMaterialInfo(Guid assetGuid)
        {
            var materialInstance = CachedMaterials[assetGuid];

            return new MaterialInfo(assetGuid, materialInstance.ShaderQueue);
        }

        public static MaterialInfo GetMaterialInfo(MaterialInstance materialInstance)
        {
            var newGuid = Guid.NewGuid();

            CachedMaterials.Add(newGuid, materialInstance);

            return new MaterialInfo(newGuid, materialInstance.ShaderQueue);
        }
    }
}
