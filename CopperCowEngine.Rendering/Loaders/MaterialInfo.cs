using System;

namespace CopperCowEngine.Rendering.Loaders
{
    public struct MaterialInfo
    {
        public Guid AssetGuid { get; }
        public uint Queue { get; }

        internal MaterialInfo(Guid assetGuid, uint queue)
        {
            AssetGuid = assetGuid;
            Queue = queue;
        }
    }
}