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

        public override void CopyValues(BaseAsset source)
        {
        }

        public override bool ImportAsset(string path, string ext)
        {
            Console.WriteLine("Can't create MetaAsset, this type only for info.");
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            Console.WriteLine("Can't create MetaAsset, this type only for info.");
        }
    }
}