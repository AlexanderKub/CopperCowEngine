using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CopperCowEngine.Rendering;

namespace CopperCowEngine.Core
{
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
