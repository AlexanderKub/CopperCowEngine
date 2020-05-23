using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.Rendering.Loaders;
using SharpDX.WIC;

namespace CopperCowEngine.Rendering.D3D11.Editor.Loaders
{
    internal static class TextureImporter
    {
        private static ImagingFactory2 _factory;

        private static ImagingFactory2 Factory => _factory ??= new ImagingFactory2();

        private static readonly string[] CubePostfixes = {
            //"ft", "bk", "dn", "up", "lf", "rt",
            "ft", "bk","up", "dn", "rt", "lf",
        };

        private static NativeUtilsNamespace.NativeUtils _nativeUtilsRef;

        private static NativeUtilsNamespace.NativeUtils NativeUtilsRef => _nativeUtilsRef ??= new NativeUtilsNamespace.NativeUtils();

        private static BitmapSource LoadBitmap(string filename, out bool isSRgb)
        {
            var bitmapDecoder = new BitmapDecoder(
                Factory,
                filename,
                DecodeOptions.CacheOnDemand
            );

            var bitmapFrameDecode = bitmapDecoder.GetFrame(0);
            var metaReader = bitmapFrameDecode.MetadataQueryReader;
            
            isSRgb = false;
            if (metaReader != null)
            {
                //var list = metaReader.QueryPaths;
                //var test = list.ToDictionary(item => item, item => metaReader.GetMetadataByName(item));

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
            }

            if (isSRgb)
            {
                Console.WriteLine(filename + " SRGB!");
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

        public static void ImportTexture(string assetName, string path, bool forceSRgb)
        {
            var bit = LoadBitmap(path, out var isSRgb);
            var stride = bit.Size.Width * 4;
            var data = new byte[bit.Size.Height * stride];
            bit.CopyPixels(data, stride);

            var asset = new Texture2DAsset
            {
                Name = assetName,
                ForceSRgb = isSRgb,
                Data = new TextureAssetData()
                {
                    Width = bit.Size.Width,
                    Height = bit.Size.Height,
                    ChannelsCount = ChannelsCountType.Four,
                    BytesPerChannel = BytesPerChannelType.One,
                    ColorSpace = (forceSRgb || isSRgb) ? ColorSpaceType.Gamma : ColorSpaceType.Linear,
                    Buffer = data,
                }
            };
            EditorAssetsManager.GetManager().CreateAssetFile(asset, true);
        }

        public static void ImportCubeTexture(string assetName, string path)
        {
            var splitName = path.Split('.');

            int h;
            var w = h = -1;

            var buffers = new List<byte[][]>();

            var isSRgb = false;
            for (var i = 0; i < 6; i++)
            {
                var p = splitName[0] + "_" + CubePostfixes[i] + "." + splitName[1];

                var bit = LoadBitmap(p, out isSRgb);
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

            var asset = new TextureCubeAsset
            {
                Name = assetName,
                Data = new TextureCubeAssetData
                {
                    Width = w,
                    Height = h,
                    ChannelsCount = 4,
                    BytesPerChannel = 1,
                    ColorSpace = isSRgb ? ColorSpaceType.Gamma : ColorSpaceType.Linear,
                    Buffer = buffers.ToArray(),
                }
            };
            EditorAssetsManager.GetManager().CreateAssetFile(asset, true);
        }

        public static float[] LoadHdrTexture(string path, out int width, out int height, out int pixelSize)
        {
            var imageData = NativeUtilsRef.load_hdr_image(path);

            var imageFloats = imageData.Data.ToArray();
            width = imageData.Width;
            height = imageData.Height;
            pixelSize = sizeof(float) * imageData.ChannelsCount;

            return imageFloats;
        }
    }
}
