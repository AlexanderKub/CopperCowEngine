﻿using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct2D1;

using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace CopperCowEngine.Rendering.D3D11.Displays
{
    internal abstract class Display : IDisposable
    {
        protected bool IsDebugMode;

        protected int SamplesCount;

        protected RenderTargetProperties RenderTarget2DProperties;

        public Device DeviceRef { get; protected set; }

        public DeviceContext Context => DeviceRef?.ImmediateContext;

        public RenderTargetView RenderTarget { get; protected set; }

        public Texture2D BackBuffer { get; protected set; }

        public int Width { get; protected set; }

        public int Height { get; protected set; }

        public Factory Factory2D { get; protected set; }

        public RenderTarget RenderTarget2D { get; protected set; }

        public SharpDX.DirectWrite.Factory FactoryDWrite { get; protected set; }

        public float AspectRatio => (float)Width / Height;

        public bool ShaderDoublesSupport { get; private set; }

        protected Display(bool isDebugMode, int msaaLevel)
        {
            IsDebugMode = isDebugMode;
            SamplesCount = msaaLevel;
        }

        public abstract void InitDevice();

        public virtual void InitRenderTarget()
        {
            OnInitRenderTarget?.Invoke();
        }

        public virtual void Render(IntPtr resource, bool isNewSurface)
        {
            OnRender?.Invoke();
        }

        protected virtual void SetUpViewport()
        {
            Context.Rasterizer.SetViewport(new Viewport(
                0, 0,
                Width,
                Height,
                0.0f, 1.0f
            ));
        }

        protected void Resize()
        {
            OnResize?.Invoke();
        }

        public virtual void Dispose()
        {
            Context?.ClearState();
            Context?.Flush();

            BackBuffer?.Dispose();
            BackBuffer = null;

            RenderTarget?.Dispose();
            RenderTarget = null;

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

        protected void CheckFeatures()
        {
            ShaderDoublesSupport = DeviceRef.CheckFeatureSupport(SharpDX.Direct3D11.Feature.ShaderDoubles);
        }

        public event Action OnRender;

        public event Action OnInitRenderTarget;

        public event Action OnResize;
    }
}