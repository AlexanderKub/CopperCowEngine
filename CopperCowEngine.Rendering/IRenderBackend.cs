using CopperCowEngine.Core;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace CopperCowEngine.Rendering
{
    public abstract class FrameData
    {
        public abstract void Reset();
        public abstract void Finish();
    }
    
    public abstract class Frame2DData
    {
        public abstract void Reset();
        public abstract void Finish();
    }

    public interface IRenderBackend
    {
        RenderingConfiguration Configuration { get; }
        
        IEngineLoopProvider LoopProvider { get; }

        IScriptEngine ScriptEngine { get; }

        FrameData CurrentFrameData { get; }
        
        Frame2DData Current2DFrameData { get; }

        bool IsQuitRequest { get; }

        bool IsInitialized { get; }

        ScreenProperties ScreenProps { get; }

        void Deinitialize();

        void QuitRequest();

        void Initialize(RenderingConfiguration config, IEngineLoopProvider loopProvider, 
            IScriptEngine scriptEngine, params object[] parameters);

        void RenderFrame();

        void RequestFrame(IntPtr surface, bool isNew);

        void SwitchConfiguration(RenderingConfiguration config);

        event Action<ScreenProperties> OnScreenPropertiesChanged;

        event Action<Keys, bool> OnInputKey;

        event Action<char> OnInputKeyPress;

        event Action<Vector2> OnMousePositionChange;
    }
}