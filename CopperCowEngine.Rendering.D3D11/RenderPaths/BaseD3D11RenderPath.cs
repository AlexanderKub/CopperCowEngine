using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering.D3D11.Displays;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.Data;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal abstract partial class BaseD3D11RenderPath : IDisposable
    {
        private List<IDisposable> _toDisposeList;

        protected D3D11RenderBackend RenderBackend { get; private set; }

        protected Device GetDevice => RenderBackend.Device;

        protected DeviceContext GetContext => RenderBackend.Device.ImmediateContext;

        protected Display GetDisplay => RenderBackend.DisplayRef;

        protected SharedRenderItemsStorage GetSharedItems => RenderBackend.SharedRenderItems;

        protected bool HdrEnable => RenderBackend.Configuration.EnableHdr;

        protected int MsSamplesCount => RenderBackend.SampleCount;

        protected bool MsaaEnable => MsSamplesCount > 1;
        
        protected bool MotionBlurEnable => HdrEnable && RenderBackend.Configuration.PostProcessing.MotionBlur.Enable;

        protected bool BloomEnable => HdrEnable && RenderBackend.Configuration.PostProcessing.Bloom.Enable;

        protected bool DofBlurEnable => HdrEnable && RenderBackend.Configuration.PostProcessing.DofBlur.Enable;

        public virtual void Init(D3D11RenderBackend renderBackend)
        {
            _toDisposeList = new List<IDisposable>();

            RenderBackend = renderBackend;
        }

        public virtual void Draw(StandardFrameData frameData) { }

        public abstract void Resize();

        protected void ToDispose(IDisposable item)
        {
            _toDisposeList.Add(item);
        }

        public virtual void Dispose()
        {
            foreach (var item in _toDisposeList)
            {
                item?.Dispose();
            }
            _toDisposeList.Clear();
            _toDisposeList = null;
            _currentInputLayout = null;
        }
    }
}
