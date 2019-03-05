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

namespace EngineCore.Displays
{
    internal class FormDisplay : Display
    {
        public Control Surface;
        public SwapChain SwapChainRef { get; protected set; }

        protected Texture2DDescription ZBufferTextureDescription;

        public FormDisplay() {
        }

        public override void InitDevice() {
            Width = Surface.ClientSize.Width;
            Height = Surface.ClientSize.Height;

            SwapChainDescription SwapDesc = new SwapChainDescription() {
                BufferCount = 1,
                ModeDescription = new ModeDescription(
                    Width, Height,
                    new Rational(60, 1), 
                    Format.R8G8B8A8_UNorm
                ),
                IsWindowed = true,
                OutputHandle = Surface.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(
                 DriverType.Hardware,
                 DeviceCreationFlags.BgraSupport,
                 //DeviceCreationFlags.None,
                 SwapDesc,
                 out device,
                 out swapChain
             );
            DeviceRef = device;
            CheckFeatures();

            SwapChainRef = swapChain;

            ZBufferTextureDescription = new Texture2DDescription {
                //Format = Format.R32G8X24_Typeless,
                Format = Format.R32_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };


            Factory2D = new Factory(FactoryType.SingleThreaded, DebugLevel.Information);
            FactoryDWrite = new SharpDX.DirectWrite.Factory();
            RenderTarget2DProperites = new RenderTargetProperties(new PixelFormat(
                Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied));
        }

        public override void InitRenderTarget() {
            BackBuffer?.Dispose();
            RenderTargetViewRef?.Dispose();
            DepthStencilSRVRef?.Dispose(); ZBuffer?.Dispose();
            DepthStencilViewRef?.Dispose();
            RenderTarget2D?.Dispose();

            SwapChainRef.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            using (Surface surface = SwapChainRef.GetBackBuffer<Surface>(0)) {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperites);
            }

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            BackBuffer = SwapChainRef.GetBackBuffer<Texture2D>(0);
            RenderTargetViewRef = new RenderTargetView(DeviceRef, BackBuffer);

            ZBufferTextureDescription.Width = Width;
            ZBufferTextureDescription.Height = Height;
            ZBuffer = new Texture2D(DeviceRef, ZBufferTextureDescription);

            DepthStencilViewRef = new DepthStencilView(DeviceRef, ZBuffer, new DepthStencilViewDescription {
                //Format = Format.D32_Float_S8X24_UInt,
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            });

            DepthStencilSRVRef = new ShaderResourceView(DeviceRef, ZBuffer, new ShaderResourceViewDescription()
            {
                Format = Format.R32_Float,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource()
                {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                },
            });

            SetUpViewport();

            Context.OutputMerger.SetTargets(DepthStencilViewRef, RenderTargetViewRef);
            OnInitRenderTarget?.Invoke();
        }

        public override void Render(IntPtr Resource, bool IsNewSurface) {
            if (Surface.ClientSize.Width != Width || Surface.ClientSize.Height != Height) {
                Width = Surface.ClientSize.Width;
                Height = Surface.ClientSize.Height;
                InitRenderTarget();
                Resize();
            }
            RenderTarget2D.BeginDraw();
            OnRender?.Invoke();
            RenderTarget2D.EndDraw();
            SwapChainRef.Present(0, PresentFlags.None);
        }

        public override void Cleanup() {
            base.Cleanup();
            SwapChainRef?.Dispose();
        }
    }
}
