using System;
using System.Threading;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using CopperCowEngine.Rendering.D3D11.Displays;
using CopperCowEngine.Rendering.D3D11.Loaders;
using CopperCowEngine.Rendering.D3D11.RenderPaths;
using CopperCowEngine.Rendering.D3D11.Shared;
using CopperCowEngine.Rendering.D3D11.Utils;
using CopperCowEngine.Rendering.Data;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace CopperCowEngine.Rendering.D3D11
{
    public sealed partial class D3D11RenderBackend : BaseRenderBackend
    {
        #region Surface and Display Properties
        internal Control Surface;

        internal Display DisplayRef { get; private set; }

        public Device Device => DisplayRef.DeviceRef;

        public DeviceContext Context => DisplayRef.DeviceRef.ImmediateContext;

        public SwapChain SwapChain => ((FormDisplay)DisplayRef).SwapChainRef;
        #endregion

        private RenderPathType _renderPathType;
        internal BaseD3D11RenderPath RenderPath;
        //private ConsoleRenderer _consoleRenderer;
        internal SharedRenderItemsStorage SharedRenderItems;

        public bool IsSingleFormMode { get; private set; }

        public override bool IsInitialized { get; protected set; }

        public override bool IsExitRequest { get; protected set; }

        public override RenderingConfiguration Configuration { get; protected set; }

        public override ScreenProperties ScreenProps { get; protected set; }
        
        internal Action<char> EngineOnCharPressed;

        private KeyPressEventHandler _keyPressEventHandler;

        internal int SampleCount;

        private readonly GpuProfiler _gpuProfiler;

        private const int TargetFrameTime = 1000 / 90;

        #region Initialize
        public D3D11RenderBackend()
        {
            CurrentFrameData = new StandardFrameData();
            ScreenProps = new ScreenProperties();
            _gpuProfiler = new GpuProfiler();
        }

        public override void Initialize(RenderingConfiguration config, params object[] parameters)
        {
            Configuration = config;
            AssetsManagement.AssetsManager.RenderBackend = this;
            D3D11AssetsLoader.RenderBackend = this;
            D3D11ShaderLoader.RenderBackend = this;

            _renderPathType = config.RenderPath;

            var name = config.AppName;

            var isInterop = (bool)parameters[0];

            if (isInterop)
            {
                DisplayRef = new InteropDisplay(config.DebugMode, (int)config.EnableMsaa);
                InitializeViews();
                return;
            }

            if (!(parameters[1] is Control surface))
            {
                IsSingleFormMode = true;
                surface = new EngineRenderForm
                {
                    ClientSize = new System.Drawing.Size(1000, 700),
                    Text = name,
                };
            }

            InitializeSurface(surface);

            _gpuProfiler.Initialize(Device);
        }

        private void InitializeSurface(Control surface)
        {
            Surface = surface;

            SampleCount = Configuration.RenderPath == RenderPathType.Deferred ? 1 :
                (int)Configuration.EnableMsaa;

            DisplayRef = new FormDisplay(Configuration.DebugMode, SampleCount)
            {
                Surface = surface,
            };

            _keyPressEventHandler = (o, args) => 
            {
                EngineOnCharPressed?.Invoke(args.KeyChar);
            };

            surface.KeyPress += _keyPressEventHandler;

            InitializeViews();
        }

        private void InitializeViews()
        {
            DisplayRef.OnResize += OnDisplayResize;

            DisplayRef.OnInitRenderTarget += OnInitRenderTarget;

            DisplayRef.OnRender += OnRenderFrame;

            DisplayRef.InitDevice();

            if (!IsSingleFormMode)
            {
                return;
            }

            DisplayRef.InitRenderTarget();
        }

        private void InitRenderStuff()
        {
            SharedRenderItems = new SharedRenderItemsStorage(this);

            switch (_renderPathType)
            {
                case RenderPathType.Forward:
                    RenderPath = new ForwardBaseD3D11RenderPath();
                    break;
                case RenderPathType.Deferred:
                    // TODO: Deferred
                    RenderPath = new DeferredD3D11RenderPath();
                    break;
                case RenderPathType.TiledForward:
                    // TODO: TiledForward
                    //RenderPath = new TiledForwardD3D11RenderPath();
                    break;
                case RenderPathType.ClusteredForward:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            RenderPath.Init(this);

            //_consoleRenderer = new ConsoleRenderer();
            //_consoleRenderer.Initialize(this);

            ScreenProps.Width = DisplayRef.Width;
            ScreenProps.Height = DisplayRef.Height;
            ScreenProps.AspectRatio = DisplayRef.Width / (float)DisplayRef.Height;
        }

        public override void ExitRequest()
        {
            IsExitRequest = true;

            if (IsSingleFormMode)
            {
                return;
            }

            Deinitialize();
        }

        private void OnInitRenderTarget()
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;
            InitRenderStuff();
            //TODO: Engine callback
        }
        #endregion

        public override void RenderFrame()
        {
            // TODO: provide in right place
            OnScreenPropertiesChanged?.Invoke(ScreenProps);

            if (!IsSingleFormMode)
            {
                // TODO: interop display loop
                return;
            }

            using (var loop = new RenderLoop(Surface))
            {
                while (loop.NextFrame())
                {
                    if (IsExitRequest)
                    {
                        if (IsSingleFormMode)
                        {
                            ((Form)Surface).Close();
                        }
                        break;
                    }

                    var start = DateTime.Now;
                    _gpuProfiler.Begin(Context);
                    DisplayRef.Render(IntPtr.Zero, false);
                    _gpuProfiler.End(Context);
                    //var ms = (int)_gpuProfiler.GetElapsedMilliseconds(Context);

                    // TODO: Implement normal WaitForTargetFrameTime
                    var waitForTargetFrameTime = TargetFrameTime - (DateTime.Now - start).Milliseconds - 2;
                    if (waitForTargetFrameTime > 0)
                    {
                        Thread.Sleep(waitForTargetFrameTime);
                    }

                    OnFrameRenderEnd?.Invoke();
                }
            }
            Deinitialize();

            /*
            //_gpuProfiler.Begin(Context);
            DisplayRef.Render(IntPtr.Zero, false);
            //_gpuProfiler.End(Context);
            OnFrameRenderEnd?.Invoke();
            */
        }

        private void OnDisplayResize()
        {
            ScreenProps.Width = DisplayRef.Width;
            ScreenProps.Height = DisplayRef.Height;
            ScreenProps.AspectRatio = DisplayRef.AspectRatio;
            OnScreenPropertiesChanged?.Invoke(ScreenProps);
            RenderPath.Resize();
        }

        // HACK
        private bool _firstFrame = true;

        public void OnRenderFrame()
        {
            if (!IsInitialized)
            {
                return;
            }

            OnFrameRenderStart?.Invoke();

            RenderPath.Draw((StandardFrameData)CurrentFrameData);

            //TODO: D2D draw
            DisplayRef.RenderTarget2D.BeginDraw();
            //_consoleRenderer.Draw();
            DisplayRef.RenderTarget2D.EndDraw();

            if (!_firstFrame)
            {
                return;
            }
            _firstFrame = false;

            if (DisplayRef is FormDisplay display)
            {
                display.Surface?.Show();
            }
        }

        /// <summary>
        /// Draw non-indexed, non-instanced primitives. Wrapper for collect drawcalls statistics.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer.</param>
        internal void DrawWrapper(int vertexCount, int startVertexLocation)
        {
            OnDrawCall?.Invoke();
            Context.Draw(vertexCount, startVertexLocation);
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives. Wrapper for collect drawcalls statistics.
        /// </summary>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        internal void DrawIndexedWrapper(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            OnDrawCall?.Invoke();
            Context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }

        public override void Deinitialize()
        {
            DisplayRef.OnRender -= OnRenderFrame;
            DisplayRef.OnInitRenderTarget -= OnInitRenderTarget;

            if (IsSingleFormMode)
            {
                Surface.KeyPress -= _keyPressEventHandler;
            }
            DisplayRef.OnResize -= OnDisplayResize;

            //_consoleRenderer?.Dispose();
            SharedRenderItems?.Dispose();
            RenderPath?.Dispose();
            DisplayRef.Cleanup();
            DisplayRef = null;
        }

        public override event Action OnFrameRenderStart;
        public override event Action OnDrawCall;
        public override event Action OnFrameRenderEnd;
        public override event Action<ScreenProperties> OnScreenPropertiesChanged;
    }
}
