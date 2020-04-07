using System;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage : IDisposable
    {

        private D3D11RenderBackend _renderBackend;

        public SharedRenderItemsStorage(D3D11RenderBackend renderBackend)
        {
            _renderBackend = renderBackend;
            InitSamplers();
            InitDepthStencilStates();
            InitBlendStates();
            InitMeshesCache();
            InitRasterizerState();
            InitTexturesCache();
            InitInputLayouts();
            InitRenderTargets();
        }

        public void Dispose()
        {
            DisposeSamplers();
            DisposeInputLayouts();
            DisposeBlendStates();
            DisposeRasterizerState();
            DisposeMeshesCache();
            DisposeDepthStencilStates();
            DisposeRenderTargets();
            DisposeTexturesCache();
            _renderBackend = null;
        }
    }
}
