using System.IO;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.FSWorkers;

namespace CopperCowEngine.AssetsManagement.Editor.FileSystemWorker
{
    internal static class EditorFileSystemWorker
    {
        internal static string RootPath = "";
        private const string Extension = ".cceaf";
        private const string ShaderExtension = ".ccesf";

        public static bool CreateAssetFile(BaseAsset asset, bool rewrite = false)
        {
            return NativeWriter.CreateAssetFile(asset, rewrite);
        }

        internal static string[] DetectAssetsNamesByType(AssetTypes type)
        {
            var path = NativeFileSystemWorker.GetAssetTypePath(type);
            return !Directory.Exists(path) ? new string[0] : Directory.GetFiles(path);
        }
    }
}
