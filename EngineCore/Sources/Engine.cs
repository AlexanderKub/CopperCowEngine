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
using EngineCore.Displays;
using System.Linq;

namespace EngineCore
{
    public class Engine
    {
        public static Engine Instance;
        public string Name;
        public SharpDX.Color ClearColor = SharpDX.Color.DarkCyan;

        public bool IsInitialized { get; private set; }
        private bool IsExitRequest;
        public bool IsSingleFormMode { get; private set; }

        public double gpuFrameTime;
        private Utils.GPUProfiler gpuProfiler;
        internal Utils.CPUProfiler CPUProfiler;

        #region Engine config properties
        public static bool EnableShaderLogLevel = true;
        #endregion

        #region Surface and Display Properties
        private Control Surface;
        internal Display DisplayRef { get; private set; }

        public Device Device {
            get {
                return DisplayRef.DeviceRef;
            }
        }

        public DeviceContext Context {
            get {
                return DisplayRef.DeviceRef.ImmediateContext;
            }
        }

        public SwapChain SwapChain {
            get {
                return ((FormDisplay)DisplayRef).SwapChainRef;
            }
        }
        #endregion

        public Engine(string name) {
            IsSingleFormMode = true;
            Control m_Form = new EngineRenderForm() {
                ClientSize = new Size(1000, 700),
            };
            m_Form.Text = name;
            Initialize(m_Form);
        }

        public Engine(Control surface) {
            IsSingleFormMode = false;
            Initialize(surface);
        }

        public Engine() {
            IsSingleFormMode = false;
            Instance = this;
            DisplayRef = new InteropDisplay();
            InitViews();
        }

        public virtual RenderTechnique.RenderPath GetRenderPath()
        {
            return RenderTechnique.RenderPath.Forward;
        }

        #region Life Cycle

        public void Initialize(Control surface) {
            Surface = surface;
            Instance = this;
            DisplayRef = new FormDisplay() {
                Surface = surface,
            };
            InitViews();
            if (IsSingleFormMode) {
                LoadBasicShaders();
                CreateMainSystems();
            }
        }

        private void InitViews() {
            DisplayRef.OnRender += RenderOneFrame;
            DisplayRef.OnInitRenderTarget += OnInitRenderTarget;
            DisplayRef.InitDevice();
            DisplayRef.InitRenderTarget();
            CreateRasterizerStates();
        }

        public void Run() {
            LoadMaterials();
            if (!IsSingleFormMode) {
                return;
            }

            OnStart();

            Surface?.Show();

            gpuFrameTime = 0.0;
            gpuProfiler = new Utils.GPUProfiler();
            gpuProfiler.Initialize(Device);
            CPUProfiler = new Utils.CPUProfiler();
            CPUProfiler.Initialize();

            using (var loop = new RenderLoop(Surface)) {
                while (loop.NextFrame()) {
                    RunFrame();
                    if (IsExitRequest) {
                        if (IsSingleFormMode) {
                            ((Form)Surface).Close();
                        }
                        loop.Dispose();
                        break;
                    }
                }
            }

            CleanupSystemResources();
        }

        public void Quit() {
            if (IsSingleFormMode) {
                IsExitRequest = true;
                return;
            }
            CleanupSystemResources();
        }

        private void OnInitRenderTarget() {
            if (!IsInitialized) {
                if (!IsSingleFormMode) {
                    LoadBasicShaders();
                    CreateMainSystems();
                    OnStart();
                }
                IsInitialized = true;
            }
        }

        public void RunFrame() {
            RunFrame(IntPtr.Zero, false);
        }

        public void RunFrame(IntPtr surface, bool isNewSurface) {
            if (DisplayRef.Factory2D.IsDisposed) {
                return;
            }
            if (IsSingleFormMode) {
                //GPU Profile
                gpuProfiler?.Begin(Context);
            }

            Time?.Update();
            DisplayRef.Render(surface, isNewSurface);

            if (IsSingleFormMode) {
                gpuProfiler?.End(Context);
                if (gpuProfiler != null) {
                    gpuFrameTime = gpuProfiler.GetElapsedMilliseconds(Context);
                }
            }
        }

