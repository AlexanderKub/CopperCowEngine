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

        public override void CopyValues(BaseAsset source)
        {
        }

        public override bool ImportAsset(string path, string ext)
        {
            Data = AssetsManager.RenderBackend.ImportTexture(path, ForceSRgb);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            base.SaveAsset(writer);
            writer.Write(Data.Width);
            writer.Write(Data.Height);
            writer.Write((int)Data.ChannelsCount);
            writer.Write((int)Data.BytesPerChannel);
            writer.Write((int)Data.ColorSpace);
            writer.Write(Data.Buffer);
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
