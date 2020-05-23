using System;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        private RasterizerState[] _rasterizerStatesArray;

        public RasterizerState GetRasterizerState(RasterizerStateType type)
        {
            return _rasterizerStatesArray[(int)type];
        }

        private void InitRasterizerState()
        {
            var isMultisampeEnabled = _renderBackend.SampleCount > 1;
            _rasterizerStatesArray = new RasterizerState[Enum.GetNames(typeof(RasterizerStateType)).Length];
            var desc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Front,
                IsMultisampleEnabled = isMultisampeEnabled,
                /*IsDepthClipEnabled = true,
                SlopeScaledDepthBias = 0.0001f,
                DepthBiasClamp = 2.0f,
                DepthBias = 25000,*/
            };
            for (var i = 0; i < _rasterizerStatesArray.Length; i++)
            {
                desc.CullMode = i % 3 == 0 ? CullMode.Front : (i % 3 == 1 ? CullMode.Back : CullMode.None);
                desc.FillMode = i > 2 ? FillMode.Wireframe : FillMode.Solid;
                _rasterizerStatesArray[i] = new RasterizerState(_renderBackend.Device, desc)
                {
                    DebugName = $"{desc.CullMode}{desc.FillMode}RasterizerState"
                };
            }
        }

        private void DisposeRasterizerState()
        {
            for (var i = 0; i < _rasterizerStatesArray.Length; i++)
            {
                _rasterizerStatesArray[i]?.Dispose();
                _rasterizerStatesArray[i] = null;
            }
            _rasterizerStatesArray = null;
        }
    }
    
    public enum RasterizerStateType : byte
    {
        SolidFrontCull,
        SolidBackCull,
        SolidNoneCull,
        WireframeFrontCull,
        WireframeBackCull,
        WireframeNoneCull,
    }
}