        private void RenderOneFrame() {
            if (!IsInitialized) {
                return;
            }

            Update();

            GameObjects.ForEach((x) => {
                if (x.transform.Parent != null) {
                    return;
                }
                x.Update();
            });
            if (SkySphereObject != null && MainCamera != null) {
                SkySphereObject.transform.WorldPosition = MainCamera.gameObject.transform.WorldPosition;
            }
            UIConsoleInstance.Update();

            if (Time.Time > lastPhysicTime + physicTimestamp) {
                m_BoundingScene.PhysicUpdate();
                lastPhysicTime = Time.Time;
            }

            RendererTechniqueRef.Draw();
            UIConsoleInstance.Draw();
            CPUProfiler?.Frame();
        }

        public void CleanupSystemResources() {
            CPUProfiler?.Shutdown();
            IsInitialized = false;

            RemoveInputEventListeners();

            GameObjects.ForEach((x) => {
                x.Destroy();
            });

            AssetsLoader.CleanupAssets();

            UIConsoleInstance.Dispose();
            RendererTechniqueRef.Dispose();
            DisplayRef.OnRender -= RenderOneFrame;
            DisplayRef.OnInitRenderTarget -= OnInitRenderTarget;
            DisplayRef.Cleanup();
        }

        //Display calss mb?
        public void ResetTargets() {
            //New or Cached old?
            Context.Rasterizer.SetViewport(new Viewport(
                0, 0,
                DisplayRef.Width,
                DisplayRef.Height,
                0.0f, 1.0f
            ));
            Context.OutputMerger.SetTargets(DisplayRef.DepthStencilViewRef, DisplayRef.RenderTargetViewRef);
            SetSolidRender();
        }
        #endregion

        #region Rasterizer State
        private RasterizerState m_RasterizerState;
        private RasterizerState m_WireframeRasterizerState;
        internal bool IsWireframe;

        private void CreateRasterizerStates() {
            m_RasterizerState = new RasterizerState(Device, new RasterizerStateDescription {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid,
            });

            m_WireframeRasterizerState = new RasterizerState(Device, new RasterizerStateDescription {
                CullMode = CullMode.None,
                FillMode = FillMode.Wireframe,
            });
        }

        public void SetWireframeRender() {
            IsWireframe = true;
            Context.Rasterizer.State = m_WireframeRasterizerState;
        }

        public void SetSolidRender() {
            IsWireframe = false;
            Context.Rasterizer.State = m_RasterizerState;
        }
        #endregion

        #region Engine Systems
        public List<GameObject> GameObjects;
        internal BoundingScene m_BoundingScene;

        public InputDevice Input;
        public Timer Time;
        internal RenderTechnique.BaseRendererTechnique RendererTechniqueRef;
        internal UIConsole UIConsoleInstance { get; private set; }
        public ScriptEngine ScriptEngineInstance;

        private float physicTimestamp;
        private float lastPhysicTime;

        private void CreateMainSystems() {
            GameObjects = new List<GameObject>();
            m_BoundingScene = new BoundingScene();

            Input = new InputDevice(this);
            Time = new Timer();
            physicTimestamp = 0.01f;
            lastPhysicTime = -0.01f;

            switch (GetRenderPath())
            {
                case RenderTechnique.RenderPath.Forward:
                    RendererTechniqueRef = new RenderTechnique.ForwardRendererTechnique();
                    break;
                case RenderTechnique.RenderPath.ForwardPlus:
                    RendererTechniqueRef = new RenderTechnique.ForwardPlusRendererTechnique();
                    break;
                case RenderTechnique.RenderPath.Deffered:
                    RendererTechniqueRef = new RenderTechnique.DefferedRendererTechnique();
                    break;
                default:
                    break;
            }

            RendererTechniqueRef.Init();

            UIConsoleInstance = new UIConsole();
            AddInputEventListeners();
            UIConsoleInstance.Init();
            ScriptEngineInstance = new ScriptEngine();
        }
        #endregion

