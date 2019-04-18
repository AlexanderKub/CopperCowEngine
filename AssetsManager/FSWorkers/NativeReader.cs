using System.IO;
using AssetsManager.AssetsMeta;
using System;
using System.IO.Compression;
using System.Collections.Generic;

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

                    /*int len = (int)reader.BaseStream.Length;
                    byte[] encrypted = reader.ReadBytes(len);

                    using (var inStream = new MemoryStream(encrypted))
                    using (GZipStream gzip = new GZipStream(inStream, CompressionMode.Decompress))
                    using (var outStream = new MemoryStream()) {
                        gzip.CopyTo(outStream);
                        using (BinaryReader decompressReader = new BinaryReader(outStream)) {
                            var test = outStream.ToArray();
                            decompressReader.BaseStream.Position = 0;
                            result = asset.LoadAsset(decompressReader);
                        }
                    }*/
                    result = asset.LoadAsset(reader);
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
