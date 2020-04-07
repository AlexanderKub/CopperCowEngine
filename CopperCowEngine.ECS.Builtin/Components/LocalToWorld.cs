namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct LocalToWorld : IComponentData
    {
        public SharpDX.Matrix Value;
    }
}
