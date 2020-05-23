using System;
using CopperCowEngine.Core;
using CopperCowEngine.Rendering;

namespace CopperCowEngine.Engine
{
    public partial class Engine
    {
        
        private IRenderBackend _renderBackend;

        private bool _bootstrapped;
        
        public EngineConfiguration Configuration { get; private set; }

        public Input Input { get; }

        public FrameData RenderingFrameData => _renderBackend.CurrentFrameData;

        public Frame2DData Rendering2DFrameData => _renderBackend.Current2DFrameData;
        
        public IScriptEngine ScriptEngine { get; private set; }

        public Engine(EngineConfiguration configuration)
        {
            Configuration = configuration;
            Input = new Input();
        }

        public void Bootstrap()
        {
            if (_bootstrapped)
            {
                return;
            }
            _bootstrapped = true;

            Time.Start();
            Configuration.EngineLoopProvider.OnUpdate += Time.Update;

            ScriptEngine = Configuration.ScriptEngine.Create();
            CreateScriptCommands();

            _renderBackend = Configuration.Rendering.Create(Configuration.EngineLoopProvider, ScriptEngine);
            // TODO: refactoring
            _renderBackend.OnScreenPropertiesChanged += Screen.SetScreenProperties;
            _renderBackend.OnInputKey += Input.TriggerKey;
            _renderBackend.OnInputKeyPress += Input.TriggerKeyPress;
            _renderBackend.OnMousePositionChange += Input.SetMousePosition;
            
            Configuration.EngineLoopProvider.Start();
            _renderBackend.RenderFrame();
        }

        public void RequestFrame(IntPtr surface, bool isNewSurface)
        {
            _renderBackend.RequestFrame(surface, isNewSurface);
        }

        public void Quit()
        {
            Configuration.EngineLoopProvider.Quit();
            //ScriptEngine?.Dispose();
        }
    }
}
