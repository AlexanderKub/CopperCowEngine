namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct LocalToParent : IComponentData
    {
        public System.Numerics.Matrix4x4 Value;
    }
}
