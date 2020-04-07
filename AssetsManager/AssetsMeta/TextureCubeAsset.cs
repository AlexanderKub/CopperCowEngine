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
        public byte[][][] Buffer;
    }

    public class TextureCubeAsset : BaseAsset
    {
        public TextureCubeAssetData Data;

        public TextureCubeAsset()
        {
            Type = AssetTypes.TextureCube;
        }

        public override void CopyValues(BaseAsset source)
        {
        }

        public override bool ImportAsset(string path, string ext)
        {
            Data = TextureLoader.LoadCubeTexture(path);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            base.SaveAsset(writer);
            writer.Write(Data.Width);
            writer.Write(Data.Height);
            writer.Write(Data.ChannelsCount);
            writer.Write(Data.BytesPerChannel);
            writer.Write((int)Data.ColorSpace);
            writer.Write(Data.MipLevels);
            for (var mip = 0; mip < Data.MipLevels; mip++)
            {
                for (var i = 0; i < 6; i++)
                {
                    writer.Write(Data.Buffer[i][mip]);
                }
            }
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }

            Data = new TextureCubeAssetData
            {
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
                ChannelsCount = reader.ReadInt32(),
                BytesPerChannel = reader.ReadInt32(),
                ColorSpace = (ColorSpaceEnum) reader.ReadInt32(),
                MipLevels = reader.ReadInt32(),
                Buffer = new byte[6][][]
            };

            for (var i = 0; i < 6; i++)
            {
                Data.Buffer[i] = new byte[Data.MipLevels][];
            }

            for (var mip = 0; mip < Data.MipLevels; mip++)
            {
                var mipSize = (int)(Data.Width * System.Math.Pow(0.5, mip));
                for (var i = 0; i < 6; i++)
                {
                    Data.Buffer[i][mip] = reader.ReadBytes(mipSize * mipSize * Data.ChannelsCount * Data.BytesPerChannel);
                }
            }
            return true;
        }
    }
}
