using System;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;

namespace EngineCore
{
    internal class InteropDisplay : Display
    {
        public InteropDisplay() {
        }

        private DriverType CurrentDriverType;
        protected Texture2DDescription ZBufferTextureDescription;

        public override void InitDevice() {
            DriverType[] driverTypes = new DriverType[] {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference,
            };

            DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            FeatureLevel[] levels = new FeatureLevel[] {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
            };

            foreach (var driverType in driverTypes) {
                DeviceRef = new Device(driverType, deviceCreationFlags, levels);
                if (DeviceRef != null) {
                    CurrentDriverType = driverType;
                    break;
                }
            }

            ZBufferTextureDescription = new Texture2DDescription {
                Format = Format.R32G8X24_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            Factory2D = new Factory(FactoryType.SingleThreaded, DebugLevel.Information);
            FactoryDWrite = new SharpDX.DirectWrite.Factory();
            RenderTarget2DProperites = new RenderTargetProperties() {
                MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
                //PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
                PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                Type = RenderTargetType.Default,
                Usage = RenderTargetUsage.None,
            };
        }

        public void InitRenderTargetSurface(IntPtr Resource) {
            ZBuffer?.Dispose();
            DepthStencilViewRef?.Dispose();
            RenderTargetViewRef?.Dispose();
            RenderTarget2D?.Dispose();

            SharpDX.DXGI.Resource dxgiResource;
            using (var r = new SharpDX.ComObject(Resource)) {
                dxgiResource = r.QueryInterface<SharpDX.DXGI.Resource>();
            }

            Texture2D OutputResource = DeviceRef.OpenSharedResource<Texture2D>(dxgiResource.SharedHandle);
            using (var surface = DeviceRef.OpenSharedResource<Surface>(dxgiResource.SharedHandle)) {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperites);
            }

            //Crash everything
            //dxgiResource.Dispose();

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            RenderTargetViewRef = new RenderTargetView(DeviceRef, OutputResource);

            Texture2DDescription OutputDesc = OutputResource.Description;
            if (OutputDesc.Width != Width || OutputDesc.Height != Height) {
                Width = OutputDesc.Width;
                Height = OutputDesc.Height;
                SetUpViewport();
            }

            ZBufferTextureDescription.Width = Width;
            ZBufferTextureDescription.Height = Height;
            ZBuffer = new Texture2D(DeviceRef, ZBufferTextureDescription);

            DepthStencilViewRef = new DepthStencilView(DeviceRef, ZBuffer, new DepthStencilViewDescription {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            });

            Context.OutputMerger.SetRenderTargets(DepthStencilViewRef, RenderTargetViewRef);
            OnInitRenderTarget?.Invoke();
            OutputResource?.Dispose();
        }
        
        public override void Render(IntPtr Resource, bool IsNewSurface) {
            if (IsNewSurface) {
                Context.OutputMerger.ResetTargets();
                InitRenderTargetSurface(Resource);
                Resize();
            }

            Context.ClearRenderTargetView(RenderTargetViewRef, Engine.Instance.ClearColor);

            RenderTarget2D?.BeginDraw();
            OnRender?.Invoke();
            RenderTarget2D?.EndDraw();

            Context?.Flush();
        }
    }
}

