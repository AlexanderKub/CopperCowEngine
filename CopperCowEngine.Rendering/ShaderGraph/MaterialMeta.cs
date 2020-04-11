namespace CopperCowEngine.Rendering.ShaderGraph
{
    public sealed class MaterialMeta
    {
        public MaterialDomainType MaterialDomain = MaterialDomainType.Surface;

        public BlendModeType BlendMode = BlendModeType.Opaque; // States and shader template

        public ShadingModeType ShadingMode = ShadingModeType.Default; // Layouts and shader template

        public CullModeType CullMode = CullModeType.Back; // States

        public bool Wireframe = false;

        public float OpacityMaskClipValue = 0.3333f;

        public int Queue => (int)BlendMode + (int)ShadingMode + (int)CullMode + (Wireframe ? 100 : 200);

        public static MaterialMeta Standard = new MaterialMeta();

        public enum MaterialDomainType : byte
        {
            Surface,
        }

        public enum BlendModeType : uint
        {
            Opaque = 100000,
            Masked = 200000,
            Translucent = 300000,
            Additive = 400000,
            Modulate = 500000,
        }

        public enum ShadingModeType : uint
        {
            Unlit = 10000,
            Default = 20000,
        }

        public enum CullModeType : uint
        {
            Front = 1000,
            Back = 2000,
            None = 3000,
        }
    }
}
