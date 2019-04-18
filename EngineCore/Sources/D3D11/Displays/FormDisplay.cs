using System;
using System.Windows.Forms;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

using SwapChain = SharpDX.DXGI.SwapChain;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryDXGI = SharpDX.DXGI.Factory;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace EngineCore.D3D11
{
    internal class FormDisplay : Display
    {
        public Control Surface;
        public SwapChain SwapChainRef { get; protected set; }

        protected Texture2DDescription ZBufferTextureDescription;

        public FormDisplay(bool isDebugMode, int msaaLevel) : base(isDebugMode, msaaLevel) { }

        public override void InitDevice() {
            Width = Surface.ClientSize.Width;
            Height = Surface.ClientSize.Height;

            SwapChainDescription SwapDesc = new SwapChainDescription() {
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

            Device device;
            SwapChain swapChain;

            DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            if (IsDebugMode) {
                deviceCreationFlags |= DeviceCreationFlags.Debug;
            }

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                deviceCreationFlags,
                //DeviceCreationFlags.None,
                SwapDesc,
                out device,
                out swapChain
            );
            DeviceRef = device;
            DeviceRef.DebugName = "The Device";
            DeviceRef.ImmediateContext.DebugName = "The Context";
            
            CheckFeatures();

            SwapChainRef = swapChain;
            SwapChainRef.DebugName = "The SwapChain";

            // Ignore all windows events
            FactoryDXGI factory = swapChain.GetParent<FactoryDXGI>();
            factory.MakeWindowAssociation(Surface.Handle, WindowAssociationFlags.IgnoreAll);

            ZBufferTextureDescription = new Texture2DDescription {
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
            RenderTarget2DProperites = new RenderTargetProperties(new PixelFormat(
                Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
        }

        public override void InitRenderTarget() {
            BackBuffer?.Dispose();
            RenderTargetViewRef?.Dispose();
            DepthStencilSRVRef?.Dispose();
            ZBuffer?.Dispose();
            DepthStencilViewRef?.Dispose();
            RenderTarget2D?.Dispose();

            SwapChainRef.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            using (Surface surface = SwapChainRef.GetBackBuffer<Surface>(0)) {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperites);
            }

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            BackBuffer = SwapChainRef.GetBackBuffer<Texture2D>(0);
            BackBuffer.DebugName = "BackBuffer";
            RenderTargetViewRef = new RenderTargetView(DeviceRef, BackBuffer);
            RenderTargetViewRef.DebugName = "BackBufferRenderTargetView";

            ZBufferTextureDescription.Width = Width;
            ZBufferTextureDescription.Height = Height;
            ZBuffer = new Texture2D(DeviceRef, ZBufferTextureDescription);
            ZBuffer.DebugName = "ZBuffer";

            DepthStencilViewRef = new DepthStencilView(DeviceRef, ZBuffer, new DepthStencilViewDescription {
                Format = Format.D32_Float,
                Dimension = MSamplesCount > 1 ? DepthStencilViewDimension.Texture2DMultisampled 
                    : DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            });
            DepthStencilViewRef.DebugName = "ZBufferDepthStencilView";

            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float,
            };

            if (MSamplesCount > 1) {
                SRVDesc.Dimension = ShaderResourceViewDimension.Texture2DMultisampled;
                SRVDesc.Texture2DMS = new ShaderResourceViewDescription.Texture2DMultisampledResource();
            } else {
                SRVDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                SRVDesc.Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                };
            }

            DepthStencilSRVRef = new ShaderResourceView(DeviceRef, ZBuffer, SRVDesc);
            DepthStencilSRVRef.DebugName = "ZBufferDepthStencilSRV";

            SetUpViewport();
            OnInitRenderTarget?.Invoke();
        }

        public override void Render(IntPtr Resource, bool IsNewSurface)
        {
            //SharpDX.Configuration.EnableObjectTracking = true;
            if (Surface.ClientSize.Width != Width || Surface.ClientSize.Height != Height) {
                Width = Surface.ClientSize.Width;
                Height = Surface.ClientSize.Height;
                InitRenderTarget();
                Resize();
            }
            OnRender?.Invoke();

            SwapChainRef.Present(0, PresentFlags.None);
        }

        public override void Cleanup()
        {
            DeviceDebug device3DDebug = null;
            if (IsDebugMode) {
                device3DDebug = new DeviceDebug(DeviceRef);
            }

            SwapChainRef?.Dispose();
            SwapChainRef = null;
            base.Cleanup();

            if (IsDebugMode && device3DDebug != null) {
                device3DDebug.ReportLiveDeviceObjects(ReportingLevel.Detail);
                System.Diagnostics.Debug.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
            }
        }
    }
}
