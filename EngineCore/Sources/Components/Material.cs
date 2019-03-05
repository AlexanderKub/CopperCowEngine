using EngineCore.RenderTechnique;
using SharpDX;
using SharpDX.Direct3D11;

namespace EngineCore
{
    public class MaterialPropetyBlock
    {
        public Vector3 AlbedoColor = Vector3.One;
        public float AlphaValue = 1.0f;
        public float RoughnessValue = 0.5f;
        public float MetallicValue = 0.0f;
        public Vector2 Tile = Vector2.One;
        public Vector2 Shift = Vector2.Zero;
    };

    public class Material {
        public string Name;
        internal ShaderResourceView albedoMapView;
        internal ShaderResourceView normalMapView;
        internal ShaderResourceView roughnessMapView;
        internal ShaderResourceView metallicMapView;
        internal ShaderResourceView occlusionMapView;
        internal SamplerState MaterialSampler;

        public bool HasSampler {
            get {
                return MaterialSampler != null;
            }
        }

        public bool HasAlbedoMap
        {
            get {
                return albedoMapView != null;
            }
        }

        public bool HasNormalMap {
            get {
                return normalMapView != null;
            }
        }

        public bool HasRoughnessMap
        {
            get {
                return roughnessMapView != null;
            }
        }
        
        public bool HasMetallicMap {
            get {
                return metallicMapView != null;
            }
        }

        public bool HasOcclusionMap
        {
            get {
                return occlusionMapView != null;
            }
        }

        public string AlbedoMapAsset;
        public string NormalMapAsset;
        public string RoughnessMapAsset;
        public string MetallicMapAsset;
        public string OcclusionMapAsset;

        public MaterialPropetyBlock PropetyBlock;

        public static Material DefaultMaterial = new Material()
        {
            PropetyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = Vector3.One * 0.8f,
            },
        };

        public Material()
        {
            PropetyBlock = new MaterialPropetyBlock();
        }

        public void LoadMapsAndInitSampler()
        {
            albedoMapView = null;
            if (!string.IsNullOrEmpty(AlbedoMapAsset)) {
                albedoMapView = AssetsLoader.LoadTextureSRV(AlbedoMapAsset, PropetyBlock.MetallicValue < 0);
            }

            normalMapView = null;
            if (!string.IsNullOrEmpty(NormalMapAsset))
            {
                normalMapView = AssetsLoader.LoadTextureSRV(NormalMapAsset);
            }

            roughnessMapView = null;
            if (!string.IsNullOrEmpty(RoughnessMapAsset))
            {
                roughnessMapView = AssetsLoader.LoadTextureSRV(RoughnessMapAsset);
            }

            metallicMapView = null;
            if (!string.IsNullOrEmpty(MetallicMapAsset))
            {
                metallicMapView = AssetsLoader.LoadTextureSRV(MetallicMapAsset);
            }

            occlusionMapView = null;
            if (!string.IsNullOrEmpty(OcclusionMapAsset))
            {
                occlusionMapView = AssetsLoader.LoadTextureSRV(OcclusionMapAsset);
            }

            //TODO: sampler selection
            MaterialSampler = SharedRenderItems.LinearWrapSamplerState;
        }

        public void Dispose() {
            //Disposed in asset loader
            /*albedoMapView?.Dispose();
            metallicMapView?.Dispose();
            roughnessMapView?.Dispose();
            normalMapView?.Dispose();
            occlusionMapView?.Dispose();*/
        }

        private static Material m_SkySphereMaterial;
        public static ShaderResourceView IrradianceMap;
        public static Material GetSkySphereMaterial() {
            if (m_SkySphereMaterial == null) {
                m_SkySphereMaterial = new Material();
                // MiraSkyboxCubeMap
                m_SkySphereMaterial.albedoMapView = AssetsLoader.LoadTextureSRV("SkyboxCubeMap", true);
                // MiraSkyboxIrradianceCubeMap
                IrradianceMap = AssetsLoader.LoadTextureSRV("SkyboxIrradianceCubeMap", true);
                m_SkySphereMaterial.MaterialSampler = SharedRenderItems.LinearClampSamplerState;
            }
            return m_SkySphereMaterial;
        }
    }
}
