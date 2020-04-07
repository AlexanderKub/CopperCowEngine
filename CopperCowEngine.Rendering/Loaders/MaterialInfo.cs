namespace CopperCowEngine.Rendering.Loaders
{
    public enum PrimitivesMesh
    {
        Cube,
        Sphere,
    }

    public struct MaterialInfo
    {
        public string Name { get; }
        public int Queue { get; }

        internal MaterialInfo(string name, int queue)
        {
            Name = name;
            Queue = queue;
        }
    }
}