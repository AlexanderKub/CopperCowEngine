using System;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        private DepthStencilState[] _depthStencilStatesArray;

        public DepthStencilState GetDepthStencilState(DepthStencilStateType stateType)
        {
            return _depthStencilStatesArray[(int)stateType];
        }

        private void InitDepthStencilStates()
        {
            _depthStencilStatesArray = new DepthStencilState[Enum.GetNames(typeof(DepthStencilStateType)).Length];
            var depthStencilStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Greater,
                IsStencilEnabled = false,
            };
            var id = (int)DepthStencilStateType.Greater;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.Greater"
            };

            id = (int)DepthStencilStateType.Less;
            depthStencilStateDesc.DepthComparison = Comparison.Less;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.Less"
            };

            id = (int)DepthStencilStateType.GreaterAndDisableWrite;
            depthStencilStateDesc.DepthWriteMask = DepthWriteMask.Zero;
            depthStencilStateDesc.DepthComparison = Comparison.Greater;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.GreaterAndDisableWrite"
            };

            id = (int)DepthStencilStateType.EqualAndDisableWrite;
            depthStencilStateDesc.DepthComparison = Comparison.Equal;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.EqualAndDisableWrite"
            };

            id = (int) DepthStencilStateType.GreaterEqualAndDisableWrite;
            depthStencilStateDesc.DepthComparison = Comparison.GreaterEqual;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.GreaterEqualAndDisableWrite"
            };

            id = (int)DepthStencilStateType.LessEqualAndDisableWrite;
            depthStencilStateDesc.DepthComparison = Comparison.LessEqual;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.LessEqualAndDisableWrite"
            };

            id = (int)DepthStencilStateType.Disabled;
            depthStencilStateDesc.IsDepthEnabled = false;
            depthStencilStateDesc.DepthComparison = Comparison.Always;
            _depthStencilStatesArray[id] = new DepthStencilState(_renderBackend.Device, depthStencilStateDesc)
            {
                DebugName = "DepthStencilStates.Disabled"
            };
        }

        private void DisposeDepthStencilStates()
        {
            for (var i = 0; i < _depthStencilStatesArray.Length; i++)
            {
                _depthStencilStatesArray[i]?.Dispose();
                _depthStencilStatesArray[i] = null;
            }
            _depthStencilStatesArray = null;
        }
    }
    
    public enum DepthStencilStateType : byte
    {
        Greater,
        Less,
        EqualAndDisableWrite,
        GreaterAndDisableWrite,
        GreaterEqualAndDisableWrite,
        LessEqualAndDisableWrite,
        Disabled,
    }
}
