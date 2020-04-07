using SharpDX;

namespace CopperCowEngine.Rendering.Data
{
    public class MaterialInstance
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
        public ShaderGraph.MaterialMeta MetaMaterial { get; private set; }

        public int ShaderQueue => MetaMaterial.Queue;

        public string Name;

        public SamplerType TexturesSampler;

        public bool HasSampler => HasAlbedoMap || HasMetallicMap || HasNormalMap || HasOcclusionMap || HasRoughnessMap;

        // TODO: change to ShaderGraphMaterials by Meta material
        public bool HasAlbedoMap => !string.IsNullOrEmpty(AlbedoMapAsset);

        public bool HasNormalMap => !string.IsNullOrEmpty(NormalMapAsset);

        public bool HasRoughnessMap => !string.IsNullOrEmpty(RoughnessMapAsset);

        public bool HasMetallicMap => !string.IsNullOrEmpty(MetallicMapAsset);

        public bool HasOcclusionMap => !string.IsNullOrEmpty(OcclusionMapAsset);

        public string AlbedoMapAsset;
        public string NormalMapAsset;
        public string RoughnessMapAsset;
        public string MetallicMapAsset;
        public string OcclusionMapAsset;

        public MaterialPropertyBlock PropertyBlock;

        public static MaterialInstance DefaultMaterial = new MaterialInstance()
        {
            Name = "DefaultMaterial",
            MetaMaterial = ShaderGraph.MaterialMeta.Standard,
            PropertyBlock = new MaterialPropertyBlock(MaterialPropertyBlock.Default)
            {
                AlbedoColor = Vector3.One * 0.8f,
            },
            AlbedoMapAsset = "DebugTextureMap",
            TexturesSampler = SamplerType.BilinearWrap,
        };

        public MaterialInstance()
        {
            MetaMaterial = ShaderGraph.MaterialMeta.Standard;
            PropertyBlock = MaterialPropertyBlock.Default;
        }

        public MaterialInstance(ShaderGraph.MaterialMeta meta)
        {
            MetaMaterial = meta;
            PropertyBlock = MaterialPropertyBlock.Default;
        }

        private static MaterialInstance _skySphereMaterial;

        public static MaterialInstance GetSkySphereMaterial()
        {
            return _skySphereMaterial ?? (_skySphereMaterial = new MaterialInstance()
            {
                Name = "SkySphereMaterial",
                // SkyboxCubeMap MiraSkyboxCubeMap NightskySkyboxCubeMap
                AlbedoMapAsset = "HouseCubeMap",
                TexturesSampler = SamplerType.PointWrap,
                MetaMaterial = new ShaderGraph.MaterialMeta()
                {
                    ShadingMode = ShaderGraph.MaterialMeta.ShadingModeType.Unlit,
                    BlendMode = ShaderGraph.MaterialMeta.BlendModeType.Opaque,
                    CullMode = ShaderGraph.MaterialMeta.CullModeType.Back,
                    MaterialDomain = ShaderGraph.MaterialMeta.MaterialDomainType.Surface,
                    Wireframe = false,
                }
            });
        }
    }
}
