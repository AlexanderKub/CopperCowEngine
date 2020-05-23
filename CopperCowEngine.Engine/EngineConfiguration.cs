using CopperCowEngine.Core;
using CopperCowEngine.Rendering;
using System;

namespace CopperCowEngine.Engine
{
    public struct EngineConfiguration
    {
        public RenderOptions Rendering;

        public ScriptEngineOptions ScriptEngine;

        public IEngineLoopProvider EngineLoopProvider;
    }
}
