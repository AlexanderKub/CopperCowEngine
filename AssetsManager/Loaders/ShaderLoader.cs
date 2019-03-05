using AssetsManager.AssetsMeta;
using SharpDX.D3DCompiler;
using System;
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
            return new FileStream(includeDirectory + subPath + fileName, FileMode.Open);
        }
    }

    internal class ShaderLoader
    {
        private static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string shadersDirectory = assemblyFolder + "/Sources/Shaders/";
        private static IncludeShader includeShader = new IncludeShader(shadersDirectory);

        private static string[] prefixies = new string[4] { "VS", "PS", "GS", "CS" };
        public static byte[] LoadAndCompileShader(string path, ShaderTypeEnum type)
        {
            string[] strarr = (shadersDirectory + path).Split('/');
            includeShader.subPath = strarr[strarr.Length - 2] + "/";
            string prefix = prefixies[(int)type];
            CompilationResult ShaderByteCode = ShaderBytecode.CompileFromFile(
                shadersDirectory + path,
                prefix + "Main",
                prefix.ToLower() + "_5_0",
                ShaderFlags.PackMatrixRowMajor,
                EffectFlags.None,
                null,
                includeShader
            );
            if (ShaderByteCode == null || ShaderByteCode.Message != null)
            {
                Console.WriteLine("[ShaderCompileMessage]: " + ShaderByteCode.Message + " Path:" + path);
                return null;
            }
            return ShaderByteCode.Bytecode.Data;
        }
    }
}
