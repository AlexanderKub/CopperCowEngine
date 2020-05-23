//#define MAPS_DEBUG

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.IO;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.Rendering.Loaders;
using ColorSpaceType = CopperCowEngine.Rendering.Loaders.ColorSpaceType;
using Device = SharpDX.Direct3D11.Device;

namespace CopperCowEngine.Rendering.D3D11.Editor.Loaders
{
    internal static class SaveToWicImage
    {
        #if MAPS_DEBUG
        private const string DebugDir = "_IBL_Prerender_Debug";

        private static SharpDX.WIC.ImagingFactory2 _factory;

        private static SharpDX.WIC.ImagingFactory2 Factory => _factory ??= new SharpDX.WIC.ImagingFactory2();
        #endif

        public static void Save(this Texture2D texture, DeviceContext context, Device device, string path, bool withMips = false)
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
                context.MapSubresource(
                    textureCopy,
                    0, 0,
                    MapMode.Read,
                    SharpDX.Direct3D11.MapFlags.None,
                    out var dataStream);

                if (textureCopy.Description.Format == Format.R16G16_Float)
                {

                    var asset2D = new Texture2DAsset
                    {
                        Name = path,
                        Data =
                        {
                            Width = textureCopy.Description.Width,
                            Height = textureCopy.Description.Height,
                            ColorSpace = ColorSpaceType.Linear,
                            ChannelsCount = ChannelsCountType.Two,
                            BytesPerChannel = BytesPerChannelType.Two,
                            Buffer = ReadFully(dataStream)
                        }
                    };
                    EditorAssetsManager.GetManager().CreateAssetFile(asset2D, true);
                    context.UnmapSubresource(textureCopy, 0);
                }
#if MAPS_DEBUG
                if (textureCopy.Description.Format == Format.R32G32B32A32_Float)
                {
                    var asset2D = new Texture2DAsset
                    {
                        Name = path,
                        Data =
                        {
                            Width = textureCopy.Description.Width,
                            Height = textureCopy.Description.Height,
                            ColorSpace = ColorSpaceType.Linear,
                            ChannelsCount = ChannelsCountType.Four,
                            BytesPerChannel = BytesPerChannelType.Four,
                            Buffer = ReadFully(dataStream)
                        }
                    };
                    EditorAssetsManager.GetManager().CreateAssetFile(asset2D);
                    context.UnmapSubresource(textureCopy, 0);
                }
                DebugSaveTexture($"{path}.png", 0, 0, Factory, textureCopy, context);
#endif
                textureCopy.Dispose();
                return;
            }
            var mipLevels = withMips ? textureCopy.Description.MipLevels : 1;
            mipLevels = mipLevels > 9 ? 9 : mipLevels;

            var asset = new TextureCubeAsset
            {
                Name = path,
                Data =
                {
                    Width = textureCopy.Description.Width,
                    Height = textureCopy.Description.Height,
                    ColorSpace = ColorSpaceType.Linear,
                    ChannelsCount = (int)ChannelsCountType.Four,
                    BytesPerChannel = (int)BytesPerChannelType.Two,//Four
                    MipLevels = mipLevels,
                    Buffer = new byte[6][][]
                }
            };

            for (var arraySlice = 0; arraySlice < 6; arraySlice++)
            {
                asset.Data.Buffer[arraySlice] = new byte[mipLevels][];
                for (var mip = 0; mip < mipLevels; mip++)
                {
                    var subresourceIndex = textureCopy.CalculateSubResourceIndex(mip, arraySlice, out var mipSize);
                    var dataBox = context.MapSubresource(
                        textureCopy,
                        subresourceIndex,
                        MapMode.Read,
                        SharpDX.Direct3D11.MapFlags.None,
                        out var dataStream);

                    var allMipBytes = ReadFully(dataStream);
                    dataStream.Dispose();

                    //var mipSize = (int)(asset.Data.Width * System.Math.Pow(0.5, mip));
                    var pitch = mipSize * asset.Data.ChannelsCount * asset.Data.BytesPerChannel;
                    var n = mipSize * pitch;

                    asset.Data.Buffer[arraySlice][mip] = new byte[n];

                    for (var j = 0; j < mipSize; j++) 
                    {
                        for (var k = 0; k < pitch; k++) 
                        {
                            asset.Data.Buffer[arraySlice][mip][j * pitch + k] = allMipBytes[j * dataBox.RowPitch + k];
                        }
                    }

                    // Dont work cause wrong dataBox.RowPitch on 4x4 mip levels issue.
                    //asset.Data.Buffer[arraySlice][mip] = allMipBytes;

                    context.UnmapSubresource(textureCopy, subresourceIndex);
                }
            }
            EditorAssetsManager.GetManager().CreateAssetFile(asset, true);
            
#if MAPS_DEBUG
            for (var mip = 0; mip < textureCopy.Description.MipLevels; mip++)
            {
                for (var i = 0; i < texture.Description.ArraySize; i++)
                {
                    DebugSaveTexture($"{path}_{i}_mip{mip}.png", i, mip, Factory, textureCopy, context);
                }
            }
#endif
            textureCopy.Dispose();
        }

        private static byte[] ReadFully(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
        
#if MAPS_DEBUG
        private static void DebugSaveTexture(string path, int arraySlice, int mipSlice, SharpDX.WIC.ImagingFactory factory, Texture2D textureCopy, DeviceContext context)
        {
            var dataBox = context.MapSubresource(
                textureCopy,
                mipSlice,
                arraySlice,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out var dataStream);

            var dataRectangle = new SharpDX.DataRectangle
            {
                DataPointer = dataStream.DataPointer,
                Pitch = dataBox.RowPitch
            };

            var pixelFormatGuid = textureCopy.Description.Format switch
            {
                Format.R16G16_Float => SharpDX.WIC.PixelFormat.Format32bppBGR101010,
                Format.R32G32B32A32_Float => SharpDX.WIC.PixelFormat.Format128bppRGBFloat,
                Format.R8G8B8A8_UNorm => SharpDX.WIC.PixelFormat.Format32bppBGRA,
                _ => SharpDX.WIC.PixelFormat.Format64bppRGBAHalf
            };
            
            var isSquare = textureCopy.Description.Width == textureCopy.Description.Height;
            var mipSize = (int)(textureCopy.Description.Width * System.Math.Pow(0.5, mipSlice));
            
            var bitmap = new SharpDX.WIC.Bitmap(
                factory,
                mipSize,
                isSquare ? mipSize : textureCopy.Description.Height,
                pixelFormatGuid,
                dataRectangle);

            Directory.CreateDirectory(DebugDir);
            using (var s = new FileStream($"{DebugDir}/{path}", FileMode.OpenOrCreate))
            {
                //CREATE
                s.Position = 0;
                using var bitmapEncoder = new SharpDX.WIC.PngBitmapEncoder(factory, s);
                using var bitmapFrameEncode = new SharpDX.WIC.BitmapFrameEncode(bitmapEncoder);
                bitmapFrameEncode.Initialize();
                bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                var pixelFormat = SharpDX.WIC.PixelFormat.FormatDontCare;
                bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                bitmapFrameEncode.WriteSource(bitmap);
                bitmapFrameEncode.Commit();
                bitmapEncoder.Commit();
            }
            bitmap.Dispose();
            context.UnmapSubresource(textureCopy, 0);
        }
#endif
        public static void DisposeFactory()
        {
#if MAPS_DEBUG
            _factory?.Dispose();
            _factory = null;
#endif
        }
    }
}
