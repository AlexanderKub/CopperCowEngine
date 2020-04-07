using AssetsManager;
using AssetsManager.Loaders;
using AssetsManager.AssetsMeta;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace EngineCore
{
    public class AssetsLoader
    {
        #region Meshes
        // Cross-backend
        private static readonly Dictionary<string, ModelGeometry> CachedMeshes = new Dictionary<string, ModelGeometry>();

        public static ModelGeometry LoadMesh(string assetName)
        {
            if (CachedMeshes.ContainsKey(assetName))
            {
                return CachedMeshes[assetName];
            }

            var meshAsset = AssetsManagerInstance.GetManager().LoadAsset<MeshAsset>(assetName);
            var modelGeometry = new ModelGeometry(meshAsset.FileScale, meshAsset.Pivot, meshAsset.Vertices,
                meshAsset.Indexes, meshAsset.BoundingMinimum, meshAsset.BoundingMaximum);
            CachedMeshes.Add(assetName, modelGeometry);
            Debug.Log("AssetManager", "Mesh " + assetName + " loaded.");
            return modelGeometry;
        }

        public sealed class MeshInfo
        {
            public string Name { get; }
            public BoundsBox Bounds { get; }

            internal MeshInfo(string name, BoundsBox bounds)
            {
                Name = name;
                Bounds = bounds;
            }
        }

        public enum PrimitivesMesh
        {
            Cube,
            Sphere,
        }

        public static MeshInfo LoadMeshInfo(PrimitivesMesh primitive)
        {
            ModelGeometry modelGeometry = null;
            string name;
            switch (primitive)
            {
                case PrimitivesMesh.Cube:
                    name = "Primitives.Cube";
                    modelGeometry = Primitives.Cube;
                    break;
                case PrimitivesMesh.Sphere:
                    name = "Primitives.Sphere";
                    modelGeometry = Primitives.Sphere(32);//20
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null);
            }
            return new MeshInfo(name, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static MeshInfo LoadMeshInfo(string assetName)
        {
            var modelGeometry = LoadMesh(assetName);
            return new MeshInfo(assetName, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static void DropCachedMesh(string assetName)
        {
            if (!CachedMeshes.ContainsKey(assetName))
            {
                return;
            }
            CachedMeshes[assetName] = null;
            CachedMeshes.Remove(assetName);
        }
        #endregion

        #region Materials
        // Cross-backend
        private static readonly Dictionary<string, Material> CachedMaterials = new Dictionary<string, Material>();
        public static Material LoadMaterial(string assetName)
        {
            if (CachedMaterials.ContainsKey(assetName))
            {
                return CachedMaterials[assetName];
            }

            var materialAsset = AssetsManagerInstance.GetManager().LoadAsset<MaterialAsset>(assetName);
            var mat = new Material()
            {
                Name = materialAsset.Name,
                AlbedoMapAsset = materialAsset.AlbedoMapAsset,
                RoughnessMapAsset = materialAsset.RoughnessMapAsset,
                MetallicMapAsset = materialAsset.MetallicMapAsset,
                NormalMapAsset = materialAsset.NormalMapAsset,
                OcclusionMapAsset = materialAsset.OcclusionMapAsset,
                PropetyBlock = new MaterialPropetyBlock()
                {
                    AlbedoColor = materialAsset.AlbedoColor,
                    AlphaValue = materialAsset.AlphaValue,
                    MetallicValue = materialAsset.MetallicValue,
                    RoughnessValue = materialAsset.RoughnessValue,
                    Shift = materialAsset.Shift,
                    Tile = materialAsset.Tile,
                },
            };
            CachedMaterials.Add(assetName, mat);
            Debug.Log("AssetManager", "Material " + assetName + " loaded.");
            return mat;
        }

        public sealed class MaterialInfo
        {
            public string Name { get; }
            public int Queue { get; }

            internal MaterialInfo(string name, int queue)
            {
                Name = name;
                Queue = queue;
            }
        }
        public static MaterialInfo LoadMaterialInfo(string assetName)
        {
            var material = LoadMaterial(assetName);
            return new MaterialInfo(assetName, material.ShaderQueue);
        }

        public static MaterialInfo LoadMaterialInfo(Material material)
        {
            CachedMaterials.Add(material.Name, material);
            return new MaterialInfo(material.Name, material.ShaderQueue);
        }

        public static void DropCachedTexture(string name)
        {
            RenderBackend.SharedRenderItems.DropCachedTexture(name);
        }
        #endregion

        #region Shaders
        // Non Cross-backend
        //TODO: Shader asset for shader graph, actual shader load in SRIStorage
        private struct ShaderPlusSignature
        {
            public DeviceChild Shader;
            public ShaderSignature Signature;
        }

        internal static D3D11.D3D11RenderBackend RenderBackend;
        public static void LoadShader(string assetName)
        {
            var shaderAsset = AssetsManagerInstance.GetManager().LoadAsset<ShaderAsset>(assetName);
            Debug.Log("AssetManager", shaderAsset.ShaderType.ToString() + " Shader " + assetName + " loaded. ");

            var pack = new ShaderPlusSignature();
            var sb = new ShaderBytecode(shaderAsset.Bytecode);
            switch (shaderAsset.ShaderType)
            {
                case ShaderTypeEnum.Vertex:
                    pack.Shader = new VertexShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Pixel:
                    pack.Shader = new PixelShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Geometry:
                    pack.Shader = new GeometryShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Compute:
                    pack.Shader = new ComputeShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Hull:
                    pack.Shader = new HullShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Domain:
                    pack.Shader = new DomainShader(RenderBackend.Device, sb);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            pack.Shader.DebugName = assetName;
            pack.Signature = ShaderSignature.GetInputSignature(sb);
            Shaders.Add(assetName, pack);
        }

        private static readonly Dictionary<string, ShaderPlusSignature> Shaders = new Dictionary<string, ShaderPlusSignature>();

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
        #endregion

        public static void CleanupAssets()
        {
            CachedMaterials.Clear();

            foreach (var key in Shaders.Keys)
            {
                Shaders[key].Shader.Dispose();
            }
            Shaders.Clear();

            CachedMeshes.Clear();
        }
    }
}
