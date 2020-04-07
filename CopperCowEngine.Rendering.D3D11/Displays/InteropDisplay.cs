using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.Direct2D1.Factory;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;

namespace CopperCowEngine.Rendering.D3D11.Displays
{
    internal sealed class InteropDisplay : Display
    {
        private DriverType _currentDriverType;

        private Texture2DDescription _zBufferTextureDescription;

        public InteropDisplay(bool isDebugMode, int msaaLevel) : base(isDebugMode, msaaLevel) { }

        public override void InitDevice()
        {
            Width = 100;
            Height = 100;
            var driverTypes = new[] {
                DriverType.Hardware,
                DriverType.Warp,
                DriverType.Reference,
            };

            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            if (IsDebugMode)
            {
                deviceCreationFlags |= DeviceCreationFlags.Debug;
            }

            var levels = new[] {
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0,
            };

            foreach (var driverType in driverTypes)
            {
                DeviceRef = new Device(driverType, deviceCreationFlags, levels);
                if (DeviceRef == null)
                {
                    continue;
                }
                _currentDriverType = driverType;
                break;
            }

            DeviceRef.DebugName = "Interop Device";
            DeviceRef.ImmediateContext.DebugName = "Interop Context";

            CheckFeatures();

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
            RenderTarget2DProperties = new RenderTargetProperties()
            {
                MinLevel = SharpDX.Direct2D1.FeatureLevel.Level_DEFAULT,
                //PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
                PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Premultiplied),
                Type = RenderTargetType.Default,
                Usage = RenderTargetUsage.None,
            };
        }

        public void InitRenderTargetSurface(IntPtr resource)
        {
            ZBuffer?.Dispose();
            DepthStencilViewRef?.Dispose();
            RenderTargetViewRef?.Dispose();
            RenderTarget2D?.Dispose();

            SharpDX.DXGI.Resource dxgiResource;
            using (var r = new SharpDX.ComObject(resource))
            {
                dxgiResource = r.QueryInterface<SharpDX.DXGI.Resource>();
            }

            var outputResource = DeviceRef.OpenSharedResource<Texture2D>(dxgiResource.SharedHandle);
            using (var surface = DeviceRef.OpenSharedResource<Surface>(dxgiResource.SharedHandle))
            {
                RenderTarget2D = new RenderTarget(Factory2D, surface, RenderTarget2DProperties);
            }

            //Crash everything
            //dxgiResource.Dispose();

            RenderTarget2D.AntialiasMode = AntialiasMode.PerPrimitive;
            RenderTarget2D.TextAntialiasMode = TextAntialiasMode.Cleartype;

            RenderTargetViewRef = new RenderTargetView(DeviceRef, outputResource);

            var outputDesc = outputResource.Description;
            if (outputDesc.Width != Width || outputDesc.Height != Height)
            {
                Width = outputDesc.Width;
                Height = outputDesc.Height;
                SetUpViewport();
            }

            _zBufferTextureDescription.Width = Width;
            _zBufferTextureDescription.Height = Height;
            ZBuffer = new Texture2D(DeviceRef, _zBufferTextureDescription);

            DepthStencilViewRef = new DepthStencilView(DeviceRef, ZBuffer, new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
            });

            Context.OutputMerger.SetRenderTargets(DepthStencilViewRef, RenderTargetViewRef);
            InitRenderTarget();
            outputResource?.Dispose();
        }

        public override void Render(IntPtr resource, bool isNewSurface)
        {
            if (isNewSurface)
            {
                Context.OutputMerger.ResetTargets();
                InitRenderTargetSurface(resource);
                Resize();
            }

            Context.ClearRenderTargetView(RenderTargetViewRef, Color.Gray);

            RenderTarget2D?.BeginDraw();
            base.Render(resource, isNewSurface);
            RenderTarget2D?.EndDraw();

            Context?.Flush();
        }
    }
}

