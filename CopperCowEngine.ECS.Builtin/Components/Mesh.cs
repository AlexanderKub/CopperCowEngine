using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Mesh : IComponentData
    {
        public string Name;

        public BoundsBox Bounds;
    }
}
