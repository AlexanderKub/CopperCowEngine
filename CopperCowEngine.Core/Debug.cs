using System;

namespace CopperCowEngine.Core
{
    public static class Debug
    {
        public static void Log(string format, params object[] args)
        {
            Log("Engine", string.Format(format, args));
        }

        public static void ScriptLog(string message)
        {
            Log("Lua", message);
        }

        public static void Log(string domain, string format, params object[] args)
        {
            Log(domain, string.Format(format, args));
        }

        public static void Log(string domain, string message)
        {
            var resLine = $"[{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            InternalLog(resLine);
        }

        public static void Warning(string domain, string message)
        {
            var resLine = $"WARNING! [{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            InternalLog(resLine);
        }

        public static void Error(string domain, string message)
        {
            var resLine = $"ERROR! [{DateTime.Now.ToLocalTime()} {domain}]: {message}";
            InternalLog(resLine);
        }

        private static void InternalLog(string message)
        {
            // TODO: Engine console
            Console.WriteLine(message);
        }
    }
}
