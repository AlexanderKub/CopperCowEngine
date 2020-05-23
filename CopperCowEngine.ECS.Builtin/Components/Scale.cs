using System.Numerics;

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

    public struct NonUniformScale : IComponentData
    {
        public Vector3 Value;

        public static NonUniformScale Default = new NonUniformScale
        {
            Value = Vector3.One,
        };
    }
}
