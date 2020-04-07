using System;
using System.Windows.Forms;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

using SwapChain = SharpDX.DXGI.SwapChain;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace CopperCowEngine.Rendering.D3D11.Displays
{
    internal sealed class FormDisplay : Display
    {
        public Control Surface;

        public SwapChain SwapChainRef { get; private set; }

        private Texture2DDescription _zBufferTextureDescription;

        public FormDisplay(bool isDebugMode, int msaaLevel) : base(isDebugMode, msaaLevel) { }

        public override void InitDevice()
        {
            Width = Surface.ClientSize.Width;
            Height = Surface.ClientSize.Height;

            var swapDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                    Width, Height,
                    new Rational(60, 1),
                    Format.R8G8B8A8_UNorm//R8G8B8A8_UNorm
                ),
                IsWindowed = true,
                OutputHandle = Surface.Handle,
                SampleDescription = new SampleDescription(MSamplesCount, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            if (IsDebugMode)
            {
                deviceCreationFlags |= DeviceCreationFlags.Debug;
            }

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                deviceCreationFlags,
                //DeviceCreationFlags.None,
                swapDesc,
                out var device,
                out var swapChain
            );
            DeviceRef = device;
            DeviceRef.DebugName = "The Device";
            DeviceRef.ImmediateContext.DebugName = "The Context";

            CheckFeatures();

            SwapChainRef = swapChain;
            SwapChainRef.DebugName = "The SwapChain";

            // Ignore all windows events
            var factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            factory.MakeWindowAssociation(Surface.Handle, WindowAssociationFlags.IgnoreAll);

            _zBufferTextureDescription = new Texture2DDescription
            {
                Format = Format.R32_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription(MSamplesCount, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            Factory2D = new Factory(FactoryType.SingleThreaded, DebugLevel.Information);
            FactoryDWrite = new SharpDX.DirectWrite.Factory();
            RenderTarget2DProperties = new RenderTargetProperties(new PixelFormat(
                Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
        }

        public override void InitRenderTarget()
        {
            BackBuffer?.Dispose();
            RenderTargetViewRef?.Dispose();
            DepthStencilShaderResourceViewRef?.Dispose();
            ZBuffer?.Dispose();
            DepthStencilViewRef?.Dispose();
            RenderTarget2D?.Dispose();

            SwapChainRef.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            using (Surface surface = SwapChainRef.GetBackBuffer<Surface>(0))
            {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperties);
            }

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            BackBuffer = SwapChainRef.GetBackBuffer<Texture2D>(0);
            BackBuffer.DebugName = "BackBuffer";
            RenderTargetViewRef = new RenderTargetView(DeviceRef, BackBuffer)
            {
                DebugName = "BackBufferRenderTargetView"
            };

            _zBufferTextureDescription.Width = Width;
            _zBufferTextureDescription.Height = Height;
            ZBuffer = new Texture2D(DeviceRef, _zBufferTextureDescription)
            {
                DebugName = "ZBuffer"
            };

            DepthStencilViewRef = new DepthStencilView(DeviceRef, ZBuffer, new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                Dimension = MSamplesCount > 1
                    ? DepthStencilViewDimension.Texture2DMultisampled
                    : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            })
            {
                DebugName = "ZBufferDepthStencilView"
            };

            var shaderResourceViewDescription = new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float,
            };

            if (MSamplesCount > 1)
            {
                shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2DMultisampled;
                shaderResourceViewDescription.Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource();
            }
            else
            {
                shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2D;
                shaderResourceViewDescription.Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                };
            }

            DepthStencilShaderResourceViewRef =
                new ShaderResourceView(DeviceRef, ZBuffer, shaderResourceViewDescription)
                {
                    DebugName = "ZBufferDepthStencilSRV"
                };

            SetUpViewport();

            base.InitRenderTarget();
        }

        public override void Render(IntPtr resource, bool isNewSurface)
        {
            //SharpDX.Configuration.EnableObjectTracking = true;
            if (Surface.ClientSize.Width != Width || Surface.ClientSize.Height != Height)
            {
                Width = Surface.ClientSize.Width;
                Height = Surface.ClientSize.Height;
                InitRenderTarget();
                Resize();
            }
            base.Render(resource, isNewSurface);

            SwapChainRef.Present(0, PresentFlags.None);
        }

        public override void Cleanup()
        {
            DeviceDebug device3DDebug = null;
            if (IsDebugMode)
            {
                device3DDebug = new DeviceDebug(DeviceRef);
            }

            SwapChainRef?.Dispose();
            SwapChainRef = null;
            base.Cleanup();

            if (!IsDebugMode || device3DDebug == null)
            {
                return;
            }
            device3DDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
            System.Diagnostics.Debug.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
        }
    }
}
