using System.IO;
using CopperCowEngine.AssetsManagement.AssetsMeta;

namespace CopperCowEngine.AssetsManagement.FSWorkers
{
    internal class NativeFileSystemWorker
    {
        internal static string RootPath = "";
        internal static string Extension = ".cceaf";
        internal static string ShaderExtension = ".ccesf";

        private readonly NativeWriter _writer;
        private readonly NativeReader _reader;

        public NativeFileSystemWorker()
        {
            _writer = new NativeWriter();
            _reader = new NativeReader();
        }

        public bool CreateAssetFile(BaseAsset asset)
        {
            return CreateAssetFile(asset, false);
        }

        public bool CreateAssetFile(BaseAsset asset, bool rewrite)
        {
            return _writer.CreateAssetFile(asset, rewrite);
        }

        public bool LoadAssetFile(BaseAsset asset)
        {
            return _reader.LoadAssetFile(asset);
        }

        internal string[] DetectAssetsNamesByType(AssetTypes type)
        {
            var path = GetAssetTypePath(type);
            return !Directory.Exists(path) ? new string[0] : Directory.GetFiles(path);
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
                    break;
            }
            return path;
        }

        internal static string GetAssetPath(BaseAsset asset)
        {
            return GetAssetPath(asset, "");
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
    }
}
