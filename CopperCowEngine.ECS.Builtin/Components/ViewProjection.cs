using System.Numerics;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct ViewProjection : IComponentData
    {
        public Matrix4x4 Value;
    }
}
