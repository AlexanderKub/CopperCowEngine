namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Rotation : IComponentData
    {
        public SharpDX.Quaternion Value;
    }
}
