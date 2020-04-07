using System.Collections.Generic;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.Loaders
{
    public class MaterialLoader
    {
        private static readonly Dictionary<string, MaterialInstance> CachedMaterials = new Dictionary<string, MaterialInstance>();

        public static MaterialInstance LoadMaterial(string assetName)
        {
            if (CachedMaterials.ContainsKey(assetName))
            {
                return CachedMaterials[assetName];
            }

            var materialAsset = AssetsManager.GetManager().LoadAsset<MaterialAsset>(assetName);
            var mat = new MaterialInstance()
            {
                Name = materialAsset.Name,
                AlbedoMapAsset = materialAsset.AlbedoMapAsset,
                RoughnessMapAsset = materialAsset.RoughnessMapAsset,
                MetallicMapAsset = materialAsset.MetallicMapAsset,
                NormalMapAsset = materialAsset.NormalMapAsset,
                OcclusionMapAsset = materialAsset.OcclusionMapAsset,
                PropertyBlock = new MaterialPropertyBlock()
                {
                    AlbedoColor = materialAsset.AlbedoColor,
                    AlphaValue = materialAsset.AlphaValue,
                    MetallicValue = materialAsset.MetallicValue,
                    RoughnessValue = materialAsset.RoughnessValue,
                    Shift = materialAsset.Shift,
                    Tile = materialAsset.Tile,
                },
            };
            CachedMaterials.Add(assetName, mat);
            //Debug.Log("AssetManager", "Material " + assetName + " loaded.");
            return mat;
        }

        public static MaterialInfo LoadMaterialInfo(string assetName)
        {
            var material = LoadMaterial(assetName);
            return new MaterialInfo(assetName, material.ShaderQueue);
        }

        public static MaterialInfo LoadMaterialInfo(MaterialInstance materialInstance)
        {
            CachedMaterials.Add(materialInstance.Name, materialInstance);
            return new MaterialInfo(materialInstance.Name, materialInstance.ShaderQueue);
        }
    }
}
