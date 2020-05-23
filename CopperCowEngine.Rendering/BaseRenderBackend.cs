using System;
using System.Numerics;
using System.Windows.Forms;
using CopperCowEngine.Core;

namespace CopperCowEngine.Rendering
{
    public abstract class BaseRenderBackend : IRenderBackend
    {
        // TODO: Protected EventTriggers
        public abstract event Action<ScreenProperties> OnScreenPropertiesChanged;

        public abstract event Action<Keys, bool> OnInputKey;

        public abstract event Action<char> OnInputKeyPress;

        public abstract event Action<Vector2> OnMousePositionChange;

        public abstract ScreenProperties ScreenProps { get; protected set; }

        public abstract bool IsInitialized { get; protected set; }

        public abstract bool IsQuitRequest { get; protected set; }

        public FrameData CurrentFrameData { get; protected set; }

        public Frame2DData Current2DFrameData { get; protected set; }

        public abstract RenderingConfiguration Configuration { get; protected set; }

        public IEngineLoopProvider LoopProvider { get; protected set; }

        public IScriptEngine ScriptEngine { get; protected set; }

        public abstract void Deinitialize();

        public abstract void QuitRequest();

        public virtual void Initialize(RenderingConfiguration config, IEngineLoopProvider loopProvider, 
            IScriptEngine scriptEngine, params object[] parameters)
        {
            Configuration = config;
            ScriptEngine = scriptEngine;
            LoopProvider = loopProvider;
            LoopProvider.OnQuit += QuitRequest;
        }

        public virtual void SwitchConfiguration(RenderingConfiguration config)
        {
            Configuration = config;
        }

        protected void DispatchDrawCall()
        {
            Statistics.DrawCall();
        }

        protected void FlushStatistics()
        {
            Statistics.Flush();
        }

        public abstract void RenderFrame();
        public abstract void RequestFrame(IntPtr surface, bool isNew);
    }
}