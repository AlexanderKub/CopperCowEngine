using AssetsManager.Loaders;
using SharpDX.DXGI;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public struct TextureAssetData
    {
        public int Width;
        public int Height;
        public ChannelsCountEnum ChannelsCount;
        public BytesPerChannelEnum BytesPerChannel;
        public ColorSpaceEnum ColorSpace;
        // TODO: mip levels
        public byte[] buffer;

        public Format GetFormat {
            get {
                switch (ChannelsCount) {
                    case ChannelsCountEnum.One:
                        switch (BytesPerChannel) {
                            case BytesPerChannelEnum.One:
                                return Format.R8_UNorm;
                            case BytesPerChannelEnum.Two:
                                return Format.R16_Float;
                            case BytesPerChannelEnum.Four:
                                return Format.R32_Float;
                        }
                        break;
                    case ChannelsCountEnum.Two:
                        switch (BytesPerChannel) {
                            case BytesPerChannelEnum.One:
                                return Format.R8G8_UNorm;
                            case BytesPerChannelEnum.Two:
                                return Format.R16G16_Float;
                            case BytesPerChannelEnum.Four:
                                return Format.R32G32_Float;
                        }
                        break;
                    case ChannelsCountEnum.Four:
                        switch (BytesPerChannel) {
                            case BytesPerChannelEnum.One:
                                switch (ColorSpace) {
                                    case ColorSpaceEnum.Gamma:
                                        return Format.R8G8B8A8_UNorm_SRgb;
                                    case ColorSpaceEnum.Linear:
                                        return Format.R8G8B8A8_UNorm;
                                }
                                break;
                            case BytesPerChannelEnum.Two:
                                return Format.R16G16B16A16_Float;
                            case BytesPerChannelEnum.Four:
                                return Format.R32G32B32A32_Float;
                        }
                        break;
                }
                return Format.R8G8B8A8_UNorm_SRgb;
            }
        }

        public bool GetMips {
            get {
                return ChannelsCount == ChannelsCountEnum.Four;
            }
        }
    }

    public class Texture2DAsset : BaseAsset
    {
        public TextureAssetData Data;
        public bool ForceSRgb;

        public Texture2DAsset() {
            this.Type = AssetTypes.Texture2D;
        }

        public override bool ImportAsset(string path, string ext) {
            Data = TextureLoader.LoadTexture(path, ForceSRgb);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write(Data.Width);
            writer.Write(Data.Height);
            writer.Write((int)Data.ChannelsCount);
            writer.Write((int)Data.BytesPerChannel);
            writer.Write((int)Data.ColorSpace);
            writer.Write(Data.buffer);
        }

        public override bool LoadAsset(BinaryReader reader) {
            if(!base.LoadAsset(reader)) {
                return false;
            }
            Data = new TextureAssetData();
            Data.Width = reader.ReadInt32();
            Data.Height = reader.ReadInt32();
            int channelsCount = reader.ReadInt32();
            Data.ChannelsCount = (ChannelsCountEnum)channelsCount;
            int bytesPerChannel = reader.ReadInt32();
            Data.BytesPerChannel = (BytesPerChannelEnum)bytesPerChannel;
            Data.ColorSpace = (ColorSpaceEnum)reader.ReadInt32();
            Data.buffer = reader.ReadBytes(Data.Width * Data.Height * channelsCount * bytesPerChannel);
            return true;
        }
    }
}
