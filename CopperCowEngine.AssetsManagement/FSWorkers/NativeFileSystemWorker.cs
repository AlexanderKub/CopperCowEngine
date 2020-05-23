using System;
using System.IO;
using CopperCowEngine.AssetsManagement.AssetsMeta;

namespace CopperCowEngine.AssetsManagement.FSWorkers
{
    internal static class NativeFileSystemWorker
    {
        internal static string RootPath = "";
        private const string Extension = ".cceaf";
        private const string ShaderExtension = ".ccesf";

        public static bool LoadAssetFile(BaseAsset asset)
        {
            return NativeReader.LoadAssetFile(asset);
        }

        internal static string GetAssetPath(BaseAsset asset)
        {
            return GetAssetPath(asset, string.Empty);
        }

        internal static string GetAssetPath(BaseAsset asset, string possibleName)
        {
            AssetTypes assetType;
            // TODO: WTF
            if (asset is MetaAsset metaAsset)
            {
                assetType = metaAsset.InfoType;
            }
            else
            {
                assetType = asset.Type;
            }

            var path = GetAssetTypePath(assetType);
            var name = asset.Name;
            if (!string.IsNullOrEmpty(possibleName))
            {
                name = possibleName;
            }
            var file = name + (assetType == AssetTypes.Shader ? ShaderExtension : Extension);
            path = Path.Combine(path, file);
            return path;
        }

        internal static string GetAssetTypePath(AssetTypes type)
        {
            var path = "Assets";
            if (!string.IsNullOrEmpty(RootPath))
            {
                path = Path.Combine(RootPath, path);
            }

            switch (type)
            {
                case AssetTypes.Mesh:
                    path = Path.Combine(path, "Meshes");
                    break;
                case AssetTypes.Texture2D:
                    path = Path.Combine(path, "Textures");
                    break;
                case AssetTypes.TextureCube:
                    path = Path.Combine(path, "Textures");
                    break;
                case AssetTypes.Material:
                    path = Path.Combine(path, "Materials");
                    break;
                case AssetTypes.Shader:
                    path = Path.Combine(path, "Shaders");
                    break;
                case AssetTypes.Invalid:
                    break;
                case AssetTypes.Meta:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return path;
        }
    }
}
