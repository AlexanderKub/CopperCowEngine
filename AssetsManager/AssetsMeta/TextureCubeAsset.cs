using AssetsManager.Loaders;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class TextureCubeAsset : BaseAsset
    {
        public TextureCubeAssetData Data;

        public TextureCubeAsset() {
            this.Type = AssetTypes.TextureCube;
        }

        public override bool ImportAsset(string path, string ext) {
            Data = TextureLoader.LoadCubeTexture(path);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write(Data.Width);
            writer.Write(Data.Height);
            for (int i = 0; i < 6; i++) {
                writer.Write(Data.buffer[i]);
            }
        }

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
                return false;
            }
            Data = new TextureCubeAssetData();
            Data.Width = reader.ReadInt32();
            Data.Height = reader.ReadInt32();
            byte[] t = new byte[0];
            Data.buffer = new byte[][] { t, t, t, t, t, t};
            for (int i = 0; i < 6; i++) {
                Data.buffer[i] = reader.ReadBytes(Data.Width * Data.Height * 4);
            }
            return true;
        }
    }
}
