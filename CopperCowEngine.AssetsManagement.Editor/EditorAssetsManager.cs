using System;
using System.Collections.Generic;
using System.Linq;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor.FileSystemWorker;
using CopperCowEngine.AssetsManagement.FSWorkers;

namespace CopperCowEngine.AssetsManagement.Editor
{
    public partial class EditorAssetsManager
    {
        private static EditorAssetsManager _instance;

        public static EditorAssetsManager GetManager()
        {
            return _instance ??= new EditorAssetsManager();
        }

        private static MetaAsset LoadMetaAsset(string name, AssetTypes type)
        {
            var asset = new MetaAsset {Name = name, InfoType = type};
            NativeFileSystemWorker.LoadAssetFile(asset);
            return asset;
        }

        public void CreateAssetFile(BaseAsset asset, bool rewrite = false)
        {
            EditorFileSystemWorker.CreateAssetFile(asset, rewrite);
        }

        public static T CopyAsset<T>(T baseAsset) where T : BaseAsset, new()
        {
            var asset = Activator.CreateInstance<T>();
            asset.CopyValues(baseAsset);
            return asset;
        }

        public bool SaveAssetChanging(BaseAsset asset)
        {
            return EditorFileSystemWorker.CreateAssetFile(asset, true);
        }
    }
}

//FOR TESTING
/*public bool CreateShaderGraphAsset(string name, SharpDX.D3DCompiler.ShaderBytecode bytecode)
{
    BaseAsset asset = new ShaderAsset()
    {
        Name = name,
        ShaderType = ShaderType.Pixel,
        Bytecode = bytecode,
    };
    return FileSystemWorker.CreateAssetFile(asset, true);
}*/
