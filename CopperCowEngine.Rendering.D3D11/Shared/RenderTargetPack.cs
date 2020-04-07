using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal class RenderTargetPack : IDisposable
    {
        private static Texture2DDescription _textureDescription = new Texture2DDescription()
        {
            MipLevels = 1,
            ArraySize = 1,
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        };

        private static ShaderResourceViewDescription _shaderResourceDescription = new ShaderResourceViewDescription()
        {
            Dimension = ShaderResourceViewDimension.Texture2D,
            Texture2D = new ShaderResourceViewDescription.Texture2DResource()
            {
                MipLevels = 1,
                MostDetailedMip = 0,
            }
        };

        private static RenderTargetViewDescription _renderTargetDescription = new RenderTargetViewDescription()
        {
            Dimension = RenderTargetViewDimension.Texture2D,
        };

        public Format TargetFormat { get; private set; }

        public Texture2D Map { get; private set; }

        public ShaderResourceView ResourceView { get; private set; }

        public RenderTargetView View { get; private set; }

        private readonly string _name;

        private readonly int _samplesCount;

        public RenderTargetPack(string name, int samples)
        {
            _name = name;
            _samplesCount = samples;
        }

        public void Create(SharpDX.Direct3D11.Device device, int width, int height, Format format)
        {
            Dispose();

            TargetFormat = format;

            _textureDescription.SampleDescription = new SampleDescription(_samplesCount, 0);
            _textureDescription.Format = format;
            _textureDescription.Width = width;
            _textureDescription.Height = height;

            Map = new Texture2D(device, _textureDescription)
            {
                DebugName = $"{_name}Map"
            };

            _shaderResourceDescription.Dimension = _samplesCount > 1 ?
                ShaderResourceViewDimension.Texture2DMultisampled :
                ShaderResourceViewDimension.Texture2D;
            _shaderResourceDescription.Format = format;

            ResourceView = new ShaderResourceView(device, Map, _shaderResourceDescription)
            {
                DebugName = $"{_name}SRV"
            };

            _renderTargetDescription.Dimension = _samplesCount > 1 ?
                RenderTargetViewDimension.Texture2DMultisampled :
                RenderTargetViewDimension.Texture2D;
            _renderTargetDescription.Format = format;

            View = new RenderTargetView(device, Map, _renderTargetDescription)
            {
                DebugName = $"{_name}RTV"
            };
        }

        public void Resize(SharpDX.Direct3D11.Device device, int width, int height)
        {
            Create(device, width, height, TargetFormat);
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
