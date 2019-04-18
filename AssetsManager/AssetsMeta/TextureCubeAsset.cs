using AssetsManager.Loaders;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public struct TextureCubeAssetData
    {
        public int Width;
        public int Height;
        public int ChannelsCount;
        public int BytesPerChannel;
        public ColorSpaceEnum ColorSpace;
        public int MipLevels;
        public byte[][][] buffer;
    }

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
            writer.Write(Data.ChannelsCount);
            writer.Write(Data.BytesPerChannel);
            writer.Write((int)Data.ColorSpace);
            writer.Write(Data.MipLevels);
            for (int mip = 0; mip < Data.MipLevels; mip++) {
                for (int i = 0; i < 6; i++) {
                    writer.Write(Data.buffer[i][mip]);
                }
            }
        }

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
                return false;
            }
            Data = new TextureCubeAssetData();
            Data.Width = reader.ReadInt32();
            Data.Height = reader.ReadInt32();
            Data.ChannelsCount = reader.ReadInt32();
            Data.BytesPerChannel = reader.ReadInt32();
            Data.ColorSpace = (ColorSpaceEnum)reader.ReadInt32();
            Data.MipLevels = reader.ReadInt32();

            Data.buffer = new byte[6][][];
            for (int i = 0; i < 6; i++) {
                Data.buffer[i] = new byte[Data.MipLevels][];
            }
            for (int mip = 0; mip < Data.MipLevels; mip++) {
                int mipSize = (int)(Data.Width * System.Math.Pow(0.5, mip));
                for (int i = 0; i < 6; i++) {
                    Data.buffer[i][mip] = reader.ReadBytes(mipSize * mipSize * Data.ChannelsCount * Data.BytesPerChannel);
                }
            }
            return true;
        }
    }
}
