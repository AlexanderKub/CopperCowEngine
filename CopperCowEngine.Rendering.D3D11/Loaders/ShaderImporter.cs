using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpDX.D3DCompiler;

namespace CopperCowEngine.Rendering.D3D11.Loaders
{
    public class IncludeShader : SharpDX.CallbackBase, Include
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

    internal static class ShaderImporter
    {
        private static readonly string AssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private static readonly string ShadersDirectory = AssemblyFolder + "\\Sources\\Shaders\\";

        private static readonly IncludeShader IncludeShader = new IncludeShader(ShadersDirectory);

        private static readonly string[] Prefixes = new string[6] { "VS", "PS", "GS", "CS", "HS", "DS" };

        public static byte[] CompileAndImportShader(string path, ShaderType type, string entryPoint, Dictionary<string, object> macro)
        {
            var split = (ShadersDirectory + path).Split('\\');

            if (split[split.Length - 2] != "Shaders")
            {
                IncludeShader.SubPath = split[split.Length - 2] + "\\";
                if (split[split.Length - 3] != "Shaders")
                {
                    IncludeShader.SubPath = split[split.Length - 3] + "\\" + IncludeShader.SubPath;
                    if (split[split.Length - 4] != "Shaders")
                    {
                        IncludeShader.SubPath = split[split.Length - 4] + "\\" + IncludeShader.SubPath;
                    }
                }
            }
            else
            {
                IncludeShader.SubPath = split[split.Length - 1] + "\\";
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
                return shaderByteCode.Bytecode.Data;
            }

            if (shaderByteCode != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + shaderByteCode.Message + " Path:" + path);
            }
            return null;
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
