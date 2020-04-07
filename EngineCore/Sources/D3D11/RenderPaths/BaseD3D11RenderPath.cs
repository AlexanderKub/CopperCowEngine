using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using static EngineCore.D3D11.SRITypeEnums;

namespace EngineCore.D3D11
{
    #region Render path helpers
    abstract internal partial class BaseD3D11RenderPath
    {
        /// <summary>
        /// Draw non-indexed, non-instanced primitives. Wrapper for collect drawcalls statistics.
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <param name="startVertexLocation"></param>
        protected void DX_Draw(int vertexCount, int startVertexLocation)
        {
            RenderBackend.DrawWrapper(vertexCount, startVertexLocation);
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives. Wrapper for collect drawcalls statistics.
        /// </summary>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        protected void DX_DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            RenderBackend.DrawIndexedWrapper(indexCount, startIndexLocation, baseVertexLocation);
        }

        #region Shaders binding
        private string CurrentVS;
        /// <summary>
        /// Non redundant set Vertex shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        protected void SetVertexShader(string name)
        {
            if (CurrentVS == name) {
                return;
            }
            GetContext.VertexShader.Set(AssetsLoader.GetShader<VertexShader>(name));
            CurrentVS = name;
        }

        /// <summary>
        /// Set Vertex shader to null.
        /// </summary>
        protected void SetNullVertexShader()
        {
            GetContext.VertexShader.Set(null);
            CurrentVS = string.Empty;
        }

        private string CurrentPS;
        /// <summary>
        /// Non redundant set Pixel shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetPixelShader(string name)
        {
            if (CurrentPS == name) {
                return false;
            }
            GetContext.PixelShader.Set(AssetsLoader.GetShader<PixelShader>(name));
            CurrentPS = name;
            return true;
        }

        /// <summary>
        /// Set Pixel shader to null.
        /// </summary>
        protected void SetNullPixelShader()
        {
            GetContext.PixelShader.Set(null);
            GetContext.PixelShader.SetShaderResource(0, null);
            GetContext.PixelShader.SetSamplers(0, (SamplerState)null);
            CurrentPS = string.Empty;
        }

        private string CurrentHS;
        /// <summary>
        /// Non redundant set Hull shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetHullShader(string name)
        {
            if (CurrentHS == name) {
                return false;
            }
            GetContext.HullShader.Set(AssetsLoader.GetShader<HullShader>(name));
            CurrentHS = name;
            return true;
        }

        /// <summary>
        /// Set Hull shader to null.
        /// </summary>
        protected void SetNullHullShader()
        {
            GetContext.HullShader.Set(null);
            CurrentHS = string.Empty;
        }

        private string CurrentDS;
        /// <summary>
        /// Non redundant set Domain shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetDomainShader(string name)
        {
            if (CurrentDS == name) {
                return false;
            }
            GetContext.DomainShader.Set(AssetsLoader.GetShader<DomainShader>(name));
            CurrentDS = name;
            return true;
        }

        /// <summary>
        /// Set Domain shader to null.
        /// </summary>
        protected void SetNullDomainShader()
        {
            GetContext.DomainShader.Set(null);
            CurrentDS = string.Empty;
        }

        private string CurrentCS;
        /// <summary>
        /// Non redundant set Compute shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetComputeShader(string name)
        {
            if (CurrentCS == name) {
                return false;
            }
            GetContext.ComputeShader.Set(AssetsLoader.GetShader<ComputeShader>(name));
            CurrentCS = name;
            return true;
        }

        /// <summary>
        /// Set Compute shader to null.
        /// </summary>
        protected void SetNullComputeShader()
        {
            GetContext.ComputeShader.Set(null);
            CurrentCS = string.Empty;
        }
        #endregion

        private int CurrentRasterizerState = -1;
        protected void SetRasterizerState(RasterizerStates state)
        {
            if (CurrentRasterizerState == (int)state) {
                return;
            }
            GetContext.Rasterizer.State = GetSharedItems.GetRasterizerState(state);
            CurrentRasterizerState = (int)state;
        }
        protected void SetNullRasterizerState()
        {
            if (CurrentRasterizerState == -1) {
                return;
            }
            GetContext.Rasterizer.State = null;
            CurrentRasterizerState = -1;
        }

