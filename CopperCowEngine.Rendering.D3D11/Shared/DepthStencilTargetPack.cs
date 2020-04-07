using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    public class DepthStencilTargetPack : IDisposable
    {
        private static Texture2DDescription _textureDescription = new Texture2DDescription()
        {
            Format = Format.R32G8X24_Typeless,
            ArraySize = 1,
            MipLevels = 1,
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None
        };

        private static ShaderResourceViewDescription _shaderResourceDescription = new ShaderResourceViewDescription()
        {
            Format = Format.R32_Float_X8X24_Typeless,
            Dimension = ShaderResourceViewDimension.Texture2D,
            Texture2D = new ShaderResourceViewDescription.Texture2DResource()
            {
                MostDetailedMip = 0,
                MipLevels = 1,
            },
        };

        private static DepthStencilViewDescription _depthStencilViewDescription = new DepthStencilViewDescription
        {
            Format = Format.D32_Float_S8X24_UInt,
            Dimension = DepthStencilViewDimension.Texture2D,
            Flags = DepthStencilViewFlags.None,
        };

        public Texture2D Map { get; private set; }

        public ShaderResourceView ResourceView { get; private set; }

        public DepthStencilView View { get; private set; }

        private readonly string _name;

        private readonly int _samplesCount;

        public DepthStencilTargetPack(string name, int samples)
        {
            _name = name;
            _samplesCount = samples;
        }

        public void Create(SharpDX.Direct3D11.Device device, int width, int height)
        {
            Dispose();

            _textureDescription.Width = width;
            _textureDescription.Height = height;
            //textureDescription.Format = Format.R32_Typeless;
            _textureDescription.SampleDescription = new SampleDescription(_samplesCount, 0);

            Map = new Texture2D(device, _textureDescription)
            {
                DebugName = _name + $"{_name}Map"
            };

            ResourceView?.Dispose();
            //shaderResourceDescription.Format = Format.R32_Float;
            _shaderResourceDescription.Dimension = _samplesCount > 1 ?
                ShaderResourceViewDimension.Texture2DMultisampled :
                ShaderResourceViewDimension.Texture2D;

            ResourceView = new ShaderResourceView(device, Map, _shaderResourceDescription)
            {
                DebugName = $"{_name}SRV"
            };

            View?.Dispose();
            //_depthStencilViewDescription.Format = Format.D32_Float;
            _depthStencilViewDescription.Dimension = _samplesCount > 1 ?
                DepthStencilViewDimension.Texture2DMultisampled :
                DepthStencilViewDimension.Texture2D;

            View = new DepthStencilView(device, Map, _depthStencilViewDescription)
            {
                DebugName = $"{_name}RTV"
            };
        }

        public void Resize(SharpDX.Direct3D11.Device device, int width, int height)
        {
            Create(device, width, height);
        }

        public void Dispose()
        {
            Map?.Dispose();
            Map = null;
            ResourceView?.Dispose();
            ResourceView = null;
            View?.Dispose();
            View = null;
        }
    }
}
