using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.Loaders
{
    public class MeshAssetsLoader
    {
        private static readonly Dictionary<string, ModelGeometry> CachedMeshes = new Dictionary<string, ModelGeometry>();

        public static ModelGeometry LoadMesh(string assetName)
        {
            if (CachedMeshes.ContainsKey(assetName))
            {
                return CachedMeshes[assetName];
            }

            var meshAsset = AssetsManager.GetManager().LoadAsset<MeshAsset>(assetName);
            var modelGeometry = new ModelGeometry(meshAsset.FileScale, meshAsset.Pivot, meshAsset.Vertices,
                meshAsset.Indexes, meshAsset.BoundingMinimum, meshAsset.BoundingMaximum);
            CachedMeshes.Add(assetName, modelGeometry);
            return modelGeometry;
        }

        public static MeshInfo LoadMeshInfo(PrimitivesMesh primitive)
        {
            ModelGeometry modelGeometry;
            string name;

            switch (primitive)
            {
                case PrimitivesMesh.Cube:
                    name = "Primitives.Cube";
                    modelGeometry = Primitives.Cube;
                    break;
                case PrimitivesMesh.Sphere:
                    name = "Primitives.Sphere";
                    modelGeometry = Primitives.Sphere(32);//20
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null);
            }
            return new MeshInfo(name, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static MeshInfo LoadMeshInfo(string assetName)
        {
            var modelGeometry = LoadMesh(assetName);
            return new MeshInfo(assetName, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static void DropCachedMesh(string assetName)
        {
            if (!CachedMeshes.ContainsKey(assetName))
            {
                return;
            }
            CachedMeshes[assetName] = null;
            CachedMeshes.Remove(assetName);
        }
    }
}
