using AssetsManager.AssetsMeta;
using SharpDX.WIC;
using System.Collections.Generic;

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
        private static ImagingFactory2 _factory;
        private static ImagingFactory2 Factory
        {
            get {
                if (_factory == null) {
                    _factory = new ImagingFactory2();
                }
                return _factory;
            }
        }

        private static BitmapSource LoadBitmap(string filename, out bool IsSRgb) {
            var bitmapDecoder = new BitmapDecoder(
                Factory,
                filename,
                DecodeOptions.CacheOnDemand
            );

            BitmapFrameDecode bitmapFrameDecode = bitmapDecoder.GetFrame(0);
            var metaReader = bitmapFrameDecode.MetadataQueryReader;

            if (metaReader == null) {
                IsSRgb = false;
            } else {
                var list = metaReader.QueryPaths;
                Dictionary<string, object> test = new Dictionary<string, object>();
                foreach (var item in list) {
                    test.Add(item, metaReader.GetMetadataByName(item));
                }

                if (metaReader.TryGetMetadataByName("/sRGB/RenderingIntent", out object t).Success) {
                    if ((ushort)t == 1) {
                        IsSRgb = true;
                    }
                }

                if (metaReader.TryGetMetadataByName("/app1/ifd/exif/{ushort=40961}", out t).Success) {
                    if ((ushort)t == 1) {
                        IsSRgb = true;
                    }
                }
                
                IsSRgb = false;
            }
            
            if (IsSRgb) {
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

        static public TextureAssetData LoadTexture(string path, bool forceSRgb) {
            BitmapSource bit = LoadBitmap(path, out bool IsSRgb);
            int stride = bit.Size.Width * 4;
            byte[] data = new byte[bit.Size.Height * stride];
            bit.CopyPixels(data, stride);

            return new TextureAssetData() {
                Width = bit.Size.Width,
                Height = bit.Size.Height,
                ChannelsCount = ChannelsCountEnum.Four,
                BytesPerChannel = BytesPerChannelEnum.One,
                ColorSpace = (forceSRgb || IsSRgb) ? ColorSpaceEnum.Gamma : ColorSpaceEnum.Linear,
                buffer = data,
            };
        }

        private static string[] CubePostfixes = new string[] {
            //"ft", "bk", "dn", "up", "lf", "rt",
            "ft", "bk","up", "dn", "rt", "lf",
        };
        static public TextureCubeAssetData LoadCubeTexture(string path) {
            string[] splitName = path.Split('.');

            BitmapSource bit;
            int stride, w, h;
            w = h = -1;

            List<byte[][]> buffers = new List<byte[][]>();

            for (int i = 0; i < 6; i++) {
                string p = splitName[0] + "_" + CubePostfixes[i] + "." + splitName[1];

                bit = LoadBitmap(p, out bool isSRgb);
                if (w == -1) {
                    w = bit.Size.Width;
                    h = bit.Size.Height;
                }
                stride = bit.Size.Width * 4;

                byte[][] data = new byte[1][];
                data[0] = new byte[bit.Size.Height * stride];
                bit.CopyPixels(data[0], stride);
                buffers.Add(data);
            }

            return new TextureCubeAssetData() {
                Width = w,
                Height = h,
                ChannelsCount = 4,
                BytesPerChannel = 1,
                ColorSpace = ColorSpaceEnum.Gamma,
                buffer = buffers.ToArray(),
            };
        }

        static private NativeUtilsNS.NativeUtils m_NativeUtilsRef;
        static private NativeUtilsNS.NativeUtils NativeUtilsRef {
            get {
                if (m_NativeUtilsRef == null) {
                    m_NativeUtilsRef = new NativeUtilsNS.NativeUtils();
                }
                return m_NativeUtilsRef;
            }
        }

        static public float[] LoadHDRTexture(string path, out int width, out int height, out int pixelSize)
        {
            NativeUtilsNS.ImageData imageData = NativeUtilsRef.LoadHDRImage(path);
            float[] imageFloats = imageData.Data.ToArray();
            width = imageData.Width;
            height = imageData.Height;
            pixelSize = sizeof(float) * 3;
            return imageFloats;
        }
    }
}
