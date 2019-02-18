using AssetsManager.Loaders;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class Texture2DAsset : BaseAsset
    {
        public TextureAssetData Data;

        public Texture2DAsset() {
            this.Type = AssetTypes.Texture2D;
        }

        public override bool ImportAsset(string path, string ext) {
            Data = TextureLoader.LoadTexture(path);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write(Data.Width);
            writer.Write(Data.Height);
            writer.Write(Data.buffer);
        }

        public override bool LoadAsset(BinaryReader reader) {
            if(!base.LoadAsset(reader)) {
                return false;
            }
            Data = new TextureAssetData();
            Data.Width = reader.ReadInt32();
            Data.Height = reader.ReadInt32();
            Data.buffer = reader.ReadBytes(Data.Width * Data.Height * 4);
            return true;
        }
    }
}
