using System;
using System.Collections.Generic;
using System.Linq;
using AssetsManager.FSWorkers;
using AssetsManager.AssetsMeta;

namespace AssetsManager
{
    public partial class AssetsManagerInstance
    {
        private string m_RootPath;
        public string RootPath {
            get {
                return m_RootPath;
            }
            set {
                m_RootPath = value;
                NativeFSWorker.RootPath = m_RootPath;
            }
        }

        internal NativeFSWorker FSWorker;
        private AssetsManagerInstance() {
            FSWorker = new NativeFSWorker();
            m_Instance = this;
        }

        private static AssetsManagerInstance m_Instance;
        public static AssetsManagerInstance GetManager() {
            if (m_Instance == null) {
                m_Instance = new AssetsManagerInstance();
            }
            return m_Instance;
        }

        private string[] shaderExts = new string[] { "hlsl", };
        private string[] meshExts = new string[] { "obj", "fbx", };
        private string[] textureExts = new string[] { "jpg", "png", "bmp", };

        public bool ImportAsset(string Path, string Name) {
            BaseAsset dummy;
            return ImportAsset(Path, Name, false, out dummy);
        }

        public bool ImportAsset(string Path, string Name, bool Rewrite) {
            BaseAsset dummy;
            return ImportAsset(Path, Name, Rewrite, out dummy);
        }

        public bool ImportAsset(string Path, string Name, bool Rewrite, out BaseAsset assetRes) {
            string[] arr = Path.Split('.');
            string ext = arr[arr.Length - 1].ToLower();

            assetRes = null;
            BaseAsset asset;
            if (shaderExts.Contains(ext)) {
                return ImportShaderAsset(Path, Name, null, null, true, out assetRes);
            } else if(meshExts.Contains(ext)) {
                asset = new MeshAsset() {
                    Name = Name,
                };
            } else if (textureExts.Contains(ext)) {
                asset = new Texture2DAsset() {
                    Name = Name,
                    // Hack for forcing srgb image with wrong meta-data
                    ForceSRgb = Name.Contains("Albedo"),
                };
            } else {
                Console.WriteLine("Unknown asset extension: {0}", ext);
                return false;
            }

            if (!asset.ImportAsset(Path, ext)) {
                return false;
            }
            assetRes = asset;
            return FSWorker.CreateAssetFile(asset, Rewrite || asset.Type == AssetTypes.Shader);
        }

        public bool ImportShaderAsset(string Path, string Name, string EntryPoint, bool Rewrite)
        {
            BaseAsset dummy;
            return ImportShaderAsset(Path, Name, EntryPoint, null, Rewrite, out dummy);
        }

        public bool ImportShaderAsset(string Path, string Name, string EntryPoint, string Macro, bool Rewrite)
        {
            BaseAsset dummy;
            return ImportShaderAsset(Path, Name, EntryPoint, new Dictionary<string, object>() {
                { Macro, 1 } }, Rewrite, out dummy);
        }

        public bool ImportShaderAsset(string Path, string Name, string EntryPoint, string Macro, string Macro2, bool Rewrite)
        {
            BaseAsset dummy;
            return ImportShaderAsset(Path, Name, EntryPoint, new Dictionary<string, object>() {
                { Macro, 1 }, { Macro2, 1 } }, Rewrite, out dummy);
        }

        public bool ImportShaderAsset(string Path, string Name, string EntryPoint, Dictionary<string, object> Macro, bool Rewrite)
        {
            BaseAsset dummy;
            return ImportShaderAsset(Path, Name, EntryPoint, Macro, Rewrite, out dummy);
        }

        public bool ImportShaderAsset(string Path, string Name, string EntryPoint, Dictionary<string,object> Macro, bool Rewrite, out BaseAsset assetRes)
        {
            string[] arr = Path.Split('.');
            string ext = arr[arr.Length - 1].ToLower();
            assetRes = null;

            if (!shaderExts.Contains(ext))
            {
                return false;
            }

            BaseAsset asset;
            
            ShaderTypeEnum ST = ShaderTypeEnum.Vertex;
            if (Name.EndsWith("VS")) {
                ST = ShaderTypeEnum.Vertex;
            } else if (Name.EndsWith("PS")) {
                ST = ShaderTypeEnum.Pixel;
            } else if (Name.EndsWith("GS")) {
                ST = ShaderTypeEnum.Geometry;
            } else if (Name.EndsWith("CS")) {
                ST = ShaderTypeEnum.Compute;
            } else if (Name.EndsWith("HS")) {
                ST = ShaderTypeEnum.Hull;
            } else if (Name.EndsWith("DS")) {
                ST = ShaderTypeEnum.Domain;
            } else {
                Console.WriteLine("Unknown shader type, please add correct postfix e.g. VS");
                return false;
            }

            asset = new ShaderAsset() {
                Name = Name,
                ShaderType = ST,
                EntryPoint = EntryPoint,
                Macro = Macro,
            };

            if (!asset.ImportAsset(Path, ext)) {
                return false;
            }

            assetRes = asset;
            return FSWorker.CreateAssetFile(asset, Rewrite);
        }

