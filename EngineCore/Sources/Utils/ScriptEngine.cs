using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace EngineCore {
    public class ScriptEngine {
        public Lua LuaInstance;

        public ScriptEngine() {
            LuaInstance = new Lua();
            LuaInstance.LoadCLRPackage();
            RegisterFunctions();
            ExecuteScriptLine(@"DebugLog('Lua script engine loaded');");
        }

        private void RegisterFunctions()
        {
            RegisterFunction("Help", this, typeof(ScriptEngine).GetMethod("Help"));
            RegisterFunction("DebugLog", Engine.Instance, typeof(Engine).GetMethod("Log"));
            RegisterFunction("Quit", Engine.Instance, typeof(Engine).GetMethod("Quit"));
            RegisterFunction("DebugRender", Engine.Instance.RendererTechnique, typeof(BaseRendererTechnique).GetMethod("SetDebug"));
            RegisterFunction("RenderIndex", Engine.Instance.RendererTechnique, typeof(BaseRendererTechnique).GetMethod("SetDebugIndex"));
            RegisterFunction("WireframeRender", Engine.Instance, typeof(Engine).GetMethod("SetWireframeRender"));
            RegisterFunction("SolidRender", Engine.Instance, typeof(Engine).GetMethod("SetSolidRender"));
            RegisterFunction("FPS", Engine.Instance.UIConsoleInstance, typeof(UIConsole).GetMethod("ToggleFPSCounter"));
        }

        public string ExecuteScriptLine(string chunk) {
            if (chunk.IndexOf('(') == -1)
            {
                chunk += "();";
            }
            try {
                LuaInstance.DoString(chunk);
                return "Ok";
            }
            catch (Exception e) {
                Engine.Log(e.ToString());
                return e.ToString();
            }
        }

        public void ExecuteScriptFile(string fileName) {
            try {
                LuaInstance.DoFile(fileName);
            }
            catch (Exception e) {
                Engine.Log(e.ToString());
            }
        }

        private string MethodList = "Commands list:\n";
        private void RegisterFunction(string name, object target, System.Reflection.MethodInfo methodInfo)
        {
            MethodList += name + "\n";
            LuaInstance.RegisterFunction(name, target, methodInfo);
        }

        public void Help()
        {
            Engine.Log(MethodList);
        }
    }
}
