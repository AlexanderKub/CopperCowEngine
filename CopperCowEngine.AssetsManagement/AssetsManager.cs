using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.FSWorkers;

[assembly: InternalsVisibleTo("CopperCowEngine.AssetsManagement.Editor")]
namespace CopperCowEngine.AssetsManagement
{
    public class AssetsManager
    {
        private string _rootPath;

        private static AssetsManager _instance;

        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
                NativeFileSystemWorker.RootPath = _rootPath;
            }
        }

        public static AssetsManager GetManager()
        {
            return _instance ??= new AssetsManager();
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public T LoadAsset<T>(string name) where T : BaseAsset, new()
        {
            var asset = new T
            {
                Name = name
            };
            NativeFileSystemWorker.LoadAssetFile(asset);
            return asset;
        }
    }
}
