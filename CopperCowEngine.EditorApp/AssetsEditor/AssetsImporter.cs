using System;
using System.Linq;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.Rendering.D3D11.Editor;

namespace CopperCowEngine.EditorApp.AssetsEditor
{
    internal static class AssetsImporter
    {
        
        private static readonly string[] MeshExtensions = { "obj", "fbx", };
        private static readonly string[] TextureExtensions = { "jpg", "png", "bmp", };

        public static bool ImportAsset(string path, string name, bool rewrite = false)
        {
            return ImportAsset(path, name, rewrite, out _);
        }

        public static bool ImportAsset(string path, string name, bool rewrite, out BaseAsset assetRes)
        {
            var arr = path.Split('.');
            var ext = arr[^1].ToLower();

            assetRes = null;

            if (MeshExtensions.Contains(ext))
            {
                if (!ImportMesh(path, name))
                {
                    return false;
                }
                assetRes = AssetsManager.GetManager().LoadAsset<MeshAsset>(name);
                return true;
            }
            else if (TextureExtensions.Contains(ext))
            {
                // Hack for forcing SRGB image with wrong meta-data
                if (!ImportTexture2D(path, name, name.Contains("Albedo")))
                {
                    return false;
                }
                assetRes = AssetsManager.GetManager().LoadAsset<MeshAsset>(name);
                return true;
            }
            else
            {
                Console.WriteLine($"Unknown asset extension: {ext}");
                return false;
            }
        }

        private static bool ImportMesh(string name, string path)
        {
            return EditorAssetsManager.GetManager().CreateMeshAsset(path, name);
        }

        private static bool ImportTexture2D(string name, string path, bool forceSRgb)
        {
            try
            {
                D3D11AssetsImporter.ImportTexture(name, path, forceSRgb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
    }
}
