namespace CopperCowEngine.Rendering
{
    public struct RenderingConfiguration
    {
        public string AppName;

        public bool DebugMode;

        public bool EnableHdr;

        public MsaaEnabled EnableMsaa;

        public bool InteropDisplay;

        public RenderPathType RenderPath;

        public static RenderingConfiguration Default = new RenderingConfiguration
        {
            AppName = "CopperCowEngine",
            DebugMode = false,
            EnableHdr = false,
            EnableMsaa = MsaaEnabled.X4,
            RenderPath = RenderPathType.Forward
        };
    }
}