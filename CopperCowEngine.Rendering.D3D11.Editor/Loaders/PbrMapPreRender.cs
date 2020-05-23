using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Threading.Tasks;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using Color = SharpDX.Color;
using Matrix = SharpDX.Matrix;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Rendering.D3D11.Editor.Loaders
{
    /// <summary>
    /// Class for converting Equirectangular Environment Map from .hdr format
    /// to TextureCubeAsset and calculate PreFiltered and Irradiance maps from it.
    /// </summary>
    internal class PbrMapPreRender
    {
        private const string EquirecToCubeComputeShader = "PBR_IBL_EquirecToCubeCS";
        private const string PrefilteredComputeShader = "PBR_IBL_PreFilteredCS";
        private const string IrradianceComputeShader = "PBR_IBL_IrradianceCS";
        private const string BrdfComputeShader = "PBR_IBL_BrdfCS";
        
        private Device _device;
        private Device GetDevice => _device ??= InitDevice();
        private DeviceContext GetContext => GetDevice?.ImmediateContext;

        private Texture2D _convertedCubeMap;
        private Texture2D _enviromentMap;
        private Texture2D _irradianceMap;
        private UnorderedAccessView _convertedCubeUav;
        private ShaderResourceView _convertedCubeSrv;
        
        public void BakeEnviromentMap(string sourcePath, string path, int outputResolution)
        {
            if (GetContext == null)
            {
                return;
            }

            // Convert equirectangular to cube
            var inputMapSrv = ImportTexture(sourcePath);
            _convertedCubeMap = CreateCubeMap(outputResolution, Format.R16G16B16A16_Float);
            _convertedCubeMap.DebugName = "ConvertedCubeMap";
            _convertedCubeSrv = CreateSrvFrom(_convertedCubeMap);
            _convertedCubeUav = CreateUavFrom(_convertedCubeMap, 0);

            var computeShader = ImportComputeShader(EquirecToCubeComputeShader);
            var computeSampler = CreateSamplerState(Filter.MinMagMipLinear, TextureAddressMode.Wrap);

            GetContext.ComputeShader.Set(computeShader);
            GetContext.ComputeShader.SetSampler(0, computeSampler);
            GetContext.ComputeShader.SetShaderResource(0, inputMapSrv);
            GetContext.ComputeShader.SetUnorderedAccessView(0, _convertedCubeUav);
            GetContext.Dispatch(outputResolution / 32, outputResolution / 32, 6);
            GetContext.ComputeShader.SetShaderResource(0, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);

            GetContext.GenerateMips(_convertedCubeSrv);

            // Prefiltered
            computeShader = ImportComputeShader(PrefilteredComputeShader);
            var preFilteredConstantBuffer = CreateConstantBuffer<SpecularMapFilterConstantValue>();
            _enviromentMap = CreateCubeMap(outputResolution, Format.R16G16B16A16_Float);
            var mipLevels = _enviromentMap.Description.MipLevels;

			for (var arraySlice = 0; arraySlice < 6; arraySlice ++) 
            {
                var subresourceIndex = SharpDX.Direct3D11.Resource.CalculateSubResourceIndex(0, arraySlice, mipLevels);
                GetContext.CopySubresourceRegion(_convertedCubeMap, subresourceIndex, null, _enviromentMap, subresourceIndex);
			}

            GetContext.ComputeShader.Set(computeShader);
            GetContext.ComputeShader.SetSampler(0, computeSampler);
            GetContext.ComputeShader.SetShaderResource(0, _convertedCubeSrv);
            GetContext.ComputeShader.SetConstantBuffer(0, preFilteredConstantBuffer);

            var deltaRoughness = 1.0f / MathF.Max(mipLevels - 1, 1);
            for (int level = 1, size = 512; level < mipLevels; level++, size /= 2)
            {
				var uav = CreateUavFrom(_enviromentMap, level);
                GetContext.ComputeShader.SetUnorderedAccessView(0, uav);

                var data = new SpecularMapFilterConstantValue
                {
                    Roughness = level * deltaRoughness,
                };
                GetContext.UpdateSubresource(ref data, preFilteredConstantBuffer);

                var numGroups = (int)MathF.Max(1, size / 32);
                GetContext.Dispatch(numGroups, numGroups, 6);
            }

            GetContext.ComputeShader.SetConstantBuffer(0, null);
            GetContext.ComputeShader.SetShaderResource(0, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);
            _enviromentMap.Save(GetContext, GetDevice, $"{path}PreFilteredEnvMap", true);

            // Irradiance
            var irradianceMapSize = 32;
            computeShader = ImportComputeShader(IrradianceComputeShader);
            _irradianceMap = CreateCubeMap(irradianceMapSize, Format.R16G16B16A16_Float, 1);
            var irradianceMapUav = CreateUavFrom(_irradianceMap, 0);
            GetContext.ComputeShader.Set(computeShader);
            GetContext.ComputeShader.SetSampler(0, computeSampler);
            GetContext.ComputeShader.SetShaderResource(0, _convertedCubeSrv);
            GetContext.ComputeShader.SetUnorderedAccessView(0, irradianceMapUav);
		    GetContext.Dispatch(irradianceMapSize / 32, irradianceMapSize / 32, 6);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);
            GetContext.ComputeShader.SetShaderResource(0, null);
            _irradianceMap.Save(GetContext, GetDevice, $"{path}IrradianceEnvMap");
        }

        public void RenderBrdf(string outputName)
        {
            if (GetContext == null)
            {
                return;
            }

            var assetsManager = AssetsManager.GetManager();
            var shaderAsset = assetsManager.LoadAsset<ShaderAsset>(BrdfComputeShader);
            var shaderBytecode = new ShaderBytecode(shaderAsset.Bytecode);
            var brdfComputeShader = ToDispose(new ComputeShader(_device, shaderBytecode));

            var mapSize = 256;
            var brdfMap = ToDispose(new Texture2D(_device, new Texture2DDescription()
            {
                Width = mapSize,
                Height = mapSize,
                Format = Format.R16G16_Float,
                ArraySize = 1,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.UnorderedAccess,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
            }));
            var brdfUAV = CreateUavFrom(brdfMap, 0);
            var brdfSampler = CreateSamplerState(Filter.MinMagMipLinear, TextureAddressMode.Clamp);

            GetContext.ComputeShader.Set(brdfComputeShader);
            GetContext.ComputeShader.SetSampler(0, brdfSampler);
            GetContext.ComputeShader.SetUnorderedAccessView(0, brdfUAV);
            GetContext.Dispatch(mapSize / 32, mapSize / 32, 1);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);
            brdfMap.Save(GetContext, _device, outputName);
        }

        private Device InitDevice()
        {
            var driverTypes = new[] {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference,
            };

            var deviceCreationFlags = 
                DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

            var levels = new[] {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
            };

            foreach (var driverType in driverTypes)
            {
                _device = new Device(driverType, deviceCreationFlags, levels);
                if (_device == null)
                {
                    continue;
                }
                _device.DebugName = "The Device";
                break;
            }

            if (_device == null)
            {
                Console.WriteLine("Device not created!");
                return null;
            }
            return _device;
        }

        private ShaderResourceView ImportTexture(string sourcePath)
        {
            var initData = GetHdrTextureData(sourcePath, out var inputWidth, out var inputHeight);
            var inputTextureDesc = new Texture2DDescription()
            {
                Format = Format.R32G32B32A32_Float,
                Width = inputWidth,
                Height = inputHeight,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };
            var inputMap = ToDispose(new Texture2D(_device, inputTextureDesc, initData) 
            {
                DebugName = "InputMap",
            });
            
            var srvDescription = new ShaderResourceViewDescription
            {
                Format = inputTextureDesc.Format,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = { MipLevels = inputTextureDesc.MipLevels, MostDetailedMip = 0 },
            };
            return ToDispose(new ShaderResourceView(_device, inputMap, srvDescription) 
            {
                DebugName = "InputMap_SRV",
            });
        }
        
        private ComputeShader ImportComputeShader(string name)
        {
            var assetsManager = AssetsManager.GetManager();
            var shaderAsset = assetsManager.LoadAsset<ShaderAsset>(name);
            var shaderBytecode = new ShaderBytecode(shaderAsset.Bytecode);

            return ToDispose(new ComputeShader(_device, shaderBytecode) 
            {
                DebugName = name,
            });
        }
        
        private Buffer CreateConstantBuffer<T>() where T : struct
        {
            var data = new T();
            var description = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Marshal.SizeOf<T>(),
                BindFlags = BindFlags.ConstantBuffer,
            };

            return ToDispose(Buffer.Create<T>(GetDevice, ref data, description));
        }

        private Buffer CreateConstantBuffer<T>(ref T data) where T : struct
        {
            var description = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Marshal.SizeOf<T>(),
                BindFlags = BindFlags.ConstantBuffer,
            };

            return ToDispose(Buffer.Create<T>(GetDevice, ref data, description));
        }

        private Texture2D CreateCubeMap(int width, Format format, int mipLevels = 0)
        {
            return CreateCubeMap(width, width, format, mipLevels);
        }

        private Texture2D CreateCubeMap(int width, int height, Format format, int mipLevels = 0)
        {
            var description = new Texture2DDescription()
            {
                Width = width,
                Height = height,
                ArraySize = 6,
                MipLevels = mipLevels,
                Format = format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                OptionFlags = ResourceOptionFlags.TextureCube,
                CpuAccessFlags = CpuAccessFlags.Read,
            };
            if (mipLevels == 0) 
            {
		        description.BindFlags |= BindFlags.RenderTarget;
		        description.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
	        }

            return ToDispose(new Texture2D(GetDevice, description));
        }

        private UnorderedAccessView CreateUavFrom(Texture2D texture, int mipSlice)
        {
            var textureDesc = texture.Description;
            var isArray = textureDesc.ArraySize > 1;

            var description = new UnorderedAccessViewDescription
            {
                Dimension = isArray
                    ? UnorderedAccessViewDimension.Texture2DArray 
                    : UnorderedAccessViewDimension.Texture2D,
                Format = textureDesc.Format,
            };

            if (isArray)
            {
                description.Texture2DArray = new UnorderedAccessViewDescription.Texture2DArrayResource
                {
                    ArraySize = textureDesc.ArraySize,
                    MipSlice = mipSlice,
                    FirstArraySlice = 0,
                };
            }
            else
            {
                description.Texture2D = new UnorderedAccessViewDescription.Texture2DResource
                {
                    MipSlice = mipSlice,
                };
            }

            return ToDispose(new UnorderedAccessView(GetDevice, texture, description)
            {
                DebugName = $"{texture.DebugName}_UAV",
            });
        }

        private ShaderResourceView CreateSrvFrom(Texture2D texture)
        {
            var textureDesc = texture.Description;
            var isArray = textureDesc.ArraySize > 1;

            var description = new ShaderResourceViewDescription
            {
                Dimension = isArray
                    ? ShaderResourceViewDimension.Texture2DArray 
                    : ShaderResourceViewDimension.Texture2D,
                Format = textureDesc.Format,
            };

            if (isArray)
            {
                description.Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                {
                    ArraySize = textureDesc.ArraySize,
                    FirstArraySlice = 0,
                    MostDetailedMip = 0,
                    MipLevels = -1,
                };
            }
            else
            {
                description.Texture2D = new ShaderResourceViewDescription.Texture2DResource
                {
                    MipLevels = -1,
                    MostDetailedMip = 0,
                };
            }

            return ToDispose(new ShaderResourceView(GetDevice, texture, description)
            {
                DebugName = $"{texture.DebugName}_SRV",
            });
        }

        private SamplerState CreateSamplerState(Filter filter, TextureAddressMode addressMode)
        {
            var description = new SamplerStateDescription
            {
                Filter = filter,
                AddressU = addressMode,
                AddressV = addressMode,
                AddressW = addressMode,
                MaximumAnisotropy = (filter == Filter.Anisotropic) ? 16 : 1,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = float.MaxValue,
            };

            return ToDispose(new SamplerState(GetDevice, description)
            {
                DebugName = "ComputeSampler",
            });
        }
        
        private static DataBox[] GetHdrTextureData(string sourcePath, out int inputWidth, out int inputHeight)
        {
            var imageFloats = TextureImporter.LoadHdrTexture(sourcePath, out inputWidth, out inputHeight, out var pixelSize);
            
            if (inputWidth == 0)
            {
                Console.WriteLine("Texture not loaded!");
                return null;
            }

            var n = imageFloats.Length + inputWidth * inputHeight;
            var imageFloatsCorrected = new float[n];
            var j = 0;
            for (var i = 0; i < imageFloats.Length; i+= 3)
            {
                imageFloatsCorrected[j] = imageFloats[i];
                imageFloatsCorrected[j + 1] = imageFloats[i + 1];
                imageFloatsCorrected[j + 2] = imageFloats[i + 2];
                imageFloatsCorrected[j + 3] = 1f;
                j += 4;
            }

            var pSrcBits = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(imageFloatsCorrected, 0);
            var initData = new DataBox[1];
            initData[0].DataPointer = pSrcBits;
            initData[0].RowPitch = inputWidth * (pixelSize + 4);

            return initData;
        }

        #region Dispose section

        private List<IDisposable> _toDisposeList = new List<IDisposable>();

        private T ToDispose<T>(T obj) where T : IDisposable
        {
            _toDisposeList.Add(obj);
            return obj;
        }

        public void Dispose()
        {
            _toDisposeList.Reverse();
            foreach (var item in _toDisposeList)
            {
                item?.Dispose();
            }
            _toDisposeList.Clear();
            _toDisposeList = null;

            GetContext.ClearState();
            GetContext.Flush();
            GetContext.Dispose();
            _device?.Dispose();
            SaveToWicImage.DisposeFactory();
        }
        #endregion
    }

    struct SpecularMapFilterConstantValue
	{
		public float Roughness;
		public Vector3 Padding;
	};
}
