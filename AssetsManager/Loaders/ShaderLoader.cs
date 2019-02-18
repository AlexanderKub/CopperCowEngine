using SharpDX.D3DCompiler;
using System;
using System.IO;
using System.Reflection;

namespace AssetsManager.Loaders
{
    public enum ShaderPackFlags
    {
        All,
        VertexOnly,
        PixelOnly,
        GeometryOnly,
        VertexAndPixel,
        VertexAndGeometry,
        PixelAndGeometry,
    }

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

        public static byte[][] LoadAndCompileShader(string path) {
            return LoadAndCompileShader(shadersDirectory + path, ShaderPackFlags.All);
        }

        public static byte[][] LoadAndCompileShader(string path, ShaderPackFlags flags) {
            string[] strarr = path.Split('/');
            includeShader.subPath = strarr[strarr.Length - 2] + "/";
            CompilationResult VertexShaderByteCode = null;
            if (flags == ShaderPackFlags.All || flags == ShaderPackFlags.VertexOnly
                || flags == ShaderPackFlags.VertexAndPixel || flags == ShaderPackFlags.VertexAndGeometry) {
                VertexShaderByteCode = ShaderBytecode.CompileFromFile(
                    path,
                    "VSMain",
                    "vs_5_0",
                    ShaderFlags.PackMatrixRowMajor,
                    EffectFlags.None,
                    null,
                    includeShader
                );
                if (VertexShaderByteCode == null || VertexShaderByteCode.Message != null) {
                    Console.WriteLine("[ShaderCompileMessage]: " + VertexShaderByteCode.Message + " Path:" + path);
                    VertexShaderByteCode = null;
                }
            }

            CompilationResult PixelShaderByteCode = null;
            if (flags == ShaderPackFlags.All || flags == ShaderPackFlags.PixelOnly
                || flags == ShaderPackFlags.VertexAndPixel || flags == ShaderPackFlags.PixelAndGeometry) {
                PixelShaderByteCode = ShaderBytecode.CompileFromFile(
                    path,
                    "PSMain",
                    "ps_5_0",
                    ShaderFlags.PackMatrixRowMajor,
                    EffectFlags.None,
                    null,
                    includeShader
                );
                if (PixelShaderByteCode == null || PixelShaderByteCode.Message != null) {
                    Console.WriteLine("ShaderCompileMessage: " + PixelShaderByteCode.Message + " Path:" + path);
                    PixelShaderByteCode = null;
                }
            }

            CompilationResult GeometryShaderByteCode = null;
            if (flags == ShaderPackFlags.All || flags == ShaderPackFlags.GeometryOnly
                || flags == ShaderPackFlags.VertexAndGeometry || flags == ShaderPackFlags.PixelAndGeometry) {
                GeometryShaderByteCode = ShaderBytecode.CompileFromFile(
                    path,
                    "GSStream",
                    "gs_5_0",
                    ShaderFlags.PackMatrixRowMajor,
                    EffectFlags.None,
                    null,
                    includeShader
                );
                if (GeometryShaderByteCode == null || GeometryShaderByteCode.Message != null) {
                    Console.WriteLine("ShaderCompileMessage: " + GeometryShaderByteCode.Message + " Path:" + path);
                    GeometryShaderByteCode = null;
                }
            }

            byte[][] t = new byte[][] {
                VertexShaderByteCode?.Bytecode?.Data,
                PixelShaderByteCode?.Bytecode?.Data,
                GeometryShaderByteCode?.Bytecode?.Data,
            };

            return t;
        }
    }
}
