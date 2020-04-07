namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct LocalToParent : IComponentData
    {
        public SharpDX.Matrix Value;
    }
}
