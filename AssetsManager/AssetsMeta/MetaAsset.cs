using AssetsManager.Loaders;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class MetaAsset : BaseAsset
    {
        public AssetTypes InfoType;
        public MetaAsset() {
            this.Type = AssetTypes.Meta;
        }

        public override bool ImportAsset(string path, string ext) {
            Console.WriteLine("Can't create MetaAsset, this type only for info.");
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            Console.WriteLine("Can't create MetaAsset, this type only for info.");
        }

        public override bool LoadAsset(BinaryReader reader) {
            return base.LoadAsset(reader);
        }
    }
}
