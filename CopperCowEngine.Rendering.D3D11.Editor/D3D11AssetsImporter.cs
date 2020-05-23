using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering.D3D11.Editor.Loaders;

namespace CopperCowEngine.Rendering.D3D11.Editor
{
    public static class D3D11AssetsImporter
    {
        public static void CompileAndImportShader(string assetName, string path, ShaderType type, string entryPoint, Dictionary<string, object> macro)
        {
            ShaderCompiler.CompileAndImportShader(assetName, path, type, entryPoint, macro);
        }

        public static void ImportTexture(string assetName, string path, bool forceSRgb)
        {
            TextureImporter.ImportTexture(assetName, path, forceSRgb);
        }

        public static void ImportCubeTexture(string assetName, string path)
        {
            TextureImporter.ImportCubeTexture(assetName, path);
        }
        public static void BrdfIntegrate(string outputName, bool computeShader = false)
        {
            if (computeShader)
            {
                var pbrMapsPrerender = new PbrMapPreRender();
                pbrMapsPrerender.RenderBrdf(outputName);
                pbrMapsPrerender.Dispose();
                return;
            }
            var cubeMapsPrerender = new IblMapsPreRender();
            cubeMapsPrerender.RenderBrdf(outputName);
            cubeMapsPrerender.Dispose();
        }

        public static void CubeMapPrerender(string path, string outputName, bool computeShader = false)
        {
            if (computeShader)
            {
                var pbrMapsPrerender = new PbrMapPreRender();
                pbrMapsPrerender.BakeEnviromentMap(path, outputName, 1024);
                pbrMapsPrerender.Dispose();
                return;
            }
            var cubeMapsPrerender = new IblMapsPreRender();
            cubeMapsPrerender.Init(path, 1024);
            cubeMapsPrerender.Render(outputName);
            cubeMapsPrerender.Dispose();
        }
        
        public static void CompileShader(string path, string name, string entryPoint, string macro = null, string macro2 = null)
        {
            ShaderType shaderType;
            if (name.EndsWith("VS"))
            {
                shaderType = ShaderType.Vertex;
            }
            else if (name.EndsWith("PS"))
            {
                shaderType = ShaderType.Pixel;
            }
            else if (name.EndsWith("GS"))
            {
                shaderType = ShaderType.Geometry;
            }
            else if (name.EndsWith("CS"))
            {
                shaderType = ShaderType.Compute;
            }
            else if (name.EndsWith("HS"))
            {
                shaderType = ShaderType.Hull;
            }
            else if (name.EndsWith("DS"))
            {
                shaderType = ShaderType.Domain;
            }
            else
            {
                Console.WriteLine("Unknown shader type, please add correct postfix e.g. VS");
                return;
            }

            if (!string.IsNullOrEmpty(macro) && !string.IsNullOrEmpty(macro2))
            {
                D3D11AssetsImporter.CompileAndImportShader(name, path, shaderType, entryPoint, new Dictionary<string, object> {{ macro, 1 }, { macro2, 1 }});
                return;
            }

            if (!string.IsNullOrEmpty(macro))
            {
                D3D11AssetsImporter.CompileAndImportShader(name, path, shaderType, entryPoint, new Dictionary<string, object> {{ macro, 1 }});
                return;
            }
            
            D3D11AssetsImporter.CompileAndImportShader(name, path, shaderType, entryPoint, null);
        }
    }
}
