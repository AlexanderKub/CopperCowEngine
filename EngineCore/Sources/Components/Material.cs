using SharpDX;
using SharpDX.Direct3D11;

namespace EngineCore
{
    public class MaterialPropetyBlock
    {
        public Vector3 AlbedoColor = Vector3.One;
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
            bool needSampler = false;

            albedoMapView?.Dispose();
            albedoMapView = null;
            if (!string.IsNullOrEmpty(AlbedoMapAsset))
            {
                Texture2D albedoMap = PropetyBlock.MetallicValue >= 0 ?
                       AssetsLoader.LoadTexture(AlbedoMapAsset) : AssetsLoader.LoadCubeTexture(AlbedoMapAsset);
                if (albedoMap != null) {
                    albedoMapView = new ShaderResourceView(Engine.Instance.Device, albedoMap);
                    Engine.Instance.Context.GenerateMips(albedoMapView);
                    needSampler = true;
                }
            }

            normalMapView?.Dispose();
            normalMapView = null;
            if (!string.IsNullOrEmpty(NormalMapAsset))
            {
                Texture2D normalMap = AssetsLoader.LoadTexture(NormalMapAsset);
                if (normalMap != null) {
                    normalMapView = new ShaderResourceView(Engine.Instance.Device, normalMap);
                    Engine.Instance.Context.GenerateMips(normalMapView);
                    needSampler = true;
                }
            }

            roughnessMapView?.Dispose();
            roughnessMapView = null;
            if (!string.IsNullOrEmpty(RoughnessMapAsset))
            {
                Texture2D roughnessMap = AssetsLoader.LoadTexture(RoughnessMapAsset);
                if (roughnessMap != null) {
                    roughnessMapView = new ShaderResourceView(Engine.Instance.Device, roughnessMap);
                    Engine.Instance.Context.GenerateMips(roughnessMapView);
                    needSampler = true;
                }
            }

            metallicMapView?.Dispose();
            metallicMapView = null;
            if (!string.IsNullOrEmpty(MetallicMapAsset))
            {
                Texture2D metallicMap = AssetsLoader.LoadTexture(MetallicMapAsset);
                if (metallicMap != null) {
                    metallicMapView = new ShaderResourceView(Engine.Instance.Device, metallicMap);
                    Engine.Instance.Context.GenerateMips(metallicMapView);
                    needSampler = true;
                }
            }

            occlusionMapView?.Dispose();
            occlusionMapView = null;
            if (!string.IsNullOrEmpty(OcclusionMapAsset))
            {
                Texture2D ambientOcclusionMap = AssetsLoader.LoadTexture(OcclusionMapAsset);
                if (ambientOcclusionMap != null) {
                    occlusionMapView = new ShaderResourceView(Engine.Instance.Device, ambientOcclusionMap);
                    Engine.Instance.Context.GenerateMips(occlusionMapView);
                    needSampler = true;
                }
            }

            if (!needSampler)
            {
                return;
            }

            MaterialSampler = new SamplerState(Engine.Instance.Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });
        }

        public void Dispose() {
            albedoMapView?.Dispose();
            metallicMapView?.Dispose();
            roughnessMapView?.Dispose();
            normalMapView?.Dispose();
            occlusionMapView?.Dispose();
        }

        private static Material m_SkySphereMaterial;
        public static ShaderResourceView IrradianceMap;
        public static Material GetSkySphereMaterial() {
            if (m_SkySphereMaterial == null) {
                m_SkySphereMaterial = new Material();
                Texture2D cubeMap = AssetsLoader.LoadCubeTexture("SkyboxCubeMap");
                m_SkySphereMaterial.albedoMapView = new ShaderResourceView(Engine.Instance.Device, cubeMap);
                Engine.Instance.Context.GenerateMips(m_SkySphereMaterial.albedoMapView);
                
                Texture2D envMap = AssetsLoader.LoadCubeTexture("SkyboxIrradianceCubeMap");
                IrradianceMap = new ShaderResourceView(Engine.Instance.Device, envMap);
                Engine.Instance.Context.GenerateMips(IrradianceMap);

                m_SkySphereMaterial.MaterialSampler = new SamplerState(Engine.Instance.Device, new SamplerStateDescription() {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    ComparisonFunction = Comparison.Never,
                    MaximumAnisotropy = 16,
                    MipLodBias = 0,
                    MinimumLod = -float.MaxValue,
                    MaximumLod = float.MaxValue
                });
            }
            return m_SkySphereMaterial;
        }
    }
}
