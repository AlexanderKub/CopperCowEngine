using CopperCowEngine.Core;
using System;
using System.Reflection;
using System.Text;

namespace CopperCowEngine.ScriptEngine.Lua
{
    public class LuaScriptEngine : IScriptEngine
    {
        private const string LuaDomain = "Lua Script";

        private readonly NLua.Lua _instance;

        private readonly StringBuilder _commandsList;

        public LuaScriptEngine()
        {
            _instance = new NLua.Lua();
            _commandsList = new StringBuilder();
            RegisterBuiltinFunctions();
        }

        public bool ExecuteScriptCommand(string command)
        {
            if (!command.Contains('('))
            {
                command += "();";
            }
            try 
            {
                var result = _instance.DoString(command);
                if (result == null)
                {
                    return true;
                }
                var resultText = string.Join(", ", result);
                Debug.Log(LuaDomain, resultText);
                return true;
            }
            catch (Exception e) 
            {
                Debug.Log(LuaDomain, e.ToString());
                return false;
            }
        }

        public void ExecuteScriptFile(string filePath)
        {
            try 
            {
                _instance.DoFile(filePath);
            }
            catch (Exception e) 
            {
                Debug.Log(LuaDomain, e.ToString());
            }
        }

        public void RegisterFunction(string command, MethodInfo methodInfo)
        {
            _commandsList.AppendLine(command);
            _instance.RegisterFunction(command, methodInfo);
        }

        public void RegisterFunction(string command, object target, MethodInfo methodInfo)
        {
            _commandsList.AppendLine(command);
            _instance.RegisterFunction(command, target, methodInfo);
        }

        public void RegisterFunction(string command, object target, string methodName)
        {
            _commandsList.AppendLine(command);
            _instance.RegisterFunction(command, target, target.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance));
        }

        private void RegisterBuiltinFunctions()
        {
            RegisterFunction("help", this, "Help");
            RegisterFunction("log", this, "Log");
        }
        
        private void Log(string line)
        {
            Debug.Log(LuaDomain, line);
        }

        private void Help()
        {
            Debug.Log(LuaDomain, $"Script Commands:\n{_commandsList}");
        }

        public void Dispose()
        {
            _instance.Dispose();
        }
    }
}
