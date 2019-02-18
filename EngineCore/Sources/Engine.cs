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

        #region Engine config properties
        public static bool EnableShaderLogLevel = true;
        #endregion

        #region Surface and Display Properties
        private Control Surface;
        internal Display DisplayRef { get; private set; }

        public Device Device
        {
            get {
                return DisplayRef.DeviceRef;
            }
        }

        public DeviceContext Context
        {
            get {
                return DisplayRef.DeviceRef.ImmediateContext;
            }
        }

        public SwapChain SwapChain
        {
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
                SkySphereObject.transform.Position = MainCamera.gameObject.transform.Position;
            }
            UIConsoleInstance.Update();

            if (Time.Time > lastPhysicTime + physicTimestamp) {
                m_BoundingScene.PhysicUpdate();
                lastPhysicTime = Time.Time;
            }

            RendererTechnique.Draw();
            UIConsoleInstance.Draw();
        }

        public void CleanupSystemResources() {
            IsInitialized = false;

            RemoveInputEventListeners();

            GameObjects.ForEach((x) => {
                x.Destroy();
            });

            AssetsLoader.CleanupAssets();

            UIConsoleInstance.Dispose();
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
        internal BaseRendererTechnique RendererTechnique;
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

            //RendererTechnique = new Technique.ForwardRendererTechnique();
            RendererTechnique = new Technique.DefferedRendererTechnique();
            RendererTechnique.Init();

            UIConsoleInstance = new UIConsole();
            AddInputEventListeners();
            UIConsoleInstance.Init();
            ScriptEngineInstance = new ScriptEngine();
        }
        #endregion

        #region Game Objects
        public GameObject AddGameObject(GameObject go) {
            GameObjects.Add(go);
            return go;
        }

        public GameObject AddGameObject(string name, Transform transform) {
            return AddGameObject(name, transform, null);
        }

        public GameObject AddGameObject(string name, Transform transform, Renderer renderer) {
            GameObject gameObject = new GameObject(name);
            gameObject.AddComponent(transform);
            if (renderer != null) {
                gameObject.AddComponent(renderer);
            }
            GameObjects.Add(gameObject);
            return gameObject;
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
            SkySphereObject = AddGameObject(
                "SkySphere",
                new Transform() {
                    Position = Vector3.Zero,
                    Rotation = Quaternion.RotationYawPitchRoll(0, MathUtil.Pi * 0.5f, 0),
                    Scale = Vector3.One * 0.05f,
                },
                new Renderer() {
                    SpecificType = Renderer.SpecificTypeEnum.SkySphere,
                    Geometry = AssetsLoader.LoadMesh("SkySphereMesh"),
                    RendererMaterial = Material.GetSkySphereMaterial(),
                }
            );
        }

        private void LoadBasicShaders() {
            //TODO: Load shaders refactoring
            AssetsLoader.LoadShader("DefferedPBRQuadShader");
            AssetsLoader.LoadShader("DefferedPBRShader");
            AssetsLoader.LoadShader("SkySphereShader");
            AssetsLoader.LoadShader("ReflectionShader");
            AssetsLoader.LoadShader("DepthShadows");
            AssetsLoader.LoadShader("TriangleShader");
        }
        #endregion

        #region Cameras
        public Camera MainCamera { get; private set; }
        public Camera AddCamera(string name, Camera camera, Transform transform) {
            camera.Init();
            GameObject CameraObject = new GameObject(name);
            CameraObject.AddComponent(transform);
            CameraObject.AddComponent(camera);
            GameObjects.Add(CameraObject);
            return camera;
        }

        public Camera AddCamera<T>(string name, Vector3 Position, Quaternion Rotation) where T : Camera, new() {
            Camera camera = new T();
            camera.Init();
            GameObject CameraObject = new GameObject(name);
            CameraObject.AddComponent(new Transform() {
                Position = Position,
                Rotation = Rotation,
            });
            CameraObject.AddComponent(camera);
            GameObjects.Add(CameraObject);
            return camera;
        }

        public Camera SetMainCamera(Camera camera) {
            camera.IsMain = true;
            MainCamera = camera;
            return camera;
        }
        #endregion

        #region Lights  
        private static int MaxLightCount = 3;
        private int m_lightSeparator = 1;
        private Light[] m_lightsArray = new Light[MaxLightCount];

        public Light MainLight {
            get {
                return m_lightsArray[0];
            }
        }

        public void AddLight(string name, Light lightObj, Vector3 Position, Quaternion Rotation) {
            AddLight(name, lightObj, Position, Rotation, false);
        }

        public void AddLight(string name, Light lightObj, Vector3 Position, Quaternion Rotation, bool isMain) {
            //Light is not game object for hierarchy?
            GameObject GO = new GameObject(name) {
                transform = new Transform() {
                    Position = Position,
                    Rotation = Rotation,
                },
            };
            GO.AddComponent(lightObj);
            InternalAddLight(lightObj, isMain);
        }

        private void InternalAddLight(Light light, bool isMain) {
            if (isMain) {
                m_lightsArray[m_lightSeparator] = m_lightsArray[0];
                m_lightSeparator++;
                m_lightsArray[0] = light;
                return;
            }
            m_lightsArray[m_lightSeparator] = light;
            m_lightSeparator++;
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
