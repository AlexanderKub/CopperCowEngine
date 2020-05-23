using System.IO;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class Texture2DAsset : BaseAsset
    {
        public TextureAssetData Data;
        public bool ForceSRgb;

        public Texture2DAsset()
        {
            this.Type = AssetTypes.Texture2D;
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }

            Data = new TextureAssetData
            {
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32(),
            };
            var channelsCount = reader.ReadInt32();
            Data.ChannelsCount = (ChannelsCountType)channelsCount;

            var bytesPerChannel = reader.ReadInt32();
            Data.BytesPerChannel = (BytesPerChannelType)bytesPerChannel;

            Data.ColorSpace = (ColorSpaceType)reader.ReadInt32();
            Data.Buffer = reader.ReadBytes(Data.Width * Data.Height * channelsCount * bytesPerChannel);
            return true;
        }
    }
}
