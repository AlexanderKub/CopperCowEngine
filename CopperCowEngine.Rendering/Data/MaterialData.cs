using System;
using System.Numerics;

namespace CopperCowEngine.Rendering.Data
{
    public class MaterialInstance
    {
        // TODO: shader meta info (name, quequ)
        public ShaderGraph.MaterialMeta MetaMaterial { get; private set; }

        public uint ShaderQueue => MetaMaterial.Queue + (uint)(HasSampler ? 20000 : 10000);

        public string Name;

        public SamplerType TexturesSampler = SamplerType.BilinearWrap;

        public bool HasSampler => HasAlbedoMap || HasMetallicMap || HasNormalMap || HasOcclusionMap || HasRoughnessMap || HasEmissiveMap;

        // TODO: change to ShaderGraphMaterials by Meta material
        public bool HasAlbedoMap => !string.IsNullOrEmpty(AlbedoMapAsset);

        public bool HasNormalMap => !string.IsNullOrEmpty(NormalMapAsset);

        public bool HasRoughnessMap => !string.IsNullOrEmpty(RoughnessMapAsset);

        public bool HasMetallicMap => !string.IsNullOrEmpty(MetallicMapAsset);

        public bool HasOcclusionMap => !string.IsNullOrEmpty(OcclusionMapAsset);

        public bool HasEmissiveMap => !string.IsNullOrEmpty(EmissiveMapAsset);

        public string AlbedoMapAsset;
        public string EmissiveMapAsset;
        public string MetallicMapAsset;
        public string NormalMapAsset;
        public string OcclusionMapAsset;
        public string RoughnessMapAsset;

        public MaterialPropertyBlock PropertyBlock;

        public static readonly MaterialInstance DefaultMaterial = new MaterialInstance()
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
        
        private static readonly Guid SkySphereMaterialGuid = Guid.NewGuid();

        public static bool IsSkySphereMaterial(Guid materialGuid)
        {
            return materialGuid == SkySphereMaterialGuid;
        }

        public static bool IsSkySphereMaterial(MaterialInstance materialInstance)
        {
            return materialInstance.Name == "SkySphereMaterial";
        }

        public static MaterialInstance GetSkySphereMaterial()
        {
            return _skySphereMaterial ??= new MaterialInstance()
            {
                Name = "SkySphereMaterial",
                // SkyboxCubeMap MiraSkyboxCubeMap NightskySkyboxCubeMap
                //AlbedoMapAsset = "HouseCubeMap",
                AlbedoMapAsset = "TokioPreFilteredEnvMap",
                EmissiveMapAsset = "TokioIrradianceEnvMap",
                TexturesSampler = SamplerType.PointWrap,
                MetaMaterial = new ShaderGraph.MaterialMeta()
                {
                    ShadingMode = ShaderGraph.MaterialMeta.ShadingModeType.Unlit,
                    BlendMode = ShaderGraph.MaterialMeta.BlendModeType.Opaque,
                    CullMode = ShaderGraph.MaterialMeta.CullModeType.Back,
                    MaterialDomain = ShaderGraph.MaterialMeta.MaterialDomainType.Surface,
                    Wireframe = false,
                }
            };
        }

        public enum SamplerType : byte
        {
            PointClamp,
            PointWrap,

            BilinearClamp,
            BilinearWrap,

            TrilinearClamp,
            TrilinearWrap,
        }
    }
}
