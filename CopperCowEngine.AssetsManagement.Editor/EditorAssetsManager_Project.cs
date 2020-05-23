using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor.FileSystemWorker;
using CopperCowEngine.AssetsManagement.FSWorkers;

namespace CopperCowEngine.AssetsManagement.Editor
{
    public partial class EditorAssetsManager
    {
        
        private Dictionary<AssetTypes, List<MetaAsset>> _cachedAssetsTable;

        public Dictionary<AssetTypes, List<MetaAsset>> LoadProjectAssets(bool refresh = true)
        {
            if (!refresh && _cachedAssetsTable != null)
            {
                return _cachedAssetsTable;
            }
            _cachedAssetsTable = new Dictionary<AssetTypes, List<MetaAsset>>();

            for (var i = 0; i < Enum.GetNames(typeof(AssetTypes)).Length; i++)
            {
                var type = (AssetTypes)i;
                if (type == AssetTypes.Invalid || type == AssetTypes.Meta)
                {
                    continue;
                }

                var names = DetectAssetsNamesByType(type);
                var result = names.Select(name => LoadMetaAsset(name, type)).Where(asset => !asset.IsInvalid).ToList();
                _cachedAssetsTable.Add(type, result);
            }
            return _cachedAssetsTable;
        }

        private static IEnumerable<string> DetectAssetsNamesByType(AssetTypes type)
        {
            var result = EditorFileSystemWorker.DetectAssetsNamesByType(type);
            for (var i = 0; i < result.Length; i++)
            {
                var arr = result[i].Split('/');
                arr = arr[^1].Split('.');
                result[i] = arr[0];
            }
            return result;
        }
    }
}
