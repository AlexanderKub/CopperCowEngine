using AssetsManager.AssetsMeta;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetsManager.Loaders
{
    public class IncludeShader : SharpDX.CallbackBase, Include
    {
        private string includeDirectory;
        public string subPath;

        public IncludeShader(string shadersDirectory) {
            includeDirectory = shadersDirectory;
        }

        public void Close(Stream stream) {
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream) {
            string root = (parentStream as FileStream)?.Name ?? "";
            if (string.IsNullOrEmpty(root)) {
                return new FileStream(includeDirectory + subPath + fileName, FileMode.Open);
            }
            int c = root.LastIndexOf("\\");
            root = root.Remove(c, root.Length - c) + "\\";
            return new FileStream(root + fileName, FileMode.Open);
        }
    }

    internal class ShaderLoader
    {
        private static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string shadersDirectory = assemblyFolder + "\\Sources\\Shaders\\";
        private static IncludeShader includeShader = new IncludeShader(shadersDirectory);

        private static string[] prefixies = new string[6] { "VS", "PS", "GS", "CS", "HS", "DS" };
        public static byte[] LoadAndCompileShader(string path, ShaderTypeEnum type, string EntryPoint, Dictionary<string, object> macro)
        {
            string[] strarr = (shadersDirectory + path).Split('\\');
            if (strarr[strarr.Length - 2] != "Shaders") {
                includeShader.subPath = strarr[strarr.Length - 2] + "\\";
                if (strarr[strarr.Length - 3] != "Shaders") {
                    includeShader.subPath = strarr[strarr.Length - 3] + "\\" + includeShader.subPath;
                    if (strarr[strarr.Length - 4] != "Shaders") {
                        includeShader.subPath = strarr[strarr.Length - 4] + "\\" + includeShader.subPath;
                    }
                }
            } else {
                includeShader.subPath = strarr[strarr.Length - 1] + "\\";
            }

            string prefix = prefixies[(int)type];

            CompilationResult ShaderByteCode = ShaderBytecode.CompileFromFile(
                shadersDirectory + path,
                string.IsNullOrEmpty(EntryPoint) ? prefix + "Main" : EntryPoint,
                prefix.ToLower() + "_5_0",
                ShaderFlags.PackMatrixRowMajor,
                EffectFlags.None,
                ParseMacro(macro),
                includeShader
            );

            if (ShaderByteCode == null || ShaderByteCode.Message != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + ShaderByteCode.Message + " Path:" + path);
                return null;
            }
            return ShaderByteCode.Bytecode.Data;
        }

        public static byte[] CompileShaderFromSource(string source, ShaderTypeEnum type, Dictionary<string, object> macro)
        {
            includeShader.subPath = shadersDirectory + "/";
            string prefix = prefixies[(int)type];
            CompilationResult ShaderByteCode = ShaderBytecode.Compile(
                source,
                prefix + "Main",
                prefix.ToLower() + "_5_0",
                ShaderFlags.PackMatrixRowMajor,
                EffectFlags.None,
                ParseMacro(macro),
                includeShader
            );
            if (ShaderByteCode == null || ShaderByteCode.Message != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + ShaderByteCode.Message + " Source:" + source);
                return null;
            }
            return ShaderByteCode.Bytecode.Data;
        }

        private static SharpDX.Direct3D.ShaderMacro[] ParseMacro(Dictionary<string, object> input)
        {
            if (input == null) {
                return null;
            }

            SharpDX.Direct3D.ShaderMacro[] output = new SharpDX.Direct3D.ShaderMacro[input.Count];
            int i = 0;
            foreach (string key in input.Keys) {
                output[i++] = new SharpDX.Direct3D.ShaderMacro(key, input[key]);
            }

            return output;
        }
    }
}
