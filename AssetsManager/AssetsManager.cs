using System;
using System.Collections.Generic;
using System.Linq;
using AssetsManager.FSWorkers;
using AssetsManager.AssetsMeta;

namespace AssetsManager
{
    public class AssetsManagerInstance
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

            //TODO: Asset Type detection
            assetRes = null;
            BaseAsset asset;
            if (shaderExts.Contains(ext)) {
                ShaderTypeEnum ST = ShaderTypeEnum.Vertex;
                if (Name.EndsWith("VS")) {
                    ST = ShaderTypeEnum.Vertex;
                } else if (Name.EndsWith("PS")) {
                    ST = ShaderTypeEnum.Pixel;
                } else if (Name.EndsWith("GS")) {
                    ST = ShaderTypeEnum.Geometry;
                } else if (Name.EndsWith("CS")) {
                    ST = ShaderTypeEnum.Compute;
                } else {
                    Console.WriteLine("Unknown shader type, please add correct postfix e.g. VS");
                    return false;
                }
                asset = new ShaderAsset() {
                    Name = Name,
                    ShaderType = ST,
                };
            } else if(meshExts.Contains(ext)) {
                asset = new MeshAsset() {
                    Name = Name,
                };
            } else if (textureExts.Contains(ext)) {
                asset = new Texture2DAsset() {
                    Name = Name,
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
            return FSWorker.CreateAssetFile(asset);
        }

        public bool CreateMaterialAsset() {
            BaseAsset asset = new MaterialAsset() {
                Name = "NewMaterial",
            };
            return FSWorker.CreateAssetFile(asset);
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
}
