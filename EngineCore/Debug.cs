using System;
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
            var resLine = $"[{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            EngineConsole?.LogLine(resLine);
            Console.WriteLine(resLine);
        }

        public static void Warning(string domain, string message)
        {
            var resLine = $"WARNING! [{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            EngineConsole?.LogLine(resLine);
            Console.WriteLine(resLine);
        }

        public static void ScriptLog(string message)
        {
            Log("Lua", message);
        }
    }
}
