using System.IO;
using System;
using System.IO.Compression;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement.AssetsMeta;

namespace CopperCowEngine.AssetsManagement.FSWorkers
{
    internal class NativeReader
    {
        public bool LoadAssetFile(BaseAsset asset)
        {
            var path = NativeFileSystemWorker.GetAssetPath(asset);
            var result = true;
            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Load file error: {0}", ex.Message);
                asset.Type = AssetTypes.Invalid;
                result = false;
            }
            return result;
        }
    }
}