using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.RenderTechnique
{
    public class SharedRenderItems
    {
        #region SamplerStates
        private static SamplerState m_LinearWrapSamplerState;
        public static SamplerState LinearWrapSamplerState {
            get {
                if (m_LinearWrapSamplerState == null)
                {
                    m_LinearWrapSamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        ComparisonFunction = Comparison.Never,
                        MaximumAnisotropy = 16,
                        MipLodBias = 0,
                        MinimumLod = -float.MaxValue,
                        MaximumLod = float.MaxValue
                    });
                }
                return m_LinearWrapSamplerState;
            }
        }

        private static SamplerState m_LinearClampSamplerState;
        public static SamplerState LinearClampSamplerState {
            get {
                if (m_LinearClampSamplerState == null)
                {
                    m_LinearClampSamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
                    {
                        Filter = Filter.MinMagMipLinear,
                        AddressU = TextureAddressMode.Clamp,
                        AddressV = TextureAddressMode.Clamp,
                        AddressW = TextureAddressMode.Clamp,
                        ComparisonFunction = Comparison.Never,
                        MaximumAnisotropy = 16,
                        MipLodBias = 0,
                        MinimumLod = -float.MaxValue,
                        MaximumLod = float.MaxValue
                    });
                }
                return m_LinearClampSamplerState;
            }
        }

        private static SamplerState m_AnisotropicWrapSamplerState;
        public static SamplerState AnisotropicWrapSamplerState {
            get {
                if (m_AnisotropicWrapSamplerState == null)
                {
                    m_AnisotropicWrapSamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
                    {
                        Filter = Filter.Anisotropic,
                        AddressU = TextureAddressMode.Wrap,
                        AddressV = TextureAddressMode.Wrap,
                        AddressW = TextureAddressMode.Wrap,
                        MaximumAnisotropy = 16,
                        ComparisonFunction = Comparison.Always,
                        MaximumLod = float.MaxValue,
                    });
                }
                return m_AnisotropicWrapSamplerState;
            }
        }
    #endregion
   

        public static void Dispose()
        {
            m_LinearWrapSamplerState?.Dispose();
            m_LinearClampSamplerState?.Dispose();
            m_AnisotropicWrapSamplerState?.Dispose();
        }
    }
}