        private int CurrentBlendMode = -1;
        protected void SetBlendState(BlendStates state)
        {
            if (CurrentBlendMode == (int)state) {
                return;
            }
            GetContext.OutputMerger.SetBlendState(GetSharedItems.GetBlendState(state),
                GetSharedItems.BlendFactor, 0xFFFFFFFF);
            CurrentBlendMode = (int)state;
        }
        protected void SetNullBlendState()
        {
            if (CurrentBlendMode == -1) {
                return;
            }
            GetContext.OutputMerger.SetBlendState(null);
            CurrentBlendMode = -1;
        }

        private int CurrentDepthStencilState = -1;
        protected void SetDepthStencilState(DepthStencilStates state)
        {
            if (CurrentDepthStencilState == (int)state) {
                return;
            }
            GetContext.OutputMerger.SetDepthStencilState(
                GetSharedItems.GetDepthStencilState(state), 0x00);
            CurrentDepthStencilState = (int)state;
        }
        protected void SetNullDepthStencilState()
        {
            if (CurrentDepthStencilState == -1) {
                return;
            }
            GetContext.OutputMerger.SetBlendState(null);
            CurrentDepthStencilState = -1;
        }

        private int[] CurrentSamplerState = new int[] { -1, -1, -1, -1, -1, -1, -1 };
        protected void SetSamplerState(int slot, SamplerType sampler)
        {
            if (CurrentSamplerState[slot] == (int)sampler) {
                return;
            }
            GetContext.PixelShader.SetSampler(slot, GetSharedItems.GetSamplerState(sampler));
            CurrentSamplerState[slot] = (int)sampler;
        }

        private SharpDX.Direct3D.PrimitiveTopology CurrentPrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.Undefined;
        protected void SetPrimitiveTopology(SharpDX.Direct3D.PrimitiveTopology topology)
        {
            if (CurrentPrimitiveTopology == topology) {
                return;
            }
            CurrentPrimitiveTopology = topology;
            GetContext.InputAssembler.PrimitiveTopology = topology;
        }

        private InputLayout CurrentInputLayout;
        protected void SetInputLayout(InputLayout inputLayout)
        {
            if (CurrentInputLayout == inputLayout) {
                return;
            }
            GetContext.InputAssembler.InputLayout = inputLayout;
            CurrentInputLayout = inputLayout;
        }
    }
    #endregion

    internal abstract partial class BaseD3D11RenderPath
    {
        protected D3D11RenderBackend RenderBackend { get; private set; }

        protected Device GetDevice {
            get {
                return RenderBackend.Device;
            }
        }

        protected DeviceContext GetContext {
            get {
                return RenderBackend.Device.ImmediateContext;
            }
        }

        protected Display GetDisplay {
            get {
                return RenderBackend.DisplayRef;
            }
        }

        protected SharedRenderItemsStorage GetSharedItems {
            get {
                return RenderBackend.SharedRenderItems;
            }
        }

        internal BaseD3D11RenderPath() { }

        protected bool EnabledMsaa { get; private set; }

        protected int MsSamplesCount { get; private set; }

        protected bool EnabledHdr { get; private set; }

        public virtual void Init(D3D11RenderBackend renderBackend)
        {
            ToDisposeList = new List<IDisposable>();
            RenderBackend = renderBackend;
            EnabledMsaa = renderBackend.SampleCount > 1;
            MsSamplesCount = 1;
            if (EnabledMsaa) {
                switch (renderBackend.EngineRef.CurrentConfig.EnableMSAA) {
                    case Engine.EngineConfiguration.MSAAEnabled.X4:
                        MsSamplesCount = 4;
                        break;
                    case Engine.EngineConfiguration.MSAAEnabled.X8:
                        MsSamplesCount = 8;
                        break;
                    case Engine.EngineConfiguration.MSAAEnabled.Off:
                        MsSamplesCount = 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            EnabledHdr = RenderBackend.EngineRef.CurrentConfig.EnableHDR;
        }

        public virtual void Draw(StandardFrameData frameData) { }

        public virtual void Resize() { }
        
        private List<IDisposable> ToDisposeList;
        protected void ToDispose(IDisposable item)
        {
            ToDisposeList.Add(item);
        }

        /// <summary>
        /// Must implement base call.
        /// </summary>
        public virtual void Dispose() {
            foreach (var item in ToDisposeList)
            {
                item?.Dispose();
            }
            ToDisposeList.Clear();
            ToDisposeList = null;
            CurrentInputLayout = null;
        }
    }
}
