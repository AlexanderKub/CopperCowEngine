namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct Scale : IComponentData
    {
        public float Value;

        public static Scale Default = new Scale
        {
            Value = 1,
        };
    }
}
