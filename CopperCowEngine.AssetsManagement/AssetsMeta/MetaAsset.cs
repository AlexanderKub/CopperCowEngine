using System;
using System.IO;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class MetaAsset : BaseAsset
    {
        public AssetTypes InfoType;

        public MetaAsset()
        {
            Type = AssetTypes.Meta;
        }
    }
}