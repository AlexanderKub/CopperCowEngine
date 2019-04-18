using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace EngineCore {
    public class ScriptEngine {
        public Lua LuaInstance;
        private Engine EngineRef;

        public ScriptEngine(Engine engine) {
            EngineRef = engine;
            LuaInstance = new Lua();
            LuaInstance.LoadCLRPackage();
            RegisterFunctions();
            ExecuteScriptLine(@"Debug('Lua script engine loaded');");
        }

        internal static D3D11.DefferedD3D11RenderPath TestRef;

        private void RegisterFunctions()
        {
            RegisterFunction("Help", this, typeof(ScriptEngine).GetMethod("Help"));
            RegisterFunction("Debug", typeof(Debug).GetMethod("ScriptLog"));
            RegisterFunction("Quit", EngineRef, typeof(Engine).GetMethod("Quit"));
            RegisterFunction("setTM", this, typeof(ScriptEngine).GetMethod("TestTM"));
            //RegisterFunction("setTM", TestRef, typeof(D3D11.DefferedD3D11RenderPath).GetMethod("SetToneMappingParams"));
            //RegisterFunction("DebugRender", Engine.Instance.RendererTechniqueRef, typeof(RenderTechnique.BaseRendererTechnique).GetMethod("SetDebug"));
            //RegisterFunction("RenderIndex", Engine.Instance.RendererTechniqueRef, typeof(RenderTechnique.BaseRendererTechnique).GetMethod("SetDebugIndex"));
            // RegisterFunction("WireframeRender", Engine.Instance, typeof(Engine).GetMethod("SetWireframeRender"));
            // RegisterFunction("SolidRender", Engine.Instance, typeof(Engine).GetMethod("SetSolidRender"));
            //RegisterFunction("stat", Engine.Instance.UIConsoleInstance, typeof(UIConsole).GetMethod("ToggleFPSCounter"));
        }

        public void TestTM(float a, float b, float c)
        {
            TestRef.SetToneMappingParams(a, b, c);
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
                Debug.ScriptLog(e.ToString());
                return e.ToString();
            }
        }

        public void ExecuteScriptFile(string fileName) {
            try {
                LuaInstance.DoFile(fileName);
            }
            catch (Exception e) {
                Debug.ScriptLog(e.ToString());
            }
        }

        private string MethodList = "Commands list:\n";
        private void RegisterFunction(string name, object target, System.Reflection.MethodInfo methodInfo)
        {
            MethodList += name + "\n";
            LuaInstance.RegisterFunction(name, target, methodInfo);
        }

        private void RegisterFunction(string name, System.Reflection.MethodInfo methodInfo)
        {
            MethodList += name + "\n";
            LuaInstance.RegisterFunction(name, methodInfo);
        }

        public void Help()
        {
            Debug.ScriptLog(MethodList);
        }
    }
}
