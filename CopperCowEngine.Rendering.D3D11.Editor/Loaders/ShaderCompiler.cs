using System;
using System.Collections.Generic;
using System.IO;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using SharpDX.D3DCompiler;

namespace CopperCowEngine.Rendering.D3D11.Editor.Loaders
{
    internal class IncludeShader : SharpDX.CallbackBase, Include
    {
        private readonly string _includeDirectory;
        public string SubPath;

        public IncludeShader(string shadersDirectory)
        {
            _includeDirectory = shadersDirectory;
        }

        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            var root = (parentStream as FileStream)?.Name ?? "";
            if (string.IsNullOrEmpty(root))
            {
                return new FileStream(_includeDirectory + SubPath + fileName, FileMode.Open);
            }
            var c = root.LastIndexOf("\\", StringComparison.Ordinal);
            root = root.Remove(c, root.Length - c) + "\\";
            return new FileStream(root + fileName, FileMode.Open);
        }
    }

    internal static class ShaderCompiler
    {
        //private static readonly string AssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private static readonly string ShadersDirectory =
            @"C:\Repos\CopperCowEngine_upgraide\CopperCowEngine.Rendering.D3D11.Editor\Shaders\";
        //AssemblyFolder + "\\Sources\\Shaders\\";

        private static readonly IncludeShader IncludeShader = new IncludeShader(ShadersDirectory);

        private static readonly string[] Prefixes = { "VS", "PS", "GS", "CS", "HS", "DS" };

        public static void CompileAndImportShader(string assetName, string path, ShaderType type, string entryPoint, Dictionary<string, object> macro)
        {
            var split = (ShadersDirectory + path).Split('\\');

            if (split[^2] != "Shaders")
            {
                IncludeShader.SubPath = split[^2] + "\\";
                if (split[^3] != "Shaders")
                {
                    IncludeShader.SubPath = split[^3] + "\\" + IncludeShader.SubPath;
                    if (split[^4] != "Shaders")
                    {
                        IncludeShader.SubPath = split[^4] + "\\" + IncludeShader.SubPath;
                    }
                }
            }
            else
            {
                IncludeShader.SubPath = split[^1] + "\\";
            }

            var prefix = Prefixes[(int)type];

            var shaderByteCode = ShaderBytecode.CompileFromFile(
                ShadersDirectory + path,
                string.IsNullOrEmpty(entryPoint) ? prefix + "Main" : entryPoint,
                prefix.ToLower() + "_5_0",
                ShaderFlags.PackMatrixRowMajor,
                EffectFlags.None,
                ParseMacro(macro),
                IncludeShader
            );

            if (shaderByteCode != null && shaderByteCode.Message == null)
            {
                var asset = new ShaderAsset
                {
                    Name = assetName,
                    ShaderType = type,
                    EntryPoint = entryPoint,
                    Macro = macro,
                    Bytecode = shaderByteCode.Bytecode.Data,
                };
                EditorAssetsManager.GetManager().CreateAssetFile(asset, true);
                return;
            }

            if (shaderByteCode != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + shaderByteCode.Message + " Path:" + path);
                return;
            }
            Console.WriteLine("[ShaderCompileMessage]: Unexpected Error. Path:" + path);
        }

        public static byte[] CompileShaderFromSource(string source, ShaderType type, Dictionary<string, object> macro)
        {
            IncludeShader.SubPath = ShadersDirectory + "/";
            var prefix = Prefixes[(int)type];
            var shaderByteCode = ShaderBytecode.Compile(
                source,
                prefix + "Main",
                prefix.ToLower() + "_5_0",
                ShaderFlags.PackMatrixRowMajor,
                EffectFlags.None,
                ParseMacro(macro),
                IncludeShader
            );
            if (shaderByteCode != null && shaderByteCode.Message == null)
            {
                return shaderByteCode.Bytecode.Data;
            }

            if (shaderByteCode != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + shaderByteCode.Message + " Source:" + source);
            }
            return null;
        }

        private static SharpDX.Direct3D.ShaderMacro[] ParseMacro(Dictionary<string, object> input)
        {
            if (input == null)
            {
                return null;
            }

            var output = new SharpDX.Direct3D.ShaderMacro[input.Count];
            var i = 0;
            foreach (var key in input.Keys)
            {
                output[i++] = new SharpDX.Direct3D.ShaderMacro(key, input[key]);
            }

            return output;
        }
    }
}
