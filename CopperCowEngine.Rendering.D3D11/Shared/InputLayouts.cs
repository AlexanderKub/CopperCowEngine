using CopperCowEngine.Rendering.D3D11.Loaders;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CopperCowEngine.Rendering.D3D11.Shared
{
    internal partial class SharedRenderItemsStorage
    {
        public InputLayout StandardInputLayout;

        public InputElement[] StandardInputElements = new[] {
            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
            new InputElement("TANGENT", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
        };

        private void InitInputLayouts()
        {
            D3D11ShaderLoader.GetShader<VertexShader>("CommonVS", out var signature);
            StandardInputLayout = new InputLayout(_renderBackend.Device, signature, StandardInputElements)
            {
                DebugName = "StandardInputLayout"
            };
        }

        private void DisposeInputLayouts()
        {
            StandardInputLayout.Dispose();
        }
    }
}
