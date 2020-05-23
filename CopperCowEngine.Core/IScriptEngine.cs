using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Core
{
    public interface IScriptEngine
    {
        bool ExecuteScriptCommand(string command);

        void ExecuteScriptFile(string filePath);

        void RegisterFunction(string command, System.Reflection.MethodInfo methodInfo);

        void RegisterFunction(string command, object target, System.Reflection.MethodInfo methodInfo);

        void RegisterFunction(string command, object target, string methodName);

        void Dispose();
    }
}
