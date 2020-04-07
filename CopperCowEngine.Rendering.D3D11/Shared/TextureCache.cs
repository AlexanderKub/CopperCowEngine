using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.Rendering.D3D11.Loaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using ColorSpaceType = CopperCowEngine.Rendering.Loaders.ColorSpaceType;
using Resource = SharpDX.Direct3D11.Resource;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        private Texture2D DebugTexture => LoadTextureAsset("DebugTextureMap");

        private Texture2D DebugCubeTexture => LoadCubeTextureAsset("SkyboxCubeMap", out _);

        // SkyboxIrradianceCubeMap MiraSkyboxIrradianceCubeMap NightskyIrradianceCubeMap HouseIrradianceCubeMap
        public ShaderResourceView IrradianceMap => LoadTextureShaderResourceView("HouseIrradianceCubeMap", true);

        public ShaderResourceView PreFilteredMap => LoadTextureShaderResourceView("HousePreFilteredCubeMap", true);

        public ShaderResourceView BRDFxLookUpTable => LoadTextureShaderResourceView("StandardBRDFxLUT", false);

        private Dictionary<string, ShaderResourceView> _texturesCache;

        private Dictionary<string, Texture2D> _textures2DCache;

        private void InitTexturesCache()
        {
            _texturesCache = new Dictionary<string, ShaderResourceView>();
            _textures2DCache = new Dictionary<string, Texture2D>();
        }

        private void DisposeTexturesCache()
        {
            foreach (var srv in _texturesCache)
            {
                srv.Value?.Dispose();
            }
            _texturesCache.Clear();
            _texturesCache = null;

            foreach (var texture in _textures2DCache)
            {
                texture.Value?.Dispose();
            }
            _textures2DCache.Clear();
            _textures2DCache = null;
        }

        public Texture2D LoadTextureAsset(string assetName)
        {
            if (_textures2DCache.ContainsKey(assetName))
            {
                return _textures2DCache[assetName];
            }
            
            var asset = AssetsManager.GetManager().LoadAsset<Texture2DAsset>(assetName);
            if (asset.IsInvalid)
            {
                //Debug.Log("AssetManager", "Texture " + assetName + " couldn't be loaded.");
                return DebugTexture;
            }
            //Debug.Log("AssetManager", "Texture " + assetName + " loaded.");

            //var forceNoMips = false;

            var stride = asset.Data.Width * 4;
            var ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(asset.Data.Buffer, 0);
            var dataBox = new DataBox(ptr, stride, stride * asset.Data.Height);
            var texture2D = new Texture2D(
                _renderBackend.Device,
                new Texture2DDescription()
                {
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                    ArraySize = 1,
                    BindFlags = asset.Data.GetMips
                        ? (BindFlags.ShaderResource | BindFlags.RenderTarget)
                        : BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = asset.Data.GetFormat(),
                    MipLevels = asset.Data.GetMips ? 0 : 1,
                    OptionFlags = asset.Data.GetMips ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                }
            )
            {
                DebugName = assetName + "Texture"
            };
            _renderBackend.Device.ImmediateContext.UpdateSubresource(dataBox,
                texture2D, Resource.CalculateSubResourceIndex(0, 0, 0));

            _textures2DCache.Add(assetName, texture2D);

            return texture2D;
        }

        public Texture2D LoadCubeTextureAsset(string assetName, out TextureCubeAsset asset)
        {
            if (_textures2DCache.ContainsKey(assetName))
            {
                asset = null;
                return _textures2DCache[assetName];
            }

            asset = AssetsManager.GetManager().LoadAsset<TextureCubeAsset>(assetName);
            if (asset.IsInvalid)
            {
                //Debug.Log("AssetManager", "TextureCube " + assetName + " couldn't be loaded.");
                return DebugCubeTexture;
            }
            //Debug.Log("AssetManager", "TextureCube " + assetName + " loaded.");

            Format format;
            switch (asset.Data.BytesPerChannel)
            {
                case 2:
                    format = Format.R16G16B16A16_Float;
                    break;
                case 4:
                    format = Format.R32G32B32A32_Float;
                    break;
                default:
                    format = Format.R8G8B8A8_UNorm;
                    if (asset.Data.ColorSpace == ColorSpaceType.Gamma)
                    {
                        format = Format.R8G8B8A8_UNorm_SRgb;
                    }
                    break;
            }

            var stride = asset.Data.ChannelsCount * asset.Data.BytesPerChannel;
            var mipLevels = asset.Data.MipLevels;

            var initData = new DataBox[6 * mipLevels];

            for (var mip = 0; mip < mipLevels; mip++)
            {
                var mipSize = (int)(asset.Data.Width * Math.Pow(0.5, mip));
                for (var i = 0; i < 6; i++)
                {
                    var bts = asset.Data.Buffer[i][mip];
                    var ptr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(bts, 0);
                    var dataBox = new DataBox(ptr, stride * mipSize, stride * mipSize * mipSize);
                    initData[i * mipLevels + mip] = dataBox;
                }
            }

            var cubeTexture = new Texture2D(
                _renderBackend.Device,
                new Texture2DDescription()
                {
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                    ArraySize = 6,
                    Format = format,
                    BindFlags = BindFlags.ShaderResource,
                    OptionFlags = ResourceOptionFlags.TextureCube,
                    SampleDescription = new SampleDescription(1, 0),
                    MipLevels = asset.Data.MipLevels,
                    Usage = ResourceUsage.Immutable,
                    CpuAccessFlags = CpuAccessFlags.None,
                },
                initData
            )
            {
                DebugName = assetName + "Texture"
            };

            asset = null;
            _textures2DCache.Add(assetName, cubeTexture);

            return cubeTexture;
        }

        public ShaderResourceView LoadTextureShaderResourceView(string assetName)
        {
            return LoadTextureShaderResourceView(assetName, false);
        }

        public ShaderResourceView LoadTextureShaderResourceView(string assetName, bool isCubeMap)
        {
            if (_texturesCache.ContainsKey(assetName))
            {
                return _texturesCache[assetName];
            }
            // TODO: create some flag
            if (assetName.EndsWith("CubeMap"))
            {
                isCubeMap = true;
            }

            var texture = isCubeMap ? LoadCubeTextureAsset(assetName, out _) : LoadTextureAsset(assetName);

            if (texture == null) { return null; }

            var shaderResourceViewDescription = new ShaderResourceViewDescription
            {
                Format = texture.Description.Format
            };


            if (isCubeMap)
            {
                shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.TextureCube;
                shaderResourceViewDescription.TextureCube.MipLevels = texture.Description.MipLevels;
                shaderResourceViewDescription.TextureCube.MostDetailedMip = 0;
            }
            else
            {
                shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2D;
                shaderResourceViewDescription.Texture2D.MipLevels = texture.Description.MipLevels == 0 ? -1 : texture.Description.MipLevels;
                shaderResourceViewDescription.Texture2D.MostDetailedMip = 0;
            }

            ShaderResourceView result;
            if (isCubeMap)
            {
                result = new ShaderResourceView(_renderBackend.Device, texture, shaderResourceViewDescription);
            }
            else
            {
                result = new ShaderResourceView(_renderBackend.Device, texture);
                _renderBackend.Context.GenerateMips(result);
            }
            result.DebugName = assetName + "SRV";

            _texturesCache.Add(assetName, result);
            return result;
        }

        public void DropCachedTexture(string assetName)
        {
            if (!_textures2DCache.ContainsKey(assetName))
            {
                return;
            }
            _textures2DCache[assetName].Dispose();
            _textures2DCache.Remove(assetName);

            if (!_texturesCache.ContainsKey(assetName))
            {
                return;
            }
            _texturesCache[assetName].Dispose();
            _texturesCache.Remove(assetName);
            //Debug.Log("AssetManager", "Texture " + assetName + " unloaded.");
        }
    }
}
