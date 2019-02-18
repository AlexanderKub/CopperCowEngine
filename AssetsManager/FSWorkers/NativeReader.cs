using System.IO;
using AssetsManager.AssetsMeta;
using System;

namespace AssetsManager.FSWorkers
{
    internal class NativeReader
    {
        public bool LoadAssetFile(BaseAsset asset) {
            string path = NativeFSWorker.GetAssetPath(asset);
            bool result = true;
            try {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream)) {
                    result = asset.LoadAsset(reader);
                    reader.Close();
                }
            } catch (Exception ex) {
                Console.WriteLine("Load file error: {0}", ex.Message);
                asset.Type = AssetTypes.Invalid;
                result = false;
            }
            return result;
        }
    }
}