        public bool CreateCubeMapAsset(string Path, string Name) {
            BaseAsset asset = new TextureCubeAsset() {
                Name = Name,
            };
            string[] arr = Path.Split('.');
            string ext = arr[arr.Length - 1].ToLower();
            if (!textureExts.Contains(ext)) {
                Console.WriteLine("Unknown asset extension: {0}", ext);
                return false;
            }
            if (!asset.ImportAsset(Path, ext)) {
                return false;
            }
            return FSWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMaterialAsset() {
            BaseAsset asset = new MaterialAsset() {
                Name = "NewMaterial",
            };
            return FSWorker.CreateAssetFile(asset);
        }

        //FOR TESTING
        public bool CreateShaderGraphAsset(string name, SharpDX.D3DCompiler.ShaderBytecode bytecode)
        {
            BaseAsset asset = new ShaderAsset()
            {
                Name = name,
                ShaderType = ShaderTypeEnum.Pixel,
                Bytecode = bytecode,
            };
            return FSWorker.CreateAssetFile(asset, true);
        }

        public bool SaveAssetChanging(BaseAsset asset) {
            return FSWorker.CreateAssetFile(asset, true);
        }

        public T LoadAsset<T>(string Name) where T : BaseAsset {
            T asset = Activator.CreateInstance<T>();
            asset.Name = Name;
            FSWorker.LoadAssetFile(asset);
            return asset;
        }

        public MetaAsset LoadMetaAsset(string Name, AssetTypes Type) {
            MetaAsset asset = new MetaAsset();
            asset.Name = Name;
            asset.InfoType = Type;
            FSWorker.LoadAssetFile(asset);
            return asset;
        }

        private Dictionary<AssetTypes, List<MetaAsset>> CachedAssetsTable;
        public Dictionary<AssetTypes, List<MetaAsset>> LoadProjectAssets() {
            return LoadProjectAssets(true);
        }

        public Dictionary<AssetTypes, List<MetaAsset>> LoadProjectAssets(bool refresh) {
            if (!refresh && CachedAssetsTable != null) {
                return CachedAssetsTable;
            }
            CachedAssetsTable = new Dictionary<AssetTypes, List<MetaAsset>>();

            AssetTypes type;
            for (int i = 0; i < Enum.GetNames(typeof(AssetTypes)).Length; i++) {
                type = (AssetTypes)i;
                if (type == AssetTypes.Invalid || type == AssetTypes.Meta) {
                    continue;
                }
                List<MetaAsset> result = new List<MetaAsset>();
                string[] names = DetectAssetsNamesByType(type);
                foreach (string name in names) {
                    MetaAsset asset = LoadMetaAsset(name, type);
                    if (!asset.IsInvalid) {
                        result.Add(asset);
                    }
                }
                CachedAssetsTable.Add(type, result);
            }
            return CachedAssetsTable;
        }

        private string[] DetectAssetsNamesByType(AssetTypes type) {
            string[] result = FSWorker.DetectAssetsNamesByType(type);
            for (int i = 0; i < result.Length; i++) {
                string[] arr = result[i].Split('/');
                arr = arr[arr.Length - 1].Split('.');
                result[i] = arr[0];
            }
            return result;
        }

    }

    #region Debug Asset Creators

    public partial class AssetsManagerInstance
    {
        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset)
        {
            BaseAsset asset = new MaterialAsset()
            {
                Name = name,
                AlbedoMapAsset = albedoAsset,
                NormalMapAsset = normalAsset,
                MetallicValue = 0.05f,
                RoughnessValue = 0.95f,
            };
            return FSWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset, string roughnessAsset, string metallicAsset)
        {
            return CreateMaterialAsset(name, albedoAsset, normalAsset, roughnessAsset, metallicAsset, "");
        }

        public bool CreateMaterialAsset(string name, string albedoAsset, string normalAsset, string roughnessAsset, string metallicAsset, string occlusionAsset)
        {
            BaseAsset asset = new MaterialAsset()
            {
                Name = name,
                AlbedoMapAsset = albedoAsset,
                NormalMapAsset = normalAsset,
                RoughnessMapAsset = roughnessAsset,
                MetallicMapAsset = metallicAsset,
                OcclusionMapAsset = occlusionAsset,
            };
            return FSWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMeshAsset(string path, string name, float fileScale)
        {
            string ext = path.Split('.').Last().ToLower();
            MeshAsset asset = new MeshAsset()
            {
                Name = name,
            };
            bool r = asset.ImportAsset(path, ext, fileScale);
            return FSWorker.CreateAssetFile(asset, true);
        }
    }

    #endregion

    #region PreRender Tools

    public partial class AssetsManagerInstance
    {
        public void CubeMapPrerender(string path, string outputName)
        {
            Loaders.IBLMapsPreRender cubeMapsPrerender = new Loaders.IBLMapsPreRender();
            cubeMapsPrerender.Init(path);
            cubeMapsPrerender.Render(outputName);
            cubeMapsPrerender.Dispose();
        }

        public void BRDFIntegrate(string outputName)
        {
            Loaders.IBLMapsPreRender cubeMapsPrerender = new Loaders.IBLMapsPreRender();
            cubeMapsPrerender.RenderBRDF(outputName);
            cubeMapsPrerender.Dispose();
            Console.WriteLine($"BRDFIntegrated: {outputName}");
        }
    }

    #endregion
}
