using AssetsManager.AssetsMeta;
using System.IO;

namespace AssetsManager.FSWorkers
{
    internal class NativeFSWorker
    {
        private NativeWriter WriterRef;
        private NativeReader ReaderRef;
        internal static string RootPath = "";
        internal static string Extension = ".cceaf";
        internal static string ShaderExtension = ".ccesf";

        public NativeFSWorker() {
            WriterRef = new NativeWriter();
            ReaderRef = new NativeReader();
        }

        public bool CreateAssetFile(BaseAsset asset) {
            return CreateAssetFile(asset, false);
        }

        public bool CreateAssetFile(BaseAsset asset, bool rewrite) {
            return WriterRef.CreateAssetFile(asset, rewrite);
        }

        public bool LoadAssetFile(BaseAsset asset) {
            return ReaderRef.LoadAssetFile(asset);
        }

        internal string[] DetectAssetsNamesByType(AssetTypes type) {
            string path = GetAssetTypePath(type);
            if (!Directory.Exists(path)) {
                return new string[0];
            }
            return Directory.GetFiles(path);
        }

        internal static string GetAssetTypePath(AssetTypes type) {
            string path = "Assets";
            if (!string.IsNullOrEmpty(RootPath)) {
                path = Path.Combine(RootPath, path);
            }

            switch (type) {
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
                default:
                    break;
            }
            return path;
        }

        internal static string GetAssetPath(BaseAsset asset) {
            return GetAssetPath(asset, "");
        }

        internal static string GetAssetPath(BaseAsset asset, string posibleName) {
            string path;
            AssetTypes tType;
            if (asset.Type == AssetTypes.Meta) {
                MetaAsset MA = ((MetaAsset)asset);
                tType = MA.InfoType;
            } else {
                tType = asset.Type;
            }

            path = GetAssetTypePath(tType);
            string name = asset.Name;
            if (!string.IsNullOrEmpty(posibleName)) {
                name = posibleName;
            }
            string file = name + (tType == AssetTypes.Shader ? ShaderExtension : Extension);
            path = Path.Combine(path, file);
            return path;
        }
    }
}
