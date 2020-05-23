using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Core
{
    public abstract class ScriptEngineOptions
    {
        internal abstract IScriptEngine Create();
    }

    public class ScriptEngineOptions<T> : ScriptEngineOptions where T : IScriptEngine
    {
        internal override IScriptEngine Create()
        {
            var scriptEngine = Activator.CreateInstance<T>();
            return scriptEngine;
        }
    }
}
