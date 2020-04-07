using CopperCowEngine.Rendering;
using System;

namespace CopperCowEngine.Core
{
    public class Engine
    {
        internal IRenderBackend RenderBackend;

        internal readonly EngineConfiguration Configuration;

        private bool _bootstrapped;

        public event Action OnBootstrapped;
        public event Action OnBeforeFrame;
        public event Action OnAfterFrame;

        public FrameData RenderingFrameData => RenderBackend.CurrentFrameData;

        public Engine(EngineConfiguration configuration)
        {
            Configuration = configuration;
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

            OnBootstrapped?.Invoke();
            RenderLoopStart();
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
