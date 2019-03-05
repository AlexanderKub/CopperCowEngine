using System;
using System.IO;
using AssetsManager.AssetsMeta;

namespace AssetsManager.FSWorkers
{
    internal class NativeWriter
    {
        public bool CreateAssetFile(BaseAsset asset) {
            return CreateAssetFile(asset, false);
        }

        public bool CreateAssetFile(BaseAsset asset, bool rewrite) {
            CreateAssetFilesTree();
            if (asset.Type == AssetTypes.Shader) {
                Console.WriteLine("Create asset: {0} type: {1} {2}", asset.Name, 
                    ((ShaderAsset)asset).ShaderType.ToString(), asset.Type.ToString());
            } else {
                Console.WriteLine("Create asset: {0} type: {1}", asset.Name, asset.Type.ToString());
            }

            string path = NativeFSWorker.GetAssetPath(asset);
            if (!rewrite) {
                string actualName = asset.Name;
                int postfix = 0;
                while (File.Exists(path)) {
                    actualName = asset.Name + "_" + (++postfix);
                    path = NativeFSWorker.GetAssetPath(asset, actualName);
                }
                asset.Name = actualName;
            }

            using (FileStream stream = new FileStream(path, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                asset.SaveAsset(writer);
                writer.Close();
            }
            return true;
        }

        private bool CreateAssetFilesTree() {
            for (int i = 0; i < Enum.GetNames(typeof(AssetTypes)).Length; i++) {
                CreateAssetTypeDirectory((AssetTypes)i);
            }
            return true;
        }

        private bool CreateAssetTypeDirectory(AssetTypes type) {
            string path = NativeFSWorker.GetAssetTypePath(type);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                Console.WriteLine("Create directory: {0}", path);
            }
            return true;
        }
    }
}
