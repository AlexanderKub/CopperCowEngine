using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.D3D11.Shared;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths
{
    internal abstract partial class BaseD3D11RenderPath
    {
        private int _currentRasterizerState = -1;

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
        private string _currentVs;
        /// <summary>
        /// Non redundant set Vertex shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        protected void SetVertexShader(string name)
        {
            if (_currentVs == name)
            {
                return;
            }
            GetContext.VertexShader.Set(D3D11ShaderLoader.GetShader<VertexShader>(name));
            _currentVs = name;
        }

        /// <summary>
        /// Set Vertex shader to null.
        /// </summary>
        protected void SetNullVertexShader()
        {
            GetContext.VertexShader.Set(null);
            _currentVs = string.Empty;
        }

        private string _currentPs;
        /// <summary>
        /// Non redundant set Pixel shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetPixelShader(string name)
        {
            if (_currentPs == name)
            {
                return false;
            }
            GetContext.PixelShader.Set(D3D11ShaderLoader.GetShader<PixelShader>(name));
            _currentPs = name;
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
            _currentPs = string.Empty;
        }

        private string _currentHs;
        /// <summary>
        /// Non redundant set Hull shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetHullShader(string name)
        {
            if (_currentHs == name)
            {
                return false;
            }
            GetContext.HullShader.Set(D3D11ShaderLoader.GetShader<HullShader>(name));
            _currentHs = name;
            return true;
        }

        /// <summary>
        /// Set Hull shader to null.
        /// </summary>
        protected void SetNullHullShader()
        {
            GetContext.HullShader.Set(null);
            _currentHs = string.Empty;
        }

        private string _currentDs;
        /// <summary>
        /// Non redundant set Domain shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetDomainShader(string name)
        {
            if (_currentDs == name)
            {
                return false;
            }
            GetContext.DomainShader.Set(D3D11ShaderLoader.GetShader<DomainShader>(name));
            _currentDs = name;
            return true;
        }

        /// <summary>
        /// Set Domain shader to null.
        /// </summary>
        protected void SetNullDomainShader()
        {
            GetContext.DomainShader.Set(null);
            _currentDs = string.Empty;
        }

        private string _currentCs;
        /// <summary>
        /// Non redundant set Compute shader.
        /// </summary>
        /// <param name="name">Shader name.</param>
        /// <returns><c>True</c> if change shader.</returns>
        protected bool SetComputeShader(string name)
        {
            if (_currentCs == name)
            {
                return false;
            }
            GetContext.ComputeShader.Set(D3D11ShaderLoader.GetShader<ComputeShader>(name));
            _currentCs = name;
            return true;
        }

        /// <summary>
        /// Set Compute shader to null.
        /// </summary>
        protected void SetNullComputeShader()
        {
            GetContext.ComputeShader.Set(null);
            _currentCs = string.Empty;
        }
        #endregion

        protected void SetRasterizerState(RasterizerStateType state)
        {
            if (_currentRasterizerState == (int)state)
            {
                return;
            }
            GetContext.Rasterizer.State = GetSharedItems.GetRasterizerState(state);
            _currentRasterizerState = (int)state;
        }

        protected void SetNullRasterizerState()
        {
            if (_currentRasterizerState == -1)
            {
                return;
            }
            GetContext.Rasterizer.State = null;
            _currentRasterizerState = -1;
        }

        private int _currentBlendMode = -1;

        protected void SetBlendState(BlendStateType state)
        {
            if (_currentBlendMode == (int)state)
            {
                return;
            }
            GetContext.OutputMerger.SetBlendState(GetSharedItems.GetBlendState(state),
                GetSharedItems.BlendFactor, 0xFFFFFFFF);
            _currentBlendMode = (int)state;
        }

        protected void SetNullBlendState()
        {
            if (_currentBlendMode == -1)
            {
                return;
            }
            GetContext.OutputMerger.SetBlendState(null);
            _currentBlendMode = -1;
        }

        private int _currentDepthStencilState = -1;

        protected void SetDepthStencilState(DepthStencilStateType state)
        {
            if (_currentDepthStencilState == (int)state)
            {
                return;
            }
            GetContext.OutputMerger.SetDepthStencilState(GetSharedItems.GetDepthStencilState(state));
            _currentDepthStencilState = (int)state;
        }
        protected void SetNullDepthStencilState()
        {
            if (_currentDepthStencilState == -1)
            {
                return;
            }
            GetContext.OutputMerger.SetBlendState(null);
            _currentDepthStencilState = -1;
        }

        private readonly int[] _currentSamplerState = new[] { -1, -1, -1, -1, -1, -1, -1 };

        protected void SetSamplerState(int slot, SamplerStateType sampler)
        {
            if (_currentSamplerState[slot] == (int)sampler)
            {
                return;
            }
            GetContext.PixelShader.SetSampler(slot, GetSharedItems.GetSamplerState(sampler));
            _currentSamplerState[slot] = (int)sampler;
        }

        private SharpDX.Direct3D.PrimitiveTopology _currentPrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.Undefined;

        protected void SetPrimitiveTopology(SharpDX.Direct3D.PrimitiveTopology topology)
        {
            if (_currentPrimitiveTopology == topology)
            {
                return;
            }
            _currentPrimitiveTopology = topology;
            GetContext.InputAssembler.PrimitiveTopology = topology;
        }

        private InputLayout _currentInputLayout;

        protected void SetInputLayout(InputLayout inputLayout)
        {
            if (_currentInputLayout == inputLayout)
            {
                return;
            }
            GetContext.InputAssembler.InputLayout = inputLayout;
            _currentInputLayout = inputLayout;
        }
    }
}
