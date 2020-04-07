using System;
using CopperCowEngine.Rendering.Loaders;
using SharpDX.DXGI;
using ColorSpaceType = CopperCowEngine.Rendering.Loaders.ColorSpaceType;

namespace CopperCowEngine.Rendering.D3D11.Loaders
{
    internal static class TextureAssetDataExtension
    {
        public static Format GetFormat(this TextureAssetData data)
        {
            switch (data.ChannelsCount)
            {
                case ChannelsCountType.One:
                    switch (data.BytesPerChannel)
                    {
                        case BytesPerChannelType.One:
                            return Format.R8_UNorm;
                        case BytesPerChannelType.Two:
                            return Format.R16_Float;
                        case BytesPerChannelType.Four:
                            return Format.R32_Float;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ChannelsCountType.Two:
                    switch (data.BytesPerChannel)
                    {
                        case BytesPerChannelType.One:
                            return Format.R8G8_UNorm;
                        case BytesPerChannelType.Two:
                            return Format.R16G16_Float;
                        case BytesPerChannelType.Four:
                            return Format.R32G32_Float;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case ChannelsCountType.Four:
                    switch (data.BytesPerChannel)
                    {
                        case BytesPerChannelType.One:
                            switch (data.ColorSpace)
                            {
                                case ColorSpaceType.Gamma:
                                    return Format.R8G8B8A8_UNorm_SRgb;
                                case ColorSpaceType.Linear:
                                    return Format.R8G8B8A8_UNorm;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        case BytesPerChannelType.Two:
                            return Format.R16G16B16A16_Float;
                        case BytesPerChannelType.Four:
                            return Format.R32G32B32A32_Float;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
