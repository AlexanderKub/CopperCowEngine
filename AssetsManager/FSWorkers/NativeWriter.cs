using System;
using System.IO;
using System.IO.Compression;
using AssetsManager.AssetsMeta;

namespace AssetsManager.FSWorkers
{
    internal class NativeWriter
    {
        public bool CreateAssetFile(BaseAsset asset)
        {
            return CreateAssetFile(asset, false);
        }

        public bool CreateAssetFile(BaseAsset asset, bool rewrite)
        {
            CreateAssetFilesTree();
            if (asset.Type == AssetTypes.Shader)
            {
                Console.WriteLine("Create asset: {0} type: {1} {2}", asset.Name,
                    ((ShaderAsset)asset).ShaderType.ToString(), asset.Type.ToString());
            }
            else
            {
                Console.WriteLine("Create asset: {0} type: {1}", asset.Name, asset.Type.ToString());
            }

            var path = NativeFileSystemWorker.GetAssetPath(asset);
            var actualName = asset.Name;
            if (!rewrite)
            {
                var postfix = 0;
                while (File.Exists(path))
                {
                    actualName = asset.Name + "_" + (++postfix);
                    path = NativeFileSystemWorker.GetAssetPath(asset, actualName);
                }
                asset.Name = actualName;
            }

            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                /*using (MemoryStream ms = new MemoryStream()) {
                    using (GZipStream compressStream = new GZipStream(ms, CompressionMode.Compress))
                    using (BinaryWriter compressWriter = new BinaryWriter(compressStream)) {
                        asset.SaveAsset(compressWriter);
                        //compressWriter.Close();
                    }
                    writer.Write(ms.ToArray());
                }*/
                asset.SaveAsset(writer);
            }
            return true;
        }

        private static bool CreateAssetFilesTree()
        {
            for (var i = 0; i < Enum.GetNames(typeof(AssetTypes)).Length; i++)
            {
                CreateAssetTypeDirectory((AssetTypes)i);
            }
            return true;
        }

        private static bool CreateAssetTypeDirectory(AssetTypes type)
        {
            var path = NativeFileSystemWorker.GetAssetTypePath(type);
            if (Directory.Exists(path))
            {
                return true;
            }
            Directory.CreateDirectory(path);
            Console.WriteLine("Create directory: {0}", path);
            return true;
        }
    }
}
