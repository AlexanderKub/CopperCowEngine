using AssetsManager.AssetsMeta;
using SharpDX.WIC;
using System.Collections.Generic;
using System.Linq;

namespace AssetsManager.Loaders
{
    public enum ColorSpaceEnum
    {
        Gamma,
        Linear,
    }

    public enum ChannelsCountEnum
    {
        One = 1,
        Two = 2,
        Four = 4,
    }

    public enum BytesPerChannelEnum
    {
        One = 1,
        Two = 2,
        Four = 4,
    }

    internal class TextureLoader
    {
        // TODO: D3D11 specific code
        private static ImagingFactory2 _factory;
        private static ImagingFactory2 Factory => _factory ?? (_factory = new ImagingFactory2());

        private static BitmapSource LoadBitmap(string filename, out bool isSRgb)
        {
            var bitmapDecoder = new BitmapDecoder(
                Factory,
                filename,
                DecodeOptions.CacheOnDemand
            );

            var bitmapFrameDecode = bitmapDecoder.GetFrame(0);
            var metaReader = bitmapFrameDecode.MetadataQueryReader;

            if (metaReader == null)
            {
                isSRgb = false;
            }
            else
            {
                var list = metaReader.QueryPaths;
                var test = list.ToDictionary(item => item, item => metaReader.GetMetadataByName(item));

                if (metaReader.TryGetMetadataByName("/sRGB/RenderingIntent", out var t).Success)
                {
                    if ((ushort)t == 1)
                    {
                        isSRgb = true;
                    }
                }

                if (metaReader.TryGetMetadataByName("/app1/ifd/exif/{ushort=40961}", out t).Success)
                {
                    if ((ushort)t == 1)
                    {
                        isSRgb = true;
                    }
                }

                isSRgb = false;
            }

            if (isSRgb)
            {
                System.Console.WriteLine(filename + " SRGB!");
            }

            var formatConverter = new FormatConverter(Factory);

            formatConverter.Initialize(
                bitmapFrameDecode,
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }

        public static TextureAssetData LoadTexture(string path, bool forceSRgb)
        {
            var bit = LoadBitmap(path, out var isSRgb);
            var stride = bit.Size.Width * 4;
            var data = new byte[bit.Size.Height * stride];
            bit.CopyPixels(data, stride);

            return new TextureAssetData()
            {
                Width = bit.Size.Width,
                Height = bit.Size.Height,
                ChannelsCount = ChannelsCountEnum.Four,
                BytesPerChannel = BytesPerChannelEnum.One,
                ColorSpace = (forceSRgb || isSRgb) ? ColorSpaceEnum.Gamma : ColorSpaceEnum.Linear,
                Buffer = data,
            };
        }

        private static readonly string[] CubePostfixes = new string[] {
            //"ft", "bk", "dn", "up", "lf", "rt",
            "ft", "bk","up", "dn", "rt", "lf",
        };
        public static TextureCubeAssetData LoadCubeTexture(string path)
        {
            var splitName = path.Split('.');

            int h;
            var w = h = -1;

            var buffers = new List<byte[][]>();

            for (var i = 0; i < 6; i++)
            {
                var p = splitName[0] + "_" + CubePostfixes[i] + "." + splitName[1];

                var bit = LoadBitmap(p, out var isSRgb);
                if (w == -1)
                {
                    w = bit.Size.Width;
                    h = bit.Size.Height;
                }
                var stride = bit.Size.Width * 4;

                var data = new byte[1][];
                data[0] = new byte[bit.Size.Height * stride];
                bit.CopyPixels(data[0], stride);
                buffers.Add(data);
            }

            return new TextureCubeAssetData()
            {
                Width = w,
                Height = h,
                ChannelsCount = 4,
                BytesPerChannel = 1,
                ColorSpace = ColorSpaceEnum.Gamma,
                Buffer = buffers.ToArray(),
            };
        }

        private static NativeUtilsNS.NativeUtils _nativeUtilsRef;

        private static NativeUtilsNS.NativeUtils NativeUtilsRef => _nativeUtilsRef ?? (_nativeUtilsRef = new NativeUtilsNS.NativeUtils());

        public static float[] LoadHdrTexture(string path, out int width, out int height, out int pixelSize)
        {
            var imageData = NativeUtilsRef.LoadHDRImage(path);
            var imageFloats = imageData.Data.ToArray();
            width = imageData.Width;
            height = imageData.Height;
            pixelSize = sizeof(float) * 3;
            return imageFloats;
        }
    }
}
