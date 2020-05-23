using System.IO;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class TextureCubeAsset : BaseAsset
    {
        public TextureCubeAssetData Data;

        public TextureCubeAsset()
        {
            Type = AssetTypes.TextureCube;
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
                ColorSpace = (ColorSpaceType) reader.ReadInt32(),
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