        #region Game Objects
        public GameObject AddGameObject(string name)
        {
            return AddGameObject(name, false);
        }

        public GameObject AddGameObject(string name, bool withoutRenderer)
        {
            GameObject go = new GameObject(name);
            go.AddComponent(new Transform());
            if (!withoutRenderer) {
                go.AddComponent(new Renderer());
            }
            GameObjects.Add(go);
            return go;
        }
        #endregion

        #region Resources
        public Dictionary<string, Material> MaterialTable = new Dictionary<string, Material>();
        public Material GetMaterial(string Name) {
            if (MaterialTable.ContainsKey(Name)) {
                return MaterialTable[Name];
            }
            return Material.DefaultMaterial;
        }

        public GameObject SkySphereObject;
        public void CreateSkySphere() {
            SkySphereObject = AddGameObject("SkySphere");
            SkySphereObject.transform.WorldPosition = Vector3.Zero;
            SkySphereObject.transform.WorldRotation = Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0);
            SkySphereObject.transform.WorldScale = Vector3.One * 0.05f;
            SkySphereObject.GetComponent<Renderer>().SpecificType = Renderer.SpecificTypeEnum.SkySphere;
            SkySphereObject.GetComponent<Renderer>().RendererMaterial = Material.GetSkySphereMaterial();
            SkySphereObject.GetComponent<Renderer>().UpdateMesh(AssetsLoader.LoadMesh("SkySphereMesh"));
        }

        private void LoadBasicShaders() {
            //TODO: Load shaders refactoring
            /*AssetsLoader.LoadShader("CommonVS");
            AssetsLoader.LoadShader("DepthShadowsVS");
            AssetsLoader.LoadShader("UITextureVS");
            AssetsLoader.LoadShader("UITexturePS");
            AssetsLoader.LoadShader("SkySpherePS");
            AssetsLoader.LoadShader("ReflectionSpherePS");
            AssetsLoader.LoadShader("FwdSkySpherePS");
            AssetsLoader.LoadShader("PBRForwardPS");
            AssetsLoader.LoadShader("PBRDefferedPS");
            AssetsLoader.LoadShader("PBRDefferedQuadVS");
            AssetsLoader.LoadShader("PBRDefferedQuadPS");*/
        }
        #endregion

        #region Cameras
        public Camera MainCamera { get; private set; }
        internal List<Camera> m_Cameras = new List<Camera>();
        public Camera[] Cameras {
            get {
                return m_Cameras.ToArray();
            }
        }

        public Camera AddCamera<T>(string name) where T : Camera, new() {
            return AddCamera<T>(name, Vector3.Zero, Quaternion.Identity);
        }

        public Camera AddCamera<T>(string name, Vector3 Position, Quaternion Rotation) where T : Camera, new() {
            Camera camera = new T();
            camera.Init();

            m_Cameras.Add(camera);
            if (!MainCamera) {
                SetMainCamera(camera);
            }

            GameObject CameraObject = AddGameObject("CAMERA_" + name, true);
            CameraObject.transform.WorldPosition = Position;
            CameraObject.transform.WorldRotation = Rotation;
            CameraObject.AddComponent(camera);
            return camera;
        }

        public Camera SetMainCamera(Camera camera) {
            if (MainCamera) {
                MainCamera.IsMain = false;
            }
            MainCamera = camera;
            MainCamera.IsMain = true;
            return MainCamera;
        }
        #endregion

        #region Lights 
        private const int MaxDirLightCount = 3;
        private List<Light> m_DirLights = new List<Light>();
        private List<Light> m_PointLights = new List<Light>();
        private List<Light> m_SpotLights = new List<Light>();

        public Light[] Lights {
            get {
                return m_DirLights.Concat(m_PointLights).Concat(m_SpotLights).ToArray();
            }
        }

        public Light[] NonDirLights {
            get {
                return m_PointLights.Concat(m_SpotLights).ToArray();
            }
        }

        public Light MainLight {
            get {
                return m_DirLights[0];
            }
        }

