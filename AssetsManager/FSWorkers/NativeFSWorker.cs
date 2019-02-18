using AssetsManager.AssetsMeta;
using System.IO;

namespace AssetsManager.FSWorkers
{
    internal class NativeFSWorker
    {
        private NativeWriter WriterRef;
        private NativeReader ReaderRef;
        internal static string Extension = ".mdxaf";
        internal static string ShaderExtension = ".mdxsaf";
        
        public NativeFSWorker() {
            WriterRef = new NativeWriter();
            ReaderRef = new NativeReader();
        }
         
        public bool CreateAssetFile(BaseAsset asset) {
            return WriterRef.CreateAssetFile(asset);
        }

        public bool LoadAssetFile(BaseAsset asset) {
            return ReaderRef.LoadAssetFile(asset);
        }

        internal string[] DetectAssetsNamesByType(AssetTypes type) {
            return Directory.GetFiles(GetAssetTypePath(type));
        }

        internal static string GetAssetTypePath(AssetTypes type) {
            string path = "Assets/";
            switch (type) {
                case AssetTypes.Mesh:
                    path += "Meshes/";
                    break;
                case AssetTypes.Texture2D:
                    path += "Textures/";
                    break;
                case AssetTypes.TextureCube:
                    path += "Textures/";
                    break;
                case AssetTypes.Material:
                    path += "Materials/";
                    break;
                case AssetTypes.Shader:
                    path += "Shaders/";
                    break;
                default:
                    break;
            }
            return path;
        }
        internal static string GetAssetPath(BaseAsset asset) {
            string path;
            AssetTypes tType;
            if (asset.Type == AssetTypes.Meta) {
                MetaAsset MA = ((MetaAsset)asset);
                tType = MA.InfoType;
            } else {
                tType = asset.Type;
            }

            path = GetAssetTypePath(tType);
            path += asset.Name;
            path += (tType == AssetTypes.Shader ? ShaderExtension : Extension);
            return path;
        }
    }
}
