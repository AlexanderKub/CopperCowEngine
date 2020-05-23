using System;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Material : IComponentData
    {
        public Guid AssetGuid;

        public uint Queue;
    }
}
