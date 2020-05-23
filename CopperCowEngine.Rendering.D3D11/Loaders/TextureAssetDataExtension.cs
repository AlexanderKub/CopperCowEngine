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
            return data.ChannelsCount switch
            {
                ChannelsCountType.One => (data.BytesPerChannel switch
                {
                    BytesPerChannelType.One => Format.R8_UNorm,
                    BytesPerChannelType.Two => Format.R16_Float,
                    BytesPerChannelType.Four => Format.R32_Float,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                ChannelsCountType.Two => (data.BytesPerChannel switch
                {
                    BytesPerChannelType.One => Format.R8G8_UNorm,
                    BytesPerChannelType.Two => Format.R16G16_Float,
                    BytesPerChannelType.Four => Format.R32G32_Float,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                ChannelsCountType.Three => (data.BytesPerChannel switch
                {
                    BytesPerChannelType.Two => Format.R32G32B32_Float,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                ChannelsCountType.Four => (data.BytesPerChannel switch
                {
                    BytesPerChannelType.One => (data.ColorSpace switch
                    {
                        ColorSpaceType.Gamma => Format.R8G8B8A8_UNorm_SRgb,
                        ColorSpaceType.Linear => Format.R8G8B8A8_UNorm,
                        _ => throw new ArgumentOutOfRangeException()
                    }),
                    BytesPerChannelType.Two => Format.R16G16B16A16_Float,
                    BytesPerChannelType.Four => Format.R32G32B32A32_Float,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
