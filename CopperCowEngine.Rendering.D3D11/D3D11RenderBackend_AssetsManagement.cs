using System.Collections.Generic;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.Rendering.D3D11
{
    public partial class D3D11RenderBackend
    {
        public override void BrdfIntegrate(string outputName)
        {
            var cubeMapsPrerender = new IBLMapsPreRender();
            cubeMapsPrerender.RenderBRDF(outputName);
            cubeMapsPrerender.Dispose();
        }

        public override byte[] CompileAndImportShader(string path, ShaderType type, string entryPoint, Dictionary<string, object> macro)
        {
            return ShaderImporter.CompileAndImportShader(path, type, entryPoint, macro);
        }

        public override void CubeMapPrerender(string path, string outputName)
        {
            var cubeMapsPrerender = new IBLMapsPreRender();
            cubeMapsPrerender.Init(path);
            cubeMapsPrerender.Render(outputName);
            cubeMapsPrerender.Dispose();
        }

        public override TextureAssetData ImportTexture(string path, bool forceSRgb)
        {
            return TextureImporter.ImportTexture(path, forceSRgb);
        }

        public override TextureCubeAssetData ImportCubeTexture(string path)
        {
            return TextureImporter.ImportCubeTexture(path);
        }
    }
}
