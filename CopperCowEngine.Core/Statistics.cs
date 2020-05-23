using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CopperCowEngine.Rendering")]
namespace CopperCowEngine.Core
{
    public static class Statistics
    {
        public static int DrawCallsCount { get; private set; }

        internal static void DrawCall()
        {
            DrawCallsCount++;
        }

        internal static void Flush()
        {
            DrawCallsCount = 0;
        }
    }
}
