using System;
using System.Collections.Generic;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.Loaders
{
    public static class MeshAssetsLoader
    {
        private static readonly Dictionary<Guid, ModelGeometry> CachedMeshes = new Dictionary<Guid, ModelGeometry>();
        private static readonly Dictionary<string, Guid> CachedMeshesGuidTable = new Dictionary<string, Guid>();

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CubeGuid = Guid.NewGuid();
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid SphereGuid = Guid.NewGuid();
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid LowPolySphereGuid = Guid.NewGuid();

        private static bool _isPrimitivesLoaded;

        private static void LoadPrimitives()
        {
            _isPrimitivesLoaded = true;
            CachedMeshes.Add(CubeGuid, Primitives.Cube);
            CachedMeshes.Add(SphereGuid, Primitives.Sphere(32));
            CachedMeshes.Add(LowPolySphereGuid, Primitives.Sphere(6));
        }

        public static Guid LoadMesh(string assetName)
        {
            if (!_isPrimitivesLoaded)
            {
                LoadPrimitives();
            }

            if (CachedMeshesGuidTable.ContainsKey(assetName))
            {
                return CachedMeshesGuidTable[assetName];
            }

            var meshAsset = AssetsManager.GetManager().LoadAsset<MeshAsset>(assetName);
            var modelGeometry = new ModelGeometry(meshAsset.FileScale, meshAsset.Pivot, meshAsset.Vertices,
                meshAsset.Indexes, meshAsset.BoundingMinimum, meshAsset.BoundingMaximum);
            
            var newGuid = Guid.NewGuid();
            CachedMeshesGuidTable.Add(assetName, newGuid);
            CachedMeshes.Add(newGuid, modelGeometry);

            return newGuid;
        }

        public static Guid GetGuid(string assetName)
        {
            return CachedMeshesGuidTable[assetName];
        }

        public static ModelGeometry GetMeshGeometry(Guid assetGuid)
        {
            return CachedMeshes.TryGetValue(assetGuid, out var instance) ? instance : null;
        }

        public static MeshInfo GetMeshInfo(PrimitivesMesh primitive)
        {
            ModelGeometry modelGeometry;
            Guid guid;

            switch (primitive)
            {
                case PrimitivesMesh.Cube:
                    guid = CubeGuid;
                    modelGeometry = Primitives.Cube;
                    break;
                case PrimitivesMesh.Sphere:
                    guid = SphereGuid;
                    modelGeometry = Primitives.Sphere(32);//20
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null);
            }
            return new MeshInfo(guid, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static MeshInfo GetMeshInfo(Guid assetGuid)
        {
            var modelGeometry = CachedMeshes[assetGuid];

            return new MeshInfo(assetGuid, new BoundsBox(modelGeometry.BoundingMinimum, modelGeometry.BoundingMaximum));
        }

        public static void DropCachedMesh(Guid assetGuid)
        {
            /*if (!CachedMeshes.ContainsKey(assetGuid))
            {
                return;
            }
            CachedMeshes[assetGuid] = null;
            CachedMeshes.Remove(assetGuid);*/
        }

    }
}
