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

        //uint max: 4,294,967,295
        public uint Queue => (uint)BlendMode + (uint)ShadingMode + (uint)CullMode + (uint)(Wireframe ? 100000 : 200000);

        public static readonly MaterialMeta Standard = new MaterialMeta();

        public enum MaterialDomainType : byte
        {
            Surface,
        }

        public enum BlendModeType : uint
        {
            Opaque = 100000000,
            Masked = 200000000,
            Translucent = 300000000,
            Additive = 400000000,
            Modulate = 500000000,
        }

        public enum ShadingModeType : uint
        {
            Unlit = 10000000,
            Default = 20000000,
        }

        public enum CullModeType : uint
        {
            Front = 1000000,
            Back = 2000000,
            None = 3000000,
        }
    }
}
