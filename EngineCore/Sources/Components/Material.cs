using EngineCore;
using EngineCore.D3D11;
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

    public class Material
    {
        public enum SamplerType
        {
            PointClamp,
            PointWrap,

            BilinearClamp,
            BilinearWrap,

            TrilinearClamp,
            TrilinearWrap,
        }

        // TODO: shader meta info (name, quequ)
        public ShaderGraph.MetaMaterial MetaMaterial { get; private set; }
        public int ShaderQueue => MetaMaterial.Queue;

        public string Name;
        public SamplerType TexturesSampler;
        internal SRITypeEnums.SamplerType GetSamplerType {
            get {
                return (SRITypeEnums.SamplerType)TexturesSampler;
            }
        }

        public bool HasSampler {
            get {
                return HasAlbedoMap || HasMetallicMap || HasNormalMap || HasOcclusionMap || HasRoughnessMap;
            }
        }

        // TODO: change to ShaderGraphMaterials by Meta material
        public bool HasAlbedoMap
        {
            get {
                return !string.IsNullOrEmpty(AlbedoMapAsset);
            }
        }

        public bool HasNormalMap {
            get {
                return !string.IsNullOrEmpty(NormalMapAsset);
            }
        }

        public bool HasRoughnessMap
        {
            get {
                return !string.IsNullOrEmpty(RoughnessMapAsset);
            }
        }
        
        public bool HasMetallicMap {
            get {
                return !string.IsNullOrEmpty(MetallicMapAsset);
            }
        }

        public bool HasOcclusionMap
        {
            get {
                return !string.IsNullOrEmpty(OcclusionMapAsset);
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
            Name = "DefaultMaterial",
            MetaMaterial = ShaderGraph.MetaMaterial.Standard,
            PropetyBlock = new MaterialPropetyBlock()
            {
                AlbedoColor = Vector3.One * 0.8f,
            },
            AlbedoMapAsset = "DebugTextureMap",
            TexturesSampler = SamplerType.BilinearWrap,
        };

        public Material()
        {
            MetaMaterial = ShaderGraph.MetaMaterial.Standard;
            PropetyBlock = new MaterialPropetyBlock();
        }

        public Material(ShaderGraph.MetaMaterial meta)
        {
            MetaMaterial = meta;
            PropetyBlock = new MaterialPropetyBlock();
        }

        private static Material m_SkySphereMaterial;
        public static Material GetSkySphereMaterial() {
            if (m_SkySphereMaterial == null) {
                m_SkySphereMaterial = new Material()
                {
                    Name = "SkySphereMaterial",
                    // SkyboxCubeMap MiraSkyboxCubeMap NightskySkyboxCubeMap
                    AlbedoMapAsset = "HouseCubeMap",
                    TexturesSampler = SamplerType.PointWrap,
                    MetaMaterial = new ShaderGraph.MetaMaterial()
                    {
                        shadingMode = ShaderGraph.MetaMaterial.ShadingMode.Unlit,
                        blendMode = ShaderGraph.MetaMaterial.BlendMode.Opaque,
                        cullMode = ShaderGraph.MetaMaterial.CullMode.Back,
                        materialDomain = ShaderGraph.MetaMaterial.MaterialDomain.Surface,
                        Wireframe = false,
                    }
                };
            }
            return m_SkySphereMaterial;
        }
    }
}
