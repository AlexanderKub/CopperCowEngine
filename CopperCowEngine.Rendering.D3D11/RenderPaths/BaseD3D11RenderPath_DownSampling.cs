using System;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.D3D11.Utils;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal abstract partial class BaseD3D11RenderPath
    {
        private const int ScaleFactor = 4;

        private const string DownsamplingSecondPassComputeShaderName = "DownsamplingSecondCS";
        private const string BloomBrightPassComputeShaderName = "BloomCS";
        private const string BloomFilterVerticalPassComputeShaderName = "BloomVerticalFilterCS";
        private const string BloomFilterHorizontalComputeShaderName = "BloomHorizontalFilterCS";

        private string DownSamplingFirstComputeShaderName
        {
            get
            {
                var blur = (BloomEnable || DofBlurEnable);

                return MsaaEnable switch
                {
                    false => (blur switch
                    {
                        false => "DownsamplingFirstCS",
                        true => "DownsamplingWithBlurFirstCS",
                    }),
                    true => (blur switch
                    {
                        false => "DownsamplingFirstMsaaCS",
                        true => "DownsamplingWithBlurFirstMsaaCS"
                    })
                };
            }
        }
        
        protected ShaderResourceView BloomResultSrv;
        protected ShaderResourceView DownScaledHdrSrv;
        protected ShaderResourceView CurrentAvgLuminanceSrv => 
            _dwnSampledPrev ? _csAvgLuminanceSrv : _csPrevAvgLuminanceSrv;
        
        private bool _dwnSampledPrev;
        private ShaderResourceView _csAvgLuminanceSrv;
        private ShaderResourceView _csPrevAvgLuminanceSrv;

        private Buffer _csLuminanceBuffer;
        private UnorderedAccessView _csLuminanceUav;
        private ShaderResourceView _csLuminanceSrv;
        private Buffer _csAvgLuminanceBuffer;
        private UnorderedAccessView _csAvgLuminanceUav;
        private Buffer _csPrevAvgLuminanceBuffer;
        private UnorderedAccessView _csPrevAvgLuminanceUav;
        private Buffer _downScaleConstantsBuffer;

        private Texture2D _downScaledHdr;
        private UnorderedAccessView _downScaledHdrUav;
        private Texture2D _bloomTemporaryHdr;
        private ShaderResourceView _bloomTemporarySrv;
        private UnorderedAccessView _bloomTemporaryUav;
        private Texture2D _bloomResultHdr;
        private UnorderedAccessView _bloomResultUav;

        private int _scaledWidth;
        private int _scaledHeight;

        protected void InitDownsamplingResources()
        {
            InitDownsamplingSizableResources();

            // Current
            _csAvgLuminanceBuffer?.Dispose();
            ToDispose(_csAvgLuminanceBuffer = new Buffer(GetDevice, new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4,
            }));
            _csAvgLuminanceBuffer.DebugName = "CSAvgLuminanceBuffer";

            _csAvgLuminanceUav?.Dispose();
            ToDispose(_csAvgLuminanceUav = new UnorderedAccessView(GetDevice, _csAvgLuminanceBuffer, new UnorderedAccessViewDescription
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            _csAvgLuminanceUav.DebugName = "CSAvgLuminanceUAV";

            _csAvgLuminanceSrv?.Dispose();
            ToDispose(_csAvgLuminanceSrv = new ShaderResourceView(GetDevice, _csAvgLuminanceBuffer, new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            _csAvgLuminanceSrv.DebugName = "CSAvgLuminanceSRV";

            // Prev
            _csPrevAvgLuminanceBuffer?.Dispose();
            ToDispose(_csPrevAvgLuminanceBuffer = new Buffer(GetDevice, new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4,
            }));
            _csPrevAvgLuminanceBuffer.DebugName = "CSPrevAvgLuminanceBuffer";

            _csPrevAvgLuminanceUav?.Dispose();
            ToDispose(_csPrevAvgLuminanceUav = new UnorderedAccessView(GetDevice, _csPrevAvgLuminanceBuffer, new UnorderedAccessViewDescription
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            _csPrevAvgLuminanceUav.DebugName = "CSPrevAvgLuminanceUAV";

            _csPrevAvgLuminanceSrv?.Dispose();
            ToDispose(_csPrevAvgLuminanceSrv = new ShaderResourceView(GetDevice, _csPrevAvgLuminanceBuffer, new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = 1,
                }
            }));
            _csPrevAvgLuminanceSrv.DebugName = "CSPrevAvgLuminanceSRV";
            
            _downScaleConstantsBuffer?.Dispose();
            ToDispose(_downScaleConstantsBuffer = new Buffer(GetDevice, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<BrandNewCommonStructs.DownScaleConstStruct>(),
            }));
            _downScaleConstantsBuffer.DebugName = "DownScaleConstantsBuffer";

            GetContext.ComputeShader.SetConstantBuffer(0, _downScaleConstantsBuffer);
        }
        
        protected void InitDownsamplingSizableResources()
        {
            _scaledWidth = (int)Math.Ceiling((double)GetDisplay.Width / ScaleFactor);
            _scaledHeight = (int)Math.Ceiling((double)GetDisplay.Height / ScaleFactor);

            var elementsCount = (GetDisplay.Width * GetDisplay.Height) / (16 * 1024);
            elementsCount = elementsCount > 0 ? elementsCount : 1;
            
            _csLuminanceBuffer?.Dispose();
            ToDispose(_csLuminanceBuffer = new Buffer(GetDevice, new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = 4,
                SizeInBytes = 4 * elementsCount,
            }));
            _csLuminanceBuffer.DebugName = "CSLuminanceBuffer";

            _csLuminanceUav?.Dispose();
            ToDispose(_csLuminanceUav = new UnorderedAccessView(GetDevice, _csLuminanceBuffer, new UnorderedAccessViewDescription
            {
                Format = Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new UnorderedAccessViewDescription.BufferResource()
                {
                    ElementCount = elementsCount,
                }
            }));
            _csLuminanceUav.DebugName = "CSLuminanceUAV";

            _csLuminanceSrv?.Dispose();
            ToDispose(_csLuminanceSrv = new ShaderResourceView(GetDevice, _csLuminanceBuffer, new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Buffer = new ShaderResourceViewDescription.BufferResource()
                {
                    ElementCount = elementsCount,
                }
            }));
            _csLuminanceSrv.DebugName = "CSLuminanceSRV";


            if (BloomEnable)
            {
                InitBloomResources();
            }
        }

        private void InitBloomResources()
        {
            var textureDesc = new Texture2DDescription
            {
                Width = _scaledWidth,
                Height = _scaledHeight,
                Format = Format.R16G16B16A16_Float,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                MipLevels = 1,
                ArraySize = 1,
            };
            var uavDesc = new UnorderedAccessViewDescription
            {
                Format = Format.R16G16B16A16_Float,
                Dimension = UnorderedAccessViewDimension.Texture2D,
                Texture2D = new UnorderedAccessViewDescription.Texture2DResource
                {
                    MipSlice = 0,
                }
            };
            var srvDesc = new ShaderResourceViewDescription
            {
                Format = Format.Unknown,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MipLevels = 1,
                    MostDetailedMip = 0,
                }
            };

            _downScaledHdr?.Dispose();
            ToDispose(_downScaledHdr = new Texture2D(GetDevice, textureDesc));
            _downScaledHdr.DebugName = "DownScaledHdrTexture";

            _downScaledHdrUav?.Dispose();
            ToDispose(_downScaledHdrUav = new UnorderedAccessView(GetDevice, _downScaledHdr, uavDesc));
            _downScaledHdrUav.DebugName = "DownScaledHdrUAV";

            DownScaledHdrSrv?.Dispose();
            ToDispose(DownScaledHdrSrv = new ShaderResourceView(GetDevice, _downScaledHdr, srvDesc));
            DownScaledHdrSrv.DebugName = "DownScaledHdrSRV";
            
            _bloomTemporaryHdr?.Dispose();
            ToDispose(_bloomTemporaryHdr = new Texture2D(GetDevice, textureDesc));
            _bloomTemporaryHdr.DebugName = "BloomTemporaryHdrTexture";

            _bloomTemporaryUav?.Dispose();
            ToDispose(_bloomTemporaryUav = new UnorderedAccessView(GetDevice, _bloomTemporaryHdr, uavDesc));
            _bloomTemporaryUav.DebugName = "BloomTemporaryHdrUAV";

            _bloomTemporarySrv?.Dispose();
            ToDispose(_bloomTemporarySrv = new ShaderResourceView(GetDevice, _bloomTemporaryHdr, srvDesc));
            _bloomTemporarySrv.DebugName = "BloomTemporaryHdrSRV";
            
            _bloomResultHdr?.Dispose();
            ToDispose(_bloomResultHdr = new Texture2D(GetDevice, textureDesc));
            _bloomResultHdr.DebugName = "BloomResultHdrTexture";

            _bloomResultUav?.Dispose();
            ToDispose(_bloomResultUav = new UnorderedAccessView(GetDevice, _bloomResultHdr, uavDesc));
            _bloomResultUav.DebugName = "BloomResultHdrUAV";

            BloomResultSrv?.Dispose();
            ToDispose(BloomResultSrv = new ShaderResourceView(GetDevice, _bloomResultHdr, srvDesc));
            BloomResultSrv.DebugName = "BloomResultHdrSRV";
        }

        protected void DownSampling(RenderTargetPack target, float frameTime)
        {
            var constData = new BrandNewCommonStructs.DownScaleConstStruct
            {
                ResX = (uint)(_scaledWidth),
                ResY = (uint)(_scaledHeight),
                Domain = (uint)((GetDisplay.Width * GetDisplay.Height) / 16),
                GroupSize = (uint)((GetDisplay.Width * GetDisplay.Height) / (16 * 1024)),
                AdaptationGreater = frameTime,
                AdaptationLower = 1 - MathF.Exp(-frameTime * 3f),
                BloomThreshold = 1 - MathF.Exp(-frameTime * 1f),
            };
            D3DUtils.WriteToDynamicBuffer(GetContext, _downScaleConstantsBuffer, constData);

            SetComputeShader(DownSamplingFirstComputeShaderName);

            GetContext.ComputeShader.SetShaderResource(0, target.ResourceView);
            GetContext.ComputeShader.SetUnorderedAccessView(0, _csLuminanceUav);
            GetContext.ComputeShader.SetUnorderedAccessView(1, _downScaledHdrUav);
            GetContext.ComputeShader.SetShaderResource(1, _dwnSampledPrev ? _csAvgLuminanceSrv : _csPrevAvgLuminanceSrv);

            var groupCountX = (GetDisplay.Width * GetDisplay.Height) / (16 * 1024);
            GetContext.Dispatch(groupCountX, 1, 1);
            GetContext.ComputeShader.SetUnorderedAccessView(1, null);

            GetContext.ComputeShader.SetUnorderedAccessView(0,
                _dwnSampledPrev ? _csPrevAvgLuminanceUav : _csAvgLuminanceUav);
            GetContext.ComputeShader.SetShaderResource(2, _csLuminanceSrv);
            SetComputeShader(DownsamplingSecondPassComputeShaderName);

            GetContext.Dispatch(1, 1, 1);
            GetContext.ComputeShader.SetShaderResource(2, null);

            if (BloomEnable)
            {
                Bloom(groupCountX);
            }
            
            GetContext.ComputeShader.SetShaderResource(0, null);
            GetContext.ComputeShader.SetShaderResource(1, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, null);

            _dwnSampledPrev = !_dwnSampledPrev;
        }

        private void Bloom(int groupCountX)
        {
            SetComputeShader(BloomBrightPassComputeShaderName);
            GetContext.ComputeShader.SetShaderResource(0, DownScaledHdrSrv);
            GetContext.ComputeShader.SetUnorderedAccessView(0, _bloomResultUav);
            GetContext.Dispatch(groupCountX, 1, 1);

            SetComputeShader(BloomFilterVerticalPassComputeShaderName);
            GetContext.ComputeShader.SetUnorderedAccessView(0, _bloomTemporaryUav);
            GetContext.ComputeShader.SetShaderResource(0, BloomResultSrv);
            var groupCount = (int) Math.Ceiling((double) _scaledHeight / (128 - 12) + 1);
            GetContext.Dispatch(_scaledWidth, groupCount, 1);
            
            GetContext.ComputeShader.SetShaderResource(0, null);
            GetContext.ComputeShader.SetUnorderedAccessView(0, _bloomResultUav);
            GetContext.ComputeShader.SetShaderResource(0, _bloomTemporarySrv);
            SetComputeShader(BloomFilterHorizontalComputeShaderName);
            groupCount = (int) Math.Ceiling((double)_scaledWidth / (128 - 12));
            GetContext.Dispatch(groupCount, _scaledHeight, 1);
        }
    }
}
