using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;

using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace EngineCore.D3D11
{
    internal abstract class Display
    {
        public Device DeviceRef { get; protected set; }
        public DeviceContext Context
        {
            get {
                return DeviceRef?.ImmediateContext;
            }
        }

        public RenderTargetView RenderTargetViewRef { get; protected set; }
        public Texture2D BackBuffer { get; protected set; }
        public Texture2D ZBuffer { get; protected set; }
        public DepthStencilView DepthStencilViewRef { get; protected set; }
        public ShaderResourceView DepthStencilSRVRef { get; protected set; }

        protected RenderTargetProperties RenderTarget2DProperites;

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Action OnRender;
        public Action OnInitRenderTarget;

        public Factory Factory2D { get; protected set; }
        public RenderTarget RenderTarget2D { get; protected set; }
        public SharpDX.DirectWrite.Factory FactoryDWrite { get; protected set; }

        public float AspectRatio {
            get {
                return ((float)Width) / Height;
            }
        }

        protected bool IsDebugMode;
        protected int MSamplesCount;

        public Display(bool isDebugMode, int msaaLevel) {
            IsDebugMode = isDebugMode;
            MSamplesCount = msaaLevel == 0 ? 1 : msaaLevel;
        }

        public virtual void InitDevice() { }
        public virtual void InitRenderTarget() { }
        public virtual void Render(IntPtr Resource, bool IsNewSurface) { }

        protected virtual void SetUpViewport() {
            Context.Rasterizer.SetViewport(new Viewport(
                0, 0,
                Width,
                Height,
                0.0f, 1.0f
            ));
        }

        public bool ShaderDoubles;
        protected void CheckFeatures() {
            ShaderDoubles = DeviceRef.CheckFeatureSupport(SharpDX.Direct3D11.Feature.ShaderDoubles);
        }

        public event Action OnResize;
        protected void Resize() {
            OnResize?.Invoke();
            //(Engine.Instance.RenderBackend as D3D11.D3D11RenderBackend).RenderPath.Resize();
            //Engine.Instance.MainCamera.AspectRatio = AspectRatio;
        }

        public virtual void Cleanup()
        {
            Context?.ClearState();
            Context?.Flush();

            BackBuffer?.Dispose();
            BackBuffer = null;
            ZBuffer?.Dispose();
            ZBuffer = null;

            RenderTargetViewRef?.Dispose();
            RenderTargetViewRef = null;

            DepthStencilSRVRef?.Dispose();
            DepthStencilSRVRef = null;

            DepthStencilViewRef?.Dispose();
            DepthStencilViewRef = null;

            FactoryDWrite?.Dispose();
            FactoryDWrite = null;
            Factory2D?.Dispose();
            Factory2D = null;
            RenderTarget2D?.Dispose();
            RenderTarget2D = null;

            Context?.Dispose();
            DeviceRef?.Dispose();
            DeviceRef = null;
        }
    }
}