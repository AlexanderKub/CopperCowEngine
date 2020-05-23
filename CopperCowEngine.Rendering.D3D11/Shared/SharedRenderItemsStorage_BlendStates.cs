using System;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        public SharpDX.Mathematics.Interop.RawColor4 BlendFactor = new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0);

        private BlendState[] _blendStatesArray;

        public BlendState GetBlendState(BlendStateType type)
        {
            return _blendStatesArray[(int)type];
        }

        private void InitBlendStates()
        {
            _blendStatesArray = new BlendState[Enum.GetNames(typeof(BlendStateType)).Length];
            var blendStateDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            blendStateDesc.RenderTarget[0].IsBlendEnabled = false;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            _blendStatesArray[(int) BlendStateType.Opaque] = new BlendState(_renderBackend.Device, blendStateDesc)
            {
                DebugName = "BlendStates.Opaque"
            };

            blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            _blendStatesArray[(int) BlendStateType.AlphaEnabledBlending] =
                new BlendState(_renderBackend.Device, blendStateDesc)
                {
                    DebugName = "BlendStates.AlphaEnabledBlending"
                };

            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
            _blendStatesArray[(int) BlendStateType.DepthOnlyAlphaTest] =
                new BlendState(_renderBackend.Device, blendStateDesc)
                {
                    DebugName = "BlendStates.DepthOnlyAlphaTest"
                };

            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = 0;
            blendStateDesc.AlphaToCoverageEnable = true;
            _blendStatesArray[(int) BlendStateType.DepthOnlyAlphaToCoverage] =
                new BlendState(_renderBackend.Device, blendStateDesc)
                {
                    DebugName = "BlendStates.DepthOnlyAlphaToCoverage"
                };

            blendStateDesc = new BlendStateDescription();
            blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.One;
            blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            _blendStatesArray[(int)BlendStateType.Additive] =
                new BlendState(_renderBackend.Device, blendStateDesc)
                {
                    DebugName = "BlendStates.Additive"
                };
        }

        private void DisposeBlendStates()
        {
            for (var i = 0; i < _blendStatesArray.Length; i++)
            {
                _blendStatesArray[i]?.Dispose();
                _blendStatesArray[i] = null;
            }
            _blendStatesArray = null;
        }
    }
    
    public enum BlendStateType : byte
    {
        Opaque,
        AlphaEnabledBlending,
        DepthOnlyAlphaTest,
        DepthOnlyAlphaToCoverage,
        Additive,
    }
}
