using System.Numerics;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct DirectionalLight : IComponentData
    {
        public Vector3 Direction;

        public float Intensity;
    }
}
