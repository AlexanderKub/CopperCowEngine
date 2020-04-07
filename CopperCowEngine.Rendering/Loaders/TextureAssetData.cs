using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperCowEngine.Rendering.Loaders
{
    public enum ColorSpaceType
    {
        Gamma,
        Linear,
    }

    public enum ChannelsCountType
    {
        One = 1,
        Two = 2,
        Four = 4,
    }

    public enum BytesPerChannelType
    {
        One = 1,
        Two = 2,
        Four = 4,
    }

    public struct TextureAssetData
    {
        public int Width;
        public int Height;
        public ChannelsCountType ChannelsCount;
        public BytesPerChannelType BytesPerChannel;
        public ColorSpaceType ColorSpace;
        // TODO: mip levels
        public byte[] Buffer;
        public bool GetMips => ChannelsCount == ChannelsCountType.Four;
    }

    public struct TextureCubeAssetData
    {
        public int Width;
        public int Height;
        public int ChannelsCount;
        public int BytesPerChannel;
        public ColorSpaceType ColorSpace;
        public int MipLevels;
        public byte[][][] Buffer;
    }
}
