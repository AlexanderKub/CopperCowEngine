using System;

namespace CopperCowEngine.Core
{
    public static class Debug
    {
        public static event Action<string> OnDebugLog;

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
            Console.WriteLine(message);
            OnDebugLog?.Invoke(message);
        }
    }
}
