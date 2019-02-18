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
    public struct ShaderBytecodePack
    {
        public ShaderBytecode VertexShaderByteCode;
        public ShaderBytecode PixelShaderByteCode;
        public ShaderBytecode GeometryShaderByteCode;

        public bool HasVS() {
            return VertexShaderByteCode != null;
        }
        public bool HasGS() {
            return GeometryShaderByteCode != null;
        }
        public bool HasPS() {
            return PixelShaderByteCode != null;
        }
    }

    public class AssetsLoader
    {
        #region Meshes
        static private Dictionary<string, ModelGeometry> CachedMeshes = new Dictionary<string, ModelGeometry>();
        static public ModelGeometry LoadMesh(string assetName) {
            if (CachedMeshes.ContainsKey(assetName)) {
                return CachedMeshes[assetName];
            }

            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            MeshAsset MA = AM.LoadAsset<MeshAsset>(assetName);
            ModelGeometry MG = new ModelGeometry(MA.FileScale, MA.Pivot, MA.Vertices, MA.Indexes);
            CachedMeshes.Add(assetName, MG);
            Engine.Log("[AssetManager] Mesh " + assetName + " loaded.");
            return MG;
        }
        #endregion

        #region Materials
        static private Dictionary<string, Material> CachedMaterials = new Dictionary<string, Material>();
        static public Material LoadMaterial(string assetName) {
            if (CachedMaterials.ContainsKey(assetName)) {
                return CachedMaterials[assetName];
            }

            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            MaterialAsset MA = AM.LoadAsset<MaterialAsset>(assetName);
            Material mat = new Material() {
                Name = MA.Name,
                AlbedoMapAsset = MA.AlbedoMapAsset,
                RoughnessMapAsset = MA.RoughnessMapAsset,
                MetallicMapAsset = MA.MetallicMapAsset,
                NormalMapAsset = MA.NormalMapAsset,
                OcclusionMapAsset = MA.OcclusionMapAsset,
                PropetyBlock = new MaterialPropetyBlock() {
                    AlbedoColor = MA.AlbedoColor,
                    MetallicValue = MA.MetallicValue,
                    RoughnessValue = MA.RoughnessValue,
                    Shift = MA.Shift,
                    Tile = MA.Tile,
                },
            };
            mat.LoadMapsAndInitSampler();
            CachedMaterials.Add(assetName, mat);
            Engine.Log("[AssetManager] Material " + assetName + " loaded.");
            return mat;
        }

        #endregion

        #region Shaders
        static private Dictionary<string, ShaderBytecodePack> ShaderBytecodePacks = new Dictionary<string, ShaderBytecodePack>();
        static public void LoadShader(string assetName) {
            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            ShaderAsset SA = AM.LoadAsset<ShaderAsset>(assetName);
            Engine.Log("[AssetManager] Shader " + assetName + " loaded.");
            ShaderBytecodePacks.Add(assetName, new ShaderBytecodePack() {
                VertexShaderByteCode = new ShaderBytecode(SA.VertexBytecode),
                PixelShaderByteCode = new ShaderBytecode(SA.PixelBytecode),
                GeometryShaderByteCode = new ShaderBytecode(SA.GeometryBytecode),
            });
        }

        static public ShaderBytecodePack GetShaderPack(string name) {
            ShaderBytecodePacks.TryGetValue(name, out ShaderBytecodePack ReturnValue);
            return ReturnValue;
        }
        #endregion

        #region Textures
        private static Texture2D DebugTexture
        {
            get {
                return LoadTexture("DebugTextureMap");
            }
        }
        private static Texture2D DebugCubeTexture
        {
            get {
                return LoadCubeTexture("SkyboxCubeMap");
            }
        }

        static private Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();
        static public Texture2D LoadTexture(string assetName) {
            if (CachedTextures.ContainsKey(assetName)) {
                return CachedTextures[assetName];
            }

            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            Texture2DAsset asset = AM.LoadAsset<Texture2DAsset>(assetName);
            if (asset.IsInvalid) {
                Engine.Log("[AssetManager] Texture " + assetName + " couldn't be loaded.");
                return DebugTexture;
            }
            Engine.Log("[AssetManager] Texture " + assetName + " loaded.");

            Texture2D texture2D = new Texture2D(
                Engine.Instance.Device,
                new Texture2DDescription() {
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    MipLevels = 0,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }
            );

            int stride = asset.Data.Width * 4;
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(asset.Data.buffer, 0);
            Engine.Instance.Device.ImmediateContext.UpdateSubresource(new SharpDX.DataBox(
                ptr, stride, stride * asset.Data.Height),
                texture2D
            );
            CachedTextures.Add(assetName, texture2D);

            return texture2D;
        }

        static public Texture2D LoadCubeTexture(string assetName) {
            if (CachedTextures.ContainsKey(assetName)) {
                return CachedTextures[assetName];
            }

            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            TextureCubeAsset asset = AM.LoadAsset<TextureCubeAsset>(assetName);
            if (asset.IsInvalid) {
                Engine.Log("[AssetManager] TextureCube " + assetName + " couldn't be loaded.");
                return DebugCubeTexture;
            }
            Engine.Log("[AssetManager] TextureCube " + assetName + " loaded.");

            Texture2D cubeTexture = new Texture2D(
                Engine.Instance.Device,
                new Texture2DDescription() {
                    ArraySize = 6,
                    MipLevels = 1,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    OptionFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Width = asset.Data.Width,
                    Height = asset.Data.Height,
                }
            );

            int stride = asset.Data.Width * 4;
            for (int i = 0; i < 6; i++) {
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(asset.Data.buffer[i], 0);
                var dataBox = new SharpDX.DataBox(ptr, stride, stride * asset.Data.Height);

                Engine.Instance.Device.ImmediateContext.UpdateSubresource(dataBox,
                    cubeTexture, Resource.CalculateSubResourceIndex(0, i, 1));
            }
            CachedTextures.Add(assetName, cubeTexture);

            return cubeTexture;
        }
        #endregion

        static public void CleanupAssets() {
            foreach (string key in CachedMaterials.Keys) {
                CachedMaterials[key].Dispose();
            }
            CachedMaterials.Clear();

            foreach (string key in CachedTextures.Keys) {
                CachedTextures[key].Dispose();
            }
            CachedTextures.Clear();
            
            ShaderBytecodePacks.Clear();
            CachedMeshes.Clear();
        }
    }
}
