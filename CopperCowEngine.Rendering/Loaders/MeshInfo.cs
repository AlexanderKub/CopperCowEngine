using System;

namespace CopperCowEngine.Rendering.Loaders
{
    public enum PrimitivesMesh : byte
    {
        Cube,
        Sphere,
    }

    public struct MeshInfo
    {
        public Guid AssetGuid { get; }
        public BoundsBox Bounds { get; }

        internal MeshInfo(Guid assetGuid, BoundsBox bounds)
        {
            AssetGuid = assetGuid;
            Bounds = bounds;
        }
    }
}
