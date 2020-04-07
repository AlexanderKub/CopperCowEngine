using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Device = SharpDX.Direct3D11.Device;

using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;
using System.Linq;
using EngineCore.D3D11;

using EngineCore.ECS;
using EngineCore.ECS.Systems;
using EngineCore.ECS.Components;
using EngineCore.Utils;

namespace EngineCore
{
    public class Engine
    {
        internal EngineConfiguration CurrentConfig { get; private set; }

        public struct EngineConfiguration
        {
            public enum RenderBackendEnum
            {
                D3D11,
            }

            public enum MSAAEnabled
            {
                Off = 0,
                X4 = 4,
                X8 = 8,
            }

            public string AppName;
            public RenderBackendEnum RenderBackend;
            public RenderPathEnum RenderPath;
            public MSAAEnabled EnableMSAA;
            public bool EnableHDR;
            public bool DebugMode;
            public bool InteropDisplay;

            // TODO: All Engine configuration fields here

            public static EngineConfiguration Default = new EngineConfiguration()
            {
                AppName = "CopperCowEngine",
                RenderBackend = RenderBackendEnum.D3D11,
                RenderPath = RenderPathEnum.Forward,
                EnableMSAA = MSAAEnabled.X4,
                EnableHDR = false,
                DebugMode = false,
            };
        }

        private IRenderBackend RenderBackend;
        public Timer Time { get; private set; }
        public InputDevice Input { get; private set; }
        public World ECSWorld { get; private set; }
        public CPUProfiler Profiler { get; private set; }
        public StatisticsCollector Statistics { get; private set; }
        public ScriptEngine ScriptEngineRef { get; private set; }
        internal EngineConsole GetEngineConsole { get; private set; }

        private bool m_AlreadyRunning;
        public void Run()
        {
            Run(EngineConfiguration.Default);
        }

        public void Run(EngineConfiguration config)
        {
            if (m_AlreadyRunning) { return; }
            m_AlreadyRunning = true;
            CurrentConfig = config;
            m_OnStart();
        }

        public Action<IntPtr, bool> GetInteropRenderRequest()
        {
            if (CurrentConfig.InteropDisplay) {
                switch (CurrentConfig.RenderBackend) {
                    case EngineConfiguration.RenderBackendEnum.D3D11:
                        return (((RenderBackend as D3D11RenderBackend).DisplayRef) as InteropDisplay).Render;
                }
            }

            return null;
        }

        private void m_OnStart()
        {
            Time = new Timer();
            Input = new InputDevice(this, OnInputKeyEvent, MouseInputEvent);
            Profiler = new CPUProfiler().Initialize();
            Statistics = new StatisticsCollector();
            GetEngineConsole = new EngineConsole(this);
            Debug.EngineConsole = GetEngineConsole;
            ScriptEngineRef = new ScriptEngine(this);
            //TODO: create all legacy systems

            switch (CurrentConfig.RenderBackend)
            {
                case EngineConfiguration.RenderBackendEnum.D3D11:
                    RenderBackend = new D3D11RenderBackend(m_Update, m_OnQuit, m_RenderBackandWillRunned, m_OnCharPressed);
                    RenderBackend.Initialize(this, CurrentConfig.InteropDisplay, null);
                    break;
            }
        }

        private void m_RenderBackandWillRunned()
        {
            ECSWorld = new World();
            OnStart();
            InitMainSystems();
        }

        private IFrameData m_Update()
        {
            Profiler.Frame();
            Time.Update();
            Update(Time);
            UpdateMainSystems(Time);
            Profiler.ECS();

            //TODO: Update RenderBackend
            //RenderBackend.RenderFrame(frameData);
            return ECSWorld.GetSingletonComponent<SingletonFrameScene>().FrameData;
        }

        private void m_OnQuit()
        {
            Profiler.Shutdown();
            AssetsLoader.CleanupAssets();
            OnQuit();
            DestroyMainSystems();
            ECSWorld?.Destroy();
            //RenderBackend.Deinitialize();
        }

        public void Quit()
        {
            RenderBackend.ExitRequest();
        }

        #region Inputs
        private void OnInputKeyEvent(Keys key, bool isDown)
        {
            if (isDown)
            {
                if (key == Keys.Oem3)
                {
                    if (GetEngineConsole.Toggle()) {
                        ECSWorld?.GetSingletonComponent<SingletonInput>().ClearInputs();
                    }
                    return;
                }
                if (GetEngineConsole.IsShownConsole) {
                    GetEngineConsole.OnKeyDown(key);
                    return;
                }
                ECSWorld?.GetSingletonComponent<SingletonInput>().AddButtonFromKeyDown(key);
            }
            else
            {
                ECSWorld?.GetSingletonComponent<SingletonInput>().RemoveButtonFromKeyUp(key);
            }
        }

        private void MouseInputEvent(InputDevice.MouseMoveEventArgs e)
        {
            if (GetEngineConsole.IsShownConsole) { return; }
            ECSWorld?.GetSingletonComponent<SingletonInput>().UpdateMousePosition(e.Offset);
        }

        internal event Action<char> OnCharPressed;
        private void m_OnCharPressed(char c)
        {
            OnCharPressed?.Invoke(c);
        }
        #endregion

        #region Main systems
        private TransformsSystem transformsSystem;
        private CamerasSystem camerasSystem;
        private LightSystem lightSystem;
        private RenderSystem renderSystem;

        private void InitMainSystems()
        {
            transformsSystem = ECSWorld.AddSystem<TransformsSystem>();
            camerasSystem = ECSWorld.AddSystem<CamerasSystem>();
            lightSystem = ECSWorld.AddSystem<LightSystem>();
            renderSystem = ECSWorld.AddSystem<RenderSystem>();
            ECSWorld.Refresh();
        }

        SingletonConfigVar configSC;
        private void UpdateMainSystems(Timer timer)
        {
            configSC = ECSWorld.GetSingletonComponent<SingletonConfigVar>();
            configSC.ScreenWidth = RenderBackend.ScreenProps.Width;
            configSC.ScreenHeight = RenderBackend.ScreenProps.Height;
            configSC.ScreenAspectRatio = RenderBackend.ScreenProps.AspectRatio;

            transformsSystem.Update(timer);
            camerasSystem.Update(timer);
            lightSystem.Update(timer);
            renderSystem.Update(timer);
        }

        private void DestroyMainSystems()
        {
            transformsSystem?.Destroy();
            camerasSystem?.Destroy();
            lightSystem?.Destroy();
            renderSystem?.Destroy();
        }
        #endregion

        #region Life-Cycle hooks
        protected virtual void OnStart() { }
        protected virtual void Update(Timer timer) { }
        protected virtual void OnQuit() { }
        #endregion

        #region Engine info

        private const string EngineName = "Copper Cow Engine";
        private static readonly int[] Version = { 0, 0, 2 };

        public static string GetName()
        {
            return EngineName;
        }

        public static int[] GetVersion()
        {
            return Version;
        }
        #endregion
    }
}
