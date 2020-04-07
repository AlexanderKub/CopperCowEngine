using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.IO;
using Device = SharpDX.Direct3D11.Device;

namespace AssetsManager.Loaders
{
    // TODO: D3D11 specific code
    internal static class SaveToWicImage
    {
        private static ImagingFactory2 _factory;

        private static ImagingFactory2 Factory => _factory ?? (_factory = new ImagingFactory2());

        public static void Save(this Texture2D texture, DeviceContext context, Device device, string path, bool withMips)
        {
            var textureCopy = new Texture2D(device, new Texture2DDescription
            {
                Width = texture.Description.Width,
                Height = texture.Description.Height,
                MipLevels = texture.Description.MipLevels,
                ArraySize = texture.Description.ArraySize,
                Format = texture.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });
            context.CopyResource(texture, textureCopy);

            if (texture.Description.ArraySize == 1)
            {
                if (textureCopy.Description.Format == Format.R16G16_Float)
                {
                    context.MapSubresource(
                        textureCopy,
                        0, 0,
                        MapMode.Read,
                        SharpDX.Direct3D11.MapFlags.None,
                        out var dataStream);

                    var asset2D = new AssetsMeta.Texture2DAsset
                    {
                        Name = path,
                        Data =
                        {
                            Width = textureCopy.Description.Width,
                            Height = textureCopy.Description.Height,
                            ColorSpace = ColorSpaceEnum.Linear,
                            ChannelsCount = ChannelsCountEnum.Two,
                            BytesPerChannel = BytesPerChannelEnum.Two,
                            Buffer = ReadFully(dataStream)
                        }
                    };
                    AssetsManagerInstance.GetManager().FileSystemWorker.CreateAssetFile(asset2D, true);
                    context.UnmapSubresource(textureCopy, 0);
                    textureCopy.Dispose();
                    return;
                }
                InternalSaveTexture($"{path}.png", 0, 0, Factory, textureCopy, context);
                textureCopy.Dispose();
                return;
            }

            AssetsMeta.TextureCubeAsset asset = new AssetsMeta.TextureCubeAsset
            {
                Name = path,
                Data =
                {
                    Width = textureCopy.Description.Width,
                    Height = textureCopy.Description.Height,
                    ColorSpace = ColorSpaceEnum.Linear,
                    ChannelsCount = 4,
                    BytesPerChannel = 2,
                    MipLevels = withMips ? textureCopy.Description.MipLevels : 1,
                    Buffer = new byte[6][][]
                }
            };

            for (var i = 0; i < 6; i++)
            {
                asset.Data.Buffer[i] = new byte[asset.Data.MipLevels][];
                for (var mip = 0; mip < asset.Data.MipLevels; mip++)
                {
                    var dataBox = context.MapSubresource(
                        textureCopy,
                        mip,
                        i,
                        MapMode.Read,
                        SharpDX.Direct3D11.MapFlags.None,
                        out var dataStream);

                    var allMipBytes = ReadFully(dataStream);
                    dataStream.Dispose();

                    var mipSize = (int)(asset.Data.Width * Math.Pow(0.5, mip));
                    var pitch = mipSize * asset.Data.ChannelsCount * asset.Data.BytesPerChannel;
                    var n = mipSize * pitch;

                    asset.Data.Buffer[i][mip] = new byte[n];

                    for (var j = 0; j < mipSize; j++)
                    {
                        for (var k = 0; k < pitch; k++)
                        {
                            asset.Data.Buffer[i][mip][j * pitch + k] = allMipBytes[j * dataBox.RowPitch + k];
                        }
                    }

                    context.UnmapSubresource(textureCopy, textureCopy.CalculateSubResourceIndex(mip, i, out _));

                    // Dont work cause wrong dataBox.RowPitch on mip levels issue.
                    // asset.Data.buffer[i][mip] = ReadFully(dataStream);
                }
            }
            AssetsManagerInstance.GetManager().FileSystemWorker.CreateAssetFile(asset, true);

            // DEBUG RO PNG
            /*if (textureCopy.Description.MipLevels != 5) {
                textureCopy.Dispose();
                return;
            }

            for (int mip = 0; mip < textureCopy.Description.MipLevels; mip++) {
                for (int i = 0; i < texture.Description.ArraySize; i++) {
                    InternalSaveTexture($"{path}_{CubePostfixes[i]}_mip{mip}.png", i, mip, factory, textureCopy, context);
                }
            }*/

            textureCopy.Dispose();
        }

        private static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static void InternalSaveTexture(string path, int arraySlice, int mipSlice, ImagingFactory factory, Texture2D textureCopy, DeviceContext context)
        {
            var dataBox = context.MapSubresource(
                textureCopy,
                mipSlice,
                arraySlice,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out var dataStream);

            var dataRectangle = new DataRectangle
            {
                DataPointer = dataStream.DataPointer,
                Pitch = dataBox.RowPitch
            };

            Guid pixelFormatGuid;

            switch (textureCopy.Description.Format)
            {
                case Format.R16G16_Float:
                    pixelFormatGuid = PixelFormat.Format32bppGrayFloat;
                    break;
                case Format.R8G8B8A8_UNorm:
                    pixelFormatGuid = PixelFormat.Format32bppBGRA;
                    break;
                default:
                    pixelFormatGuid = PixelFormat.Format64bppRGBAHalf;
                    break;
            }

            var mipSize = (int)(textureCopy.Description.Width * Math.Pow(0.5, mipSlice));

            var bitmap = new Bitmap(
                factory,
                mipSize,
                mipSize,
                pixelFormatGuid,
                dataRectangle);

            using (var s = new FileStream(path, FileMode.OpenOrCreate))
            {
                //CREATE
                s.Position = 0;
                using (var bitmapEncoder = new PngBitmapEncoder(factory, s))
                {
                    using (var bitmapFrameEncode = new BitmapFrameEncode(bitmapEncoder))
                    {
                        bitmapFrameEncode.Initialize();
                        bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                        var pixelFormat = PixelFormat.FormatDontCare;
                        bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                        bitmapFrameEncode.WriteSource(bitmap);
                        bitmapFrameEncode.Commit();
                        bitmapEncoder.Commit();
                    }
                }
            }
            bitmap.Dispose();
            context.UnmapSubresource(textureCopy, 0);
        }

        public static void DisposeFactory()
        {
            _factory?.Dispose();
            _factory = null;
        }
    }
}
