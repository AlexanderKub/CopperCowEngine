using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Device = SharpDX.Direct3D11.Device;

namespace AssetsManager.Loaders
{
    internal static class SaveToWICImage
    {
        private static ImagingFactory2 _factory;
        private static ImagingFactory2 Factory {
            get {
                if (_factory == null) {
                    _factory = new ImagingFactory2();
                }
                return _factory;
            }
        }

        private static string[] CubePostfixes = new string[] {
            "ft", "bk", "up", "dn", "rt", "lf",
        };

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

            if (texture.Description.ArraySize == 1) {
                if (textureCopy.Description.Format == Format.R16G16_Float) {
                    DataBox dataBox = context.MapSubresource(
                        textureCopy,
                        0, 0,
                        MapMode.Read,
                        SharpDX.Direct3D11.MapFlags.None,
                        out DataStream dataStream);
                    
                    AssetsMeta.Texture2DAsset asset2D = new AssetsMeta.Texture2DAsset();
                    asset2D.Name = path;
                    asset2D.Data.Width = textureCopy.Description.Width;
                    asset2D.Data.Height = textureCopy.Description.Height;
                    asset2D.Data.ColorSpace = ColorSpaceEnum.Linear;
                    asset2D.Data.ChannelsCount = ChannelsCountEnum.Two;
                    asset2D.Data.BytesPerChannel = BytesPerChannelEnum.Two;
                    asset2D.Data.buffer = ReadFully(dataStream);
                    AssetsManagerInstance.GetManager().FSWorker.CreateAssetFile(asset2D, true);
                    context.UnmapSubresource(textureCopy, 0);
                    textureCopy.Dispose();
                    return;
                }
                InternalSaveTexture($"{path}.png", 0, 0, Factory, textureCopy, context);
                textureCopy.Dispose();
                return;
            }

            AssetsMeta.TextureCubeAsset asset = new AssetsMeta.TextureCubeAsset();
            asset.Name = path;
            asset.Data.Width = textureCopy.Description.Width;
            asset.Data.Height = textureCopy.Description.Height;
            asset.Data.ColorSpace = ColorSpaceEnum.Linear;
            asset.Data.ChannelsCount = 4;
            asset.Data.BytesPerChannel = 2;
            asset.Data.MipLevels = withMips ? textureCopy.Description.MipLevels : 1;
            asset.Data.buffer = new byte[6][][];

            for (int i = 0; i < 6; i++) {
                asset.Data.buffer[i] = new byte[asset.Data.MipLevels][];
                for (int mip = 0; mip < asset.Data.MipLevels; mip++) {
                    DataBox dataBox = context.MapSubresource(
                        textureCopy,
                        mip,
                        i,
                        MapMode.Read,
                        SharpDX.Direct3D11.MapFlags.None,
                        out DataStream dataStream);

                    byte[] allMipBytes = ReadFully(dataStream);
                    dataStream.Dispose();

                    int mipSize = (int)(asset.Data.Width * Math.Pow(0.5, mip));
                    int pitch = mipSize * asset.Data.ChannelsCount * asset.Data.BytesPerChannel;
                    int n = mipSize * pitch;

                    asset.Data.buffer[i][mip] = new byte[n];

                    for (int j = 0; j < mipSize; j++) {
                        for (int k = 0; k < pitch; k++) {
                            asset.Data.buffer[i][mip][j * pitch + k] = allMipBytes[j * dataBox.RowPitch + k];
                        }
                    }

                    context.UnmapSubresource(textureCopy, textureCopy.CalculateSubResourceIndex(mip, i, out int m));

                    // Dont work cause wrong dataBox.RowPitch on mip levels issue.
                    // asset.Data.buffer[i][mip] = ReadFully(dataStream);
                }
            }
            AssetsManagerInstance.GetManager().FSWorker.CreateAssetFile(asset, true);

            // DEBUG RO PNG
            /*if (textureCopy.Description.MipLevels != 5) {
                textureCopy.Dispose();
                return;
            }

            for (int mip = 0; mip < textureCopy.Description.MipLevels; mip++) {
                for (int i = 0; i < texture.Description.ArraySize; i++) {
                    InternalSaveTexture($"{path}_{CubePostfixes[i]}_mip{mip}.png", i, mip, Factory, textureCopy, context);
                }
            }*/

            textureCopy.Dispose();
        }

        private static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream()) {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static void InternalSaveTexture(string path, int arraySlice, int mipSlice, ImagingFactory2 Factory, Texture2D textureCopy, DeviceContext context)
        {
            DataStream dataStream;
            var dataBox = context.MapSubresource(
                textureCopy,
                mipSlice,
                arraySlice,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out dataStream);

            DataRectangle dataRectangle = new DataRectangle
            {
                DataPointer = dataStream.DataPointer,
                Pitch = dataBox.RowPitch
            };

            Guid m_PixelFormat = PixelFormat.Format64bppRGBAHalf;

            if (textureCopy.Description.Format == Format.R16G16_Float) {
                m_PixelFormat = PixelFormat.Format32bppGrayFloat;
            }

            if (textureCopy.Description.Format == Format.R8G8B8A8_UNorm) {
                m_PixelFormat = PixelFormat.Format32bppBGRA;
            }

            int mipSize = (int)(textureCopy.Description.Width * Math.Pow(0.5, mipSlice));

            var bitmap = new Bitmap(
                Factory,
                mipSize,
                mipSize,
                m_PixelFormat,
                dataRectangle);

            using (var s = new FileStream(path, FileMode.OpenOrCreate)) {//CREATE
                s.Position = 0;
                using (var bitmapEncoder = new PngBitmapEncoder(Factory, s)) {
                    using (var bitmapFrameEncode = new BitmapFrameEncode(bitmapEncoder)) {
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
