using System;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Mesh : IComponentData
    {
        public Guid AssetGuid;

        public BoundsBox Bounds;
    }
}
