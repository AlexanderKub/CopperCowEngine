using SharpDX.WIC;
using System.Collections.Generic;

namespace AssetsManager.Loaders
{
    public struct TextureAssetData
    {
        public int Width;
        public int Height;
        public byte[] buffer;
    }

    public struct TextureCubeAssetData
    {
        public int Width;
        public int Height;
        public byte[][] buffer;
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

        private static BitmapSource LoadBitmap(string filename) {
            var bitmapDecoder = new BitmapDecoder(
                Factory,
                filename,
                DecodeOptions.CacheOnDemand
            );

            var formatConverter = new FormatConverter(Factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }

        static public TextureAssetData LoadTexture(string path) {
            BitmapSource bit = LoadBitmap(path);
            int stride = bit.Size.Width * 4;
            byte[] data = new byte[bit.Size.Height * stride];
            bit.CopyPixels(data, stride);

            return new TextureAssetData() {
                Width = bit.Size.Width,
                Height = bit.Size.Height,
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

            List<byte[]> buffers = new List<byte[]>();

            for (int i = 0; i < 6; i++) {
                string p = splitName[0] + "_" + CubePostfixes[i] + "." + splitName[1];

                bit = LoadBitmap(p);
                if (w == -1) {
                    w = bit.Size.Width;
                    h = bit.Size.Height;
                }
                stride = bit.Size.Width * 4;

                byte[] data = new byte[bit.Size.Height * stride];
                bit.CopyPixels(data, stride);
                buffers.Add(data);
            }

            return new TextureCubeAssetData() {
                Width = w,
                Height = h,
                buffer = buffers.ToArray(),
            };
        }
        
    }
}
