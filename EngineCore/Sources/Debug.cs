using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;

using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;
using System.Linq;
using EngineCore.D3D11;

using EngineCore.ECS;
using EngineCore.ECS.Systems;
using EngineCore.ECS.Components;
using EngineCore.Utils;

namespace EngineCore
{
    public static class Debug
    {
        internal static EngineConsole EngineConsole;
        public static void Log(string format, params object[] args)
        {
            Log("Engine", string.Format(format, args));
        }

        public static void Log(string domain, string format, params object[] args)
        {
            Log(domain, string.Format(format, args));
        }

        public static void Log(string domain, string message)
        {
            string resLine = $"[{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            EngineConsole?.LogLine(resLine);
            Console.WriteLine(resLine);
        }

        public static void Warning(string domain, string message)
        {
            string resLine = $"WARNING! [{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            EngineConsole?.LogLine(resLine);
            Console.WriteLine(resLine);
        }

        public static void ScriptLog(string message)
        {
            Log("Lua", message);
        }
    }
}
