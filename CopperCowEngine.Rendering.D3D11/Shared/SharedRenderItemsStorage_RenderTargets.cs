using System.Collections.Generic;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        private Dictionary<string, RenderTargetPack> _renderTargetPacks;
        
        private Dictionary<string, DepthStencilTargetPack> _depthStencilTargetPacks;

        public RenderTargetPack CreateRenderTarget(string name, int width, int height, Format format, 
            int samples, bool disposeExist = true)
        {
            if (_renderTargetPacks.ContainsKey(name))
            {
                //Debug.Warning("CreateRenderTarget", $"Target {Name} already exist.");
                if (!disposeExist)
                {
                    return _renderTargetPacks[name];
                }
                _renderTargetPacks[name]?.Dispose();
                _renderTargetPacks.Remove(name);
            }

            var tmp = new RenderTargetPack(name, samples);
            tmp.Create(_renderBackend.Device, width, height, format);
            _renderTargetPacks.Add(name, tmp);
            return tmp;
        }

        public void ResizeRenderTarget(string name, int width, int height)
        {
            if (!_renderTargetPacks.ContainsKey(name))
            {
                //Debug.Warning("ResizeRenderTarget", $"Target {Name} not exist.");
                return;
            }
            _renderTargetPacks[name].Resize(_renderBackend.Device, width, height);
        }

        public DepthStencilTargetPack CreateDepthRenderTarget(string name, int width, int height, 
            int samples, bool disposeExist = true)
        {
            if (_depthStencilTargetPacks.ContainsKey(name))
            {
                //Debug.Warning("CreateDepthRenderTarget", $"Target {Name} already exist.");
                if (!disposeExist)
                {
                    return _depthStencilTargetPacks[name];
                }
                _depthStencilTargetPacks[name]?.Dispose();
                _depthStencilTargetPacks.Remove(name);
            }

            var tmp = new DepthStencilTargetPack(name, samples);
            tmp.Create(_renderBackend.Device, width, height);
            _depthStencilTargetPacks.Add(name, tmp);
            return tmp;
        }

        public void ResizeDepthRenderTarget(string name, int width, int height)
        {
            if (!_depthStencilTargetPacks.ContainsKey(name))
            {
                //Debug.Warning("ResizeRenderTarget", $"Target {Name} not exist.");
                return;
            }
            _depthStencilTargetPacks[name].Resize(_renderBackend.Device, width, height);
        }

        private void InitRenderTargets()
        {
            _renderTargetPacks = new Dictionary<string, RenderTargetPack>();
            _depthStencilTargetPacks = new Dictionary<string, DepthStencilTargetPack>();
        }

        private void DisposeRenderTargets()
        {
            foreach (var item in _renderTargetPacks)
            {
                item.Value.Dispose();
            }
            _renderTargetPacks.Clear();
            _renderTargetPacks = null;

            foreach (var item in _depthStencilTargetPacks)
            {
                item.Value.Dispose();
            }
            _depthStencilTargetPacks.Clear();
            _depthStencilTargetPacks = null;
        }
    }
}
