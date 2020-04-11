using CopperCowEngine.Rendering;
using System;

namespace CopperCowEngine.Core
{
    public class Engine
    {
        public event Action OnBootstrapped;
        public event Action OnBeforeFrame;
        public event Action OnAfterFrame;
        public event Action OnQuit;

        public FrameData RenderingFrameData => RenderBackend.CurrentFrameData;

        internal IRenderBackend RenderBackend;

        internal readonly EngineConfiguration Configuration;

        private bool _bootstrapped;

        public Input Input { get; }

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

            RenderBackend = Configuration.Rendering.Create();
            RenderBackend.OnFrameRenderStart += BeforeFrameLoop;
            RenderBackend.OnFrameRenderEnd += AfterFrameLoop;
            RenderBackend.OnScreenPropertiesChanged += ScreenPropertiesChanged;

            RenderBackend.OnInputKey += Input.TriggerKey;
            RenderBackend.OnMousePositionChange += Input.SetMousePosition;

            OnBootstrapped?.Invoke();
            RenderLoopStart();
        }

        public void Quit()
        {
            RenderBackend.QuitRequest();
            OnQuit?.Invoke();
        }

        private void RenderLoopStart()
        {
            RenderBackend.RenderFrame();
            /*while (true)
            {
                RenderBackend.RenderFrame();
            }*/
        }

        private void BeforeFrameLoop()
        {
            OnBeforeFrame?.Invoke();
        }

        private void AfterFrameLoop()
        {
            Time.Update();
            OnAfterFrame?.Invoke();
        }

        private static void ScreenPropertiesChanged(ScreenProperties properties)
        {
            Screen.SetScreenProperties(properties);
        }
    }
}
