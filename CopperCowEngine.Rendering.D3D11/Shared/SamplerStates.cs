using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal enum SamplerStateType
    {
        PointClamp,
        PointWrap,

        BilinearClamp,
        BilinearWrap,

        TrilinearClamp,
        TrilinearWrap,

        AnisotropicWrap,
        ShadowMap,
        PreIntegratedSampler,
        // What the sampler (Who?)
        IBLSampler,
    }

    internal partial class SharedRenderItemsStorage
    {
        private SamplerState[] _samplersArray;

        public SamplerState GetSamplerState(SamplerStateType stateType)
        {
            return GetSamplerState((int)stateType);
        }

        public SamplerState GetSamplerState(int stateIndex)
        {
            return _samplersArray[stateIndex];
        }

        // Point Texture Filtering - D3D11_FILTER_MIN_MAG_MIP_POINT - Filter.MinMagMipPoint
        // Bilinear Texture Filtering - D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT - Filter.MinMagLinearMipPoint
        // Trilinear Texture Filtering - D3D11_FILTER_MIN_MAG_MIP_LINEAR - Filter.MinMagMipLinear
        // Anisotropic Texture Filtering - D3D11_FILTER_ANISOTROPIC - Filter.Anisotropic

        private void InitSamplers()
        {
            _samplersArray = new SamplerState[Enum.GetNames(typeof(SamplerStateType)).Length];

            _samplersArray[(int)SamplerStateType.PointClamp] = new SamplerState(_renderBackend.Device,
                new SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter = Filter.MinMagMipPoint,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "PointClampSampler" };

            _samplersArray[(int)SamplerStateType.PointWrap] = new SamplerState(_renderBackend.Device,
                new SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.MinMagMipPoint,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "PointWrapSampler" };

            _samplersArray[(int)SamplerStateType.BilinearClamp] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    Filter = Filter.MinMagLinearMipPoint,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Never,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "BilinearClampSampler" };

            _samplersArray[(int)SamplerStateType.BilinearWrap] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    Filter = Filter.MinMagLinearMipPoint,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "BilinearWrapSampler" };

            _samplersArray[(int)SamplerStateType.TrilinearClamp] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Never,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "TrilinearClampSampler" };

            _samplersArray[(int)SamplerStateType.TrilinearWrap] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                })
            { DebugName = "TrilinearWrapSampler" };

            _samplersArray[(int)SamplerStateType.AnisotropicWrap] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    Filter = Filter.Anisotropic,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MaximumAnisotropy = 16,
                    ComparisonFunction = Comparison.Always,
                    MaximumLod = float.MaxValue,
                })
            { DebugName = "AnisotropicWrapSampler" };

            _samplersArray[(int)SamplerStateType.ShadowMap] = new SamplerState(_renderBackend.Device,
                new SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Border,
                    AddressV = TextureAddressMode.Border,
                    AddressW = TextureAddressMode.Border,
                    BorderColor = Color.Black,
                    Filter = Filter.ComparisonMinMagMipLinear,
                    ComparisonFunction = Comparison.Less,
                })
            { DebugName = "ShadowMapSampler" };

            _samplersArray[(int)SamplerStateType.PreIntegratedSampler] = new SamplerState(
                _renderBackend.Device, new SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    //Filter = Filter.MinMagLinearMipPoint,
                    Filter = Filter.MinMagMipPoint, // Point
                })
            { DebugName = "PreIntegratedSampler" };

            _samplersArray[(int) SamplerStateType.IBLSampler] = new SamplerState(_renderBackend.Device,
                new SamplerStateDescription()
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter = Filter.MinMagLinearMipPoint, // Bilinear
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0.0f,
                }) {DebugName = "IBLSampler"};
        }

        private void DisposeSamplers()
        {
            for (var i = 0; i < _samplersArray.Length; i++)
            {
                _samplersArray[i]?.Dispose();
                _samplersArray[i] = null;
            }
            _samplersArray = null;
        }
    }
}
