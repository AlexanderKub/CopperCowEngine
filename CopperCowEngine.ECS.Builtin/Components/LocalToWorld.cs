namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct LocalToWorld : IComponentData
    {
        public System.Numerics.Matrix4x4 Value;
    }
}
