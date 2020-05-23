namespace CopperCowEngine.Core
{
    public class ScreenProperties
    {
        public float AspectRatio;
        public int Height;
        public int Width;
    }

    public static class Screen
    {
        public static float AspectRatio { get; private set; }

        public static int Width { get; private set; }

        public static int Height { get; private set; }

        internal static void SetScreenProperties(ScreenProperties properties)
        {
            Width = properties.Width;

            Height = properties.Height;

            AspectRatio = properties.AspectRatio;
        }
    }
}
