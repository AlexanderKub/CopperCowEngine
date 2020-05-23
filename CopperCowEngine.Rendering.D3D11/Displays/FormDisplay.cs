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

        private const Format BackBufferFormat = Format.R8G8B8A8_UNorm_SRgb; //Format.R8G8B8A8_UNorm;

        public SwapChain SwapChainRef { get; private set; }

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
                    BackBufferFormat
                ),
                IsWindowed = true,
                OutputHandle = Surface.Handle,
                SampleDescription = new SampleDescription(SamplesCount, 0),
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

            Factory2D = new Factory(FactoryType.SingleThreaded, DebugLevel.Information);
            FactoryDWrite = new SharpDX.DirectWrite.Factory();
            RenderTarget2DProperties = new RenderTargetProperties(new PixelFormat(
                BackBufferFormat, AlphaMode.Premultiplied));
        }

        public override void InitRenderTarget()
        {
            BackBuffer?.Dispose();
            RenderTarget?.Dispose();
            RenderTarget2D?.Dispose();

            SwapChainRef.ResizeBuffers(1, Width, Height, BackBufferFormat, SwapChainFlags.None);

            using (Surface surface = SwapChainRef.GetBackBuffer<Surface>(0))
            {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperties);
            }

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            BackBuffer = SwapChainRef.GetBackBuffer<Texture2D>(0);
            BackBuffer.DebugName = "BackBuffer";
            RenderTarget = new RenderTargetView(DeviceRef, BackBuffer)
            {
                DebugName = "BackBufferRenderTargetView"
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

        public override void Dispose()
        {
            DeviceDebug device3DDebug = null;
            if (IsDebugMode)
            {
                device3DDebug = new DeviceDebug(DeviceRef);
            }

            SwapChainRef?.Dispose();
            SwapChainRef = null;
            base.Dispose();

            if (!IsDebugMode || device3DDebug == null)
            {
                return;
            }
            device3DDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
            System.Diagnostics.Debug.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
        }
    }
}
