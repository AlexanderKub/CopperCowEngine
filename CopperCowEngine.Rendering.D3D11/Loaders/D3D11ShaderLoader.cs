using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace CopperCowEngine.Rendering.D3D11.Loaders
{
    internal class D3D11ShaderLoader
    {
        private struct ShaderPlusSignature
        {
            public DeviceChild Shader;
            public ShaderSignature Signature;
        }

        internal static D3D11RenderBackend RenderBackend;

        private static readonly Dictionary<string, ShaderPlusSignature> Shaders = new Dictionary<string, ShaderPlusSignature>();

        public static void LoadShader(string assetName)
        {
            var shaderAsset = AssetsManager.GetManager().LoadAsset<ShaderAsset>(assetName);
            //Debug.Log("AssetManager", shaderAsset.ShaderType.ToString() + " Shader " + assetName + " loaded. ");

            var pack = new ShaderPlusSignature();
            var sb = new ShaderBytecode(shaderAsset.Bytecode);
            switch (shaderAsset.ShaderType)
            {
                case ShaderType.Vertex:
                    pack.Shader = new VertexShader(RenderBackend.Device, sb);
                    break;
                case ShaderType.Pixel:
                    pack.Shader = new PixelShader(RenderBackend.Device, sb);
                    break;
                case ShaderType.Geometry:
                    pack.Shader = new GeometryShader(RenderBackend.Device, sb);
                    break;
                case ShaderType.Compute:
                    pack.Shader = new ComputeShader(RenderBackend.Device, sb);
                    break;
                case ShaderType.Hull:
                    pack.Shader = new HullShader(RenderBackend.Device, sb);
                    break;
                case ShaderType.Domain:
                    pack.Shader = new DomainShader(RenderBackend.Device, sb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            pack.Shader.DebugName = assetName;
            pack.Signature = ShaderSignature.GetInputSignature(sb);
            Shaders.Add(assetName, pack);
        }

        public static T GetShader<T>(string name) where T : DeviceChild
        {
            return GetShader<T>(name, out _);
        }

        public static T GetShader<T>(string name, out ShaderSignature signature) where T : DeviceChild
        {
            if (!Shaders.ContainsKey(name))
            {
                LoadShader(name);
            }
            Shaders.TryGetValue(name, out var pack);
            signature = pack.Signature;
            return (T)pack.Shader;
        }
    }
}