        public Light[] GetLightsByType(Light.LightType type) {
            switch (type)
            {
                case Light.LightType.Directional:
                    return m_DirLights.ToArray();
                case Light.LightType.Point:
                    return m_PointLights.ToArray();
                case Light.LightType.Spot:
                    return m_SpotLights.ToArray();
            }
            return null;
        }

        public GameObject AddLight(string name, Light lightObj, Vector3 Position, Quaternion Rotation) {
            return AddLight(name, lightObj, Position, Rotation, false);
        }

        public GameObject AddLight(string name, Light lightObj, Vector3 Position, Quaternion Rotation, bool isMain) {
            if (lightObj.Type == Light.LightType.Directional) {
                if (m_DirLights.Count >= MaxDirLightCount) {
                    Log("Maximum directional light count!");
                    return null;
                }
            }
            GameObject GO = AddGameObject(name, true);
            GO.transform.WorldPosition = Position;
            GO.transform.WorldRotation = Rotation;
            GO.transform.WorldScale = Vector3.One;
            GO.AddComponent(lightObj);
            InternalAddLight(lightObj, isMain);
            return GO;
        }

        private void InternalAddLight(Light light, bool isMain) {
            switch (light.Type) {
                case Light.LightType.Directional:
                    m_DirLights.Add(light);
                    break;
                case Light.LightType.Point:
                    m_PointLights.Add(light);
                    break;
                case Light.LightType.Spot:
                    m_SpotLights.Add(light);
                    break;
            }
        }
        #endregion

        #region InpuDevice
        public bool IsInputTextFieldFocus {
            get {
                return (UIConsoleInstance != null && UIConsoleInstance.IsShowConsole);
            }
        }

        public System.Drawing.Point MousePosition {
            get {
                if (Surface == null) {
                    return new System.Drawing.Point();
                }
                return Surface.PointToClient(Cursor.Position);
            }
        }

        public void OnSpecialKeyPressedWpf(Keys key) {
            if (!IsInitialized) {
                return;
            }
            UIConsoleInstance.OnSpecialKeyPressed(key);
        }

        public void OnKeyCharPressWpf(char keyChar) {
            if (!IsInitialized) {
                return;
            }
            Input.OnKeyCharPressWpf(keyChar);
        }

        public void WpfKeyboardInputReset() {
            Input?.WpfKeyboardInputReset();
        }

        public void WpfKeyboardInput(bool Break, Keys Key) {
            Input?.WpfKeyboardInput(Break, Key);
        }

        private void AddInputEventListeners() {
            if (UIConsoleInstance != null) {
                Input.KeyCharInput += UIConsoleInstance.OnCharPressed;
            }
            if (Surface == null) {
                return;
            }
            Surface.KeyPress += Input.Device_FormKeyPress;
            if (IsSingleFormMode) {
                if (UIConsoleInstance != null) {
                    ((EngineRenderForm)Surface).OnSpecialKeyPressed += UIConsoleInstance.OnSpecialKeyPressed;
                }
            }
        }

        private void RemoveInputEventListeners() {
            if (UIConsoleInstance != null) {
                Input.KeyCharInput -= UIConsoleInstance.OnCharPressed;
            }
            if (Surface == null) {
                return;
            }
            Surface.KeyPress -= Input.Device_FormKeyPress;
            if (IsSingleFormMode) {
                if (UIConsoleInstance != null) {
                    ((EngineRenderForm)Surface).OnSpecialKeyPressed -= UIConsoleInstance.OnSpecialKeyPressed;
                }
            }
        }
        #endregion

        #region Events
        public virtual void OnStart() { }
        public virtual void LoadMaterials() { }
        public virtual void Update() { }
        #endregion

        #region Utils
        public static void Log(string log) {
            string resLine = "[Engine Log " + System.DateTime.Now.ToLocalTime() + "]: " + log;
            Instance?.UIConsoleInstance?.LogLine(resLine);
            Console.WriteLine(resLine);
        }
        #endregion

        #region Engine Meta
        public static string GetName() {
            return "Copper Cow Engine";
        }

        public static int[] GetVersion() {
            return new int[] {
                0, 0, 1
            };
        }
        #endregion
    }
}
