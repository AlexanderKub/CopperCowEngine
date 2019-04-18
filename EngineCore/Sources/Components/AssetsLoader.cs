using AssetsManager;
using AssetsManager.Loaders;
using AssetsManager.AssetsMeta;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;

namespace EngineCore
{
    public class AssetsLoader
    {
        #region Meshes
        // Cross-backend
        static private Dictionary<string, ModelGeometry> CachedMeshes = new Dictionary<string, ModelGeometry>();
        static public ModelGeometry LoadMesh(string assetName) {
            if (CachedMeshes.ContainsKey(assetName)) {
                return CachedMeshes[assetName];
            }

            MeshAsset MA = AssetsManagerInstance.GetManager().LoadAsset<MeshAsset>(assetName);
            ModelGeometry MG = new ModelGeometry(MA.FileScale, MA.Pivot, MA.Vertices, MA.Indexes, MA.BoundingMinimum, MA.BoundingMaximum);
            MA = null;
            CachedMeshes.Add(assetName, MG);
            Debug.Log("AssetManager", "Mesh " + assetName + " loaded.");
            return MG;
        }
        
        public sealed class MeshInfo
        {
            public string Name { get; private set; }
            public BoundsBox Bounds { get; private set; }

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

        static public MeshInfo LoadMeshInfo(PrimitivesMesh primitive)
        {
            ModelGeometry MG = null;
            string name = "";
            switch (primitive)
            {
                case PrimitivesMesh.Cube:
                    name = "Primitives.Cube";
                    MG = Primitives.Cube;
                    break;
                case PrimitivesMesh.Sphere:
                    name = "Primitives.Sphere";
                    MG = Primitives.Sphere(32);//20
                    break;
            }
            return new MeshInfo(name, new BoundsBox(MG.BoundingMinimum, MG.BoundingMaximum));
        }

        static public MeshInfo LoadMeshInfo(string assetName)
        {
            ModelGeometry MG = LoadMesh(assetName);
            return new MeshInfo(assetName, new BoundsBox(MG.BoundingMinimum, MG.BoundingMaximum));
        }

        static public void DropCachedMesh(string assetName)
        {
            if (CachedMeshes.ContainsKey(assetName))
            {
                CachedMeshes[assetName] = null;
                CachedMeshes.Remove(assetName);
            }
        }
        #endregion

        #region Materials
        // Cross-backend
        static private Dictionary<string, Material> CachedMaterials = new Dictionary<string, Material>();
        static public Material LoadMaterial(string assetName) {
            if (CachedMaterials.ContainsKey(assetName)) {
                return CachedMaterials[assetName];
            }
            
            MaterialAsset MA = AssetsManagerInstance.GetManager().LoadAsset<MaterialAsset>(assetName);
            Material mat = new Material() {
                Name = MA.Name,
                AlbedoMapAsset = MA.AlbedoMapAsset,
                RoughnessMapAsset = MA.RoughnessMapAsset,
                MetallicMapAsset = MA.MetallicMapAsset,
                NormalMapAsset = MA.NormalMapAsset,
                OcclusionMapAsset = MA.OcclusionMapAsset,
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = MA.AlbedoColor,
                    AlphaValue = MA.AlphaValue,
                    MetallicValue = MA.MetallicValue,
                    RoughnessValue = MA.RoughnessValue,
                    Shift = MA.Shift,
                    Tile = MA.Tile,
                },
            };
            MA = null;
            CachedMaterials.Add(assetName, mat);
            Debug.Log("AssetManager", "Material " + assetName + " loaded.");
            return mat;
        }

        public sealed class MaterialInfo
        {
            public string Name { get; private set; }
            public int Queue { get; private set; }

            internal MaterialInfo(string name, int queue)
            {
                Name = name;
                Queue = queue;
            }
        }
        static public MaterialInfo LoadMaterialInfo(string assetName)
        {
            Material material = LoadMaterial(assetName);
            return new MaterialInfo(assetName, material.ShaderQueue);
        }

        static public MaterialInfo LoadMaterialInfo(Material material)
        {
            CachedMaterials.Add(material.Name, material);
            return new MaterialInfo(material.Name, material.ShaderQueue);
        }

        static public void DropCachedTexture(string name)
        {
            RenderBackend.SharedRenderItems.DropCachedTexture(name);
        }
        #endregion

        #region Shaders
        // Non Cross-backend
        //TODO: Shader asset for shader graph, actual shader load in SRIStorage
        private struct ShaderPlusSignature
        {
            public DeviceChild shader;
            public ShaderSignature signature;
        }

        internal static D3D11.D3D11RenderBackend RenderBackend;
        static public void LoadShader(string assetName) {
            ShaderAsset SA = AssetsManagerInstance.GetManager().LoadAsset<ShaderAsset>(assetName);
            Debug.Log("AssetManager", SA.ShaderType.ToString() + " Shader " + assetName + " loaded. ");

            ShaderPlusSignature pack = new ShaderPlusSignature();
            ShaderBytecode sb = new ShaderBytecode(SA.Bytecode);
            switch (SA.ShaderType)  
            {
                case ShaderTypeEnum.Vertex:
                    pack.shader = new VertexShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Pixel:
                    pack.shader = new PixelShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Geometry:
                    pack.shader = new GeometryShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Compute:
                    pack.shader = new ComputeShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Hull:
                    pack.shader = new HullShader(RenderBackend.Device, sb);
                    break;
                case ShaderTypeEnum.Domain:
                    pack.shader = new DomainShader(RenderBackend.Device, sb);
                    break;
                default:
                    break;
            };
            SA = null;
            pack.shader.DebugName = assetName;
            pack.signature = ShaderSignature.GetInputSignature(sb);
            Shaders.Add(assetName, pack);
        }

        static private Dictionary<string, ShaderPlusSignature> Shaders = new Dictionary<string, ShaderPlusSignature>();

        static public T GetShader<T>(string name) where T : DeviceChild
        {
            return GetShader<T>(name, out ShaderSignature signature);
        }

        static public T GetShader<T>(string name, out ShaderSignature signature) where T: DeviceChild
        {
            if (!Shaders.ContainsKey(name)) {
                LoadShader(name);
            }
            Shaders.TryGetValue(name, out ShaderPlusSignature pack);
            signature = pack.signature;
            return (T)(pack.shader);
        }
        #endregion

        static public void CleanupAssets() {
            CachedMaterials.Clear();

            foreach (string key in Shaders.Keys)
            {
                Shaders[key].shader.Dispose();
            }
            Shaders.Clear();

            CachedMeshes.Clear();
        }
    }
}
