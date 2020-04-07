using System;
using System.Collections.Generic;
using System.Linq;
using AssetsManager.FSWorkers;
using AssetsManager.AssetsMeta;

namespace AssetsManager
{
    public partial class AssetsManagerInstance
    {
        private string _rootPath;

        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
                NativeFileSystemWorker.RootPath = _rootPath;
            }
        }

        internal NativeFileSystemWorker FileSystemWorker;
        private AssetsManagerInstance()
        {
            FileSystemWorker = new NativeFileSystemWorker();
            _instance = this;
        }

        private static AssetsManagerInstance _instance;
        public static AssetsManagerInstance GetManager()
        {
            return _instance ?? (_instance = new AssetsManagerInstance());
        }

        private readonly string[] _shaderExtensions = new string[] { "hlsl", };
        private readonly string[] _meshExtensions = new string[] { "obj", "fbx", };
        private readonly string[] _textureExtensions = new string[] { "jpg", "png", "bmp", };

        public bool ImportAsset(string path, string name)
        {
            return ImportAsset(path, name, false, out var dummy);
        }

        public bool ImportAsset(string path, string name, bool rewrite)
        {
            return ImportAsset(path, name, rewrite, out var dummy);
        }

        public bool ImportAsset(string path, string name, bool rewrite, out BaseAsset assetRes)
        {
            var arr = path.Split('.');
            var ext = arr[arr.Length - 1].ToLower();

            assetRes = null;
            BaseAsset asset;
            if (_shaderExtensions.Contains(ext))
            {
                return ImportShaderAsset(path, name, null, null, true, out assetRes);
            }

            if (_meshExtensions.Contains(ext))
            {
                asset = new MeshAsset()
                {
                    Name = name,
                };
            }
            else if (_textureExtensions.Contains(ext))
            {
                asset = new Texture2DAsset()
                {
                    Name = name,
                    // Hack for forcing SRGB image with wrong meta-data
                    ForceSRgb = name.Contains("Albedo"),
                };
            }
            else
            {
                Console.WriteLine("Unknown asset extension: {0}", ext);
                return false;
            }

            if (!asset.ImportAsset(path, ext))
            {
                return false;
            }
            assetRes = asset;
            return FileSystemWorker.CreateAssetFile(asset, rewrite || asset.Type == AssetTypes.Shader);
        }

        public bool ImportShaderAsset(string path, string name, string entryPoint, bool rewrite)
        {
            return ImportShaderAsset(path, name, entryPoint, null, rewrite, out var dummy);
        }

        public bool ImportShaderAsset(string path, string name, string entryPoint, string macro, bool rewrite)
        {
            return ImportShaderAsset(path, name, entryPoint, new Dictionary<string, object>() {
                { macro, 1 } }, rewrite, out var dummy);
        }

        public bool ImportShaderAsset(string path, string name, string entryPoint, string macro, string macro2, bool rewrite)
        {
            return ImportShaderAsset(path, name, entryPoint, new Dictionary<string, object>() {
                { macro, 1 }, { macro2, 1 } }, rewrite, out var dummy);
        }

        public bool ImportShaderAsset(string path, string name, string entryPoint, Dictionary<string, object> macro, bool rewrite)
        {
            return ImportShaderAsset(path, name, entryPoint, macro, rewrite, out var dummy);
        }

        public bool ImportShaderAsset(string path, string name, string entryPoint, Dictionary<string, object> macro, bool rewrite, out BaseAsset assetRes)
        {
            var arr = path.Split('.');
            var ext = arr[arr.Length - 1].ToLower();
            assetRes = null;

            if (!_shaderExtensions.Contains(ext))
            {
                return false;
            }

            ShaderTypeEnum st;
            if (name.EndsWith("VS"))
            {
                st = ShaderTypeEnum.Vertex;
            }
            else if (name.EndsWith("PS"))
            {
                st = ShaderTypeEnum.Pixel;
            }
            else if (name.EndsWith("GS"))
            {
                st = ShaderTypeEnum.Geometry;
            }
            else if (name.EndsWith("CS"))
            {
                st = ShaderTypeEnum.Compute;
            }
            else if (name.EndsWith("HS"))
            {
                st = ShaderTypeEnum.Hull;
            }
            else if (name.EndsWith("DS"))
            {
                st = ShaderTypeEnum.Domain;
            }
            else
            {
                Console.WriteLine("Unknown shader type, please add correct postfix e.g. VS");
                return false;
            }

            BaseAsset asset = new ShaderAsset()
            {
                Name = name,
                ShaderType = st,
                EntryPoint = entryPoint,
                Macro = macro,
            };

            if (!asset.ImportAsset(path, ext))
            {
                return false;
            }

            assetRes = asset;
            return FileSystemWorker.CreateAssetFile(asset, rewrite);
        }

        public bool CreateCubeMapAsset(string path, string name)
        {
            BaseAsset asset = new TextureCubeAsset()
            {
                Name = name,
            };

            var arr = path.Split('.');
            var ext = arr[arr.Length - 1].ToLower();
            if (_textureExtensions.Contains(ext))
            {
                return asset.ImportAsset(path, ext) && FileSystemWorker.CreateAssetFile(asset, true);
            }

            Console.WriteLine("Unknown asset extension: {0}", ext);
            return false;
        }

        public bool CreateMaterialAsset()
        {
            return FileSystemWorker.CreateAssetFile(new MaterialAsset()
            {
                Name = "NewMaterial",
            });
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
            return FileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool SaveAssetChanging(BaseAsset asset)
        {
            return FileSystemWorker.CreateAssetFile(asset, true);
        }

        public T LoadAsset<T>(string name) where T : BaseAsset, new()
        {
            var asset = new T
            {
                Name = name
            };
            FileSystemWorker.LoadAssetFile(asset);
            return asset;
        }

        public MetaAsset LoadMetaAsset(string name, AssetTypes type)
        {
            var asset = new MetaAsset {Name = name, InfoType = type};
            FileSystemWorker.LoadAssetFile(asset);
            return asset;
        }

        private Dictionary<AssetTypes, List<MetaAsset>> _cachedAssetsTable;

        public Dictionary<AssetTypes, List<MetaAsset>> LoadProjectAssets()
        {
            return LoadProjectAssets(true);
        }

        public Dictionary<AssetTypes, List<MetaAsset>> LoadProjectAssets(bool refresh)
        {
            if (!refresh && _cachedAssetsTable != null)
            {
                return _cachedAssetsTable;
            }
            _cachedAssetsTable = new Dictionary<AssetTypes, List<MetaAsset>>();

            for (var i = 0; i < Enum.GetNames(typeof(AssetTypes)).Length; i++)
            {
                var type = (AssetTypes)i;
                if (type == AssetTypes.Invalid || type == AssetTypes.Meta)
                {
                    continue;
                }

                var names = DetectAssetsNamesByType(type);
                var result = names.Select(name => LoadMetaAsset(name, type)).Where(asset => !asset.IsInvalid).ToList();
                _cachedAssetsTable.Add(type, result);
            }
            return _cachedAssetsTable;
        }

        private IEnumerable<string> DetectAssetsNamesByType(AssetTypes type)
        {
            var result = FileSystemWorker.DetectAssetsNamesByType(type);
            for (var i = 0; i < result.Length; i++)
            {
                var arr = result[i].Split('/');
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
            return FileSystemWorker.CreateAssetFile(asset, true);
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
            return FileSystemWorker.CreateAssetFile(asset, true);
        }

        public bool CreateMeshAsset(string path, string name, float fileScale)
        {
            var ext = path.Split('.').Last().ToLower();
            var asset = new MeshAsset()
            {
                Name = name,
            };
            asset.ImportAsset(path, ext, fileScale);
            return FileSystemWorker.CreateAssetFile(asset, true);
        }
    }

    #endregion

    #region PreRender Tools

    public partial class AssetsManagerInstance
    {
        public void CubeMapPrerender(string path, string outputName)
        {
            var cubeMapsPrerender = new Loaders.IBLMapsPreRender();
            cubeMapsPrerender.Init(path);
            cubeMapsPrerender.Render(outputName);
            cubeMapsPrerender.Dispose();
        }

        public void BrdfIntegrate(string outputName)
        {
            var cubeMapsPrerender = new Loaders.IBLMapsPreRender();
            cubeMapsPrerender.RenderBRDF(outputName);
            cubeMapsPrerender.Dispose();
            Console.WriteLine($"BRDFIntegrated: {outputName}");
        }
    }

    #endregion
}
