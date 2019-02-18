using System;
using System.IO;
using AssetsManager.AssetsMeta;

namespace AssetsManager.FSWorkers
{
    internal class NativeWriter
    {
        public bool CreateAssetFile(BaseAsset asset) {
            CreateAssetFilesTree();
            Console.WriteLine("Create asset: {0} type: {1}", asset.Name, asset.Type.ToString());
            using (FileStream stream = new FileStream(NativeFSWorker.GetAssetPath(asset), FileMode.Create))
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
