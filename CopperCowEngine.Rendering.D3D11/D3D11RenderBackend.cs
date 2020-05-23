using System;
using System.Threading;
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
using System.Windows.Forms;
using CopperCowEngine.Core;

namespace CopperCowEngine.Rendering.D3D11
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed partial class D3D11RenderBackend : BaseRenderBackend
    {
        #region Surface and Display Properties
        internal Control Surface;

        internal Display DisplayRef { get; private set; }

        public Device Device => DisplayRef.DeviceRef;

        public DeviceContext Context => DisplayRef.DeviceRef.ImmediateContext;

        public SwapChain SwapChain => ((FormDisplay)DisplayRef).SwapChainRef;
        #endregion
        
        private const int TargetFrameTime = 1000 / 90;

        private readonly GpuProfiler _gpuProfiler;

        private RenderPathType _renderPathType;
        private Frame2DRenderer _frame2DRenderer;
        private bool _configSwitched;

        internal int SampleCount;
        internal BaseD3D11RenderPath RenderPath;
        internal SharedRenderItemsStorage SharedRenderItems;

        public bool IsSingleFormMode { get; private set; }

        public override bool IsInitialized { get; protected set; }

        public override bool IsQuitRequest { get; protected set; }

        public override RenderingConfiguration Configuration { get; protected set; }

        public override ScreenProperties ScreenProps { get; protected set; }

        #region Initialize
        public D3D11RenderBackend()
        {
            CurrentFrameData = new StandardFrameData();
            Current2DFrameData = new Standard2DFrameData();
            ScreenProps = new ScreenProperties();
            _gpuProfiler = new GpuProfiler();
        }

        public override void Initialize(RenderingConfiguration config, IEngineLoopProvider loopProvider, 
            IScriptEngine scriptEngine, params object[] parameters)
        {
            base.Initialize(config, loopProvider, scriptEngine, parameters);

            D3D11AssetsLoader.RenderBackend = this;
            D3D11ShaderLoader.RenderBackend = this;

            _renderPathType = config.RenderPath;

            var name = config.AppName;

            var isInterop = (bool)parameters[0];

            if (isInterop)
            {
                SampleCount = (int)Configuration.EnableMsaa;
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

            SampleCount = Configuration.RenderPath == RenderPathType.Deferred || Configuration.EnableHdr ? 1 :
                (int)Configuration.EnableMsaa;

            DisplayRef?.Dispose();
            DisplayRef = new FormDisplay(Configuration.DebugMode, SampleCount)
            {
                Surface = surface,
            };

            RegisterInputHandling();

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
                    //RenderPath = new ForwardBaseD3D11RenderPath();
                    RenderPath = new D3D11ForwardPath();
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

            _frame2DRenderer = new Frame2DRenderer();
            _frame2DRenderer.Initialize(this);

            ScreenProps.Width = DisplayRef.Width;
            ScreenProps.Height = DisplayRef.Height;
            ScreenProps.AspectRatio = DisplayRef.Width / (float)DisplayRef.Height;
        }

        public override void QuitRequest()
        {
            IsQuitRequest = true;

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

        private static void WaitForTargetFps(int milliseconds)
        {
            // TODO: Implement normal WaitForTargetFrameTime
            var waitForTargetFrameTime = TargetFrameTime - milliseconds - 1;
            if (waitForTargetFrameTime > 0)
            {
                Thread.Sleep(waitForTargetFrameTime);
            }
        }

        public override void SwitchConfiguration(RenderingConfiguration config)
        {
            base.SwitchConfiguration(config);
            SampleCount = Configuration.RenderPath == RenderPathType.Deferred || Configuration.EnableHdr ? 1 :
                (int)Configuration.EnableMsaa;
            _configSwitched = true;
        }

        private void ApplyConfigurationSwitch()
        {
            if (_configSwitched)
            {
                //DisplayRef.
                RenderPath.Init(this);
                _configSwitched = false;
            }
        }

        public override void RenderFrame()
        {
            ApplyConfigurationSwitch();
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
                    if (IsQuitRequest)
                    {
                        if (IsSingleFormMode)
                        {
                            ((Form)Surface).Close();
                        }
                        break;
                    }
                    
                    ApplyConfigurationSwitch();

                    var start = DateTime.Now;
                    //_gpuProfiler.Begin(Context);
                    DisplayRef.Render(IntPtr.Zero, false);
                    //_gpuProfiler.End(Context);
                    //var ms = (int)_gpuProfiler.GetElapsedMilliseconds(Context);

                    WaitForTargetFps((DateTime.Now - start).Milliseconds);

                    LoopProvider.Update();
                }
            }
            Deinitialize();

            /*
            //_gpuProfiler.Begin(Context);
            DisplayRef.Render(IntPtr.Zero, false);
            //_gpuProfiler.End(Context);
            */
        }

        public override void RequestFrame(IntPtr surface, bool isNew)
        {
            if (IsSingleFormMode)
            {
                return;
            }

            DisplayRef?.Render(surface, isNew);
            LoopProvider.Update();
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

        private void OnRenderFrame()
        {
            if (!IsInitialized)
            {
                return;
            }
            FlushStatistics();

            RenderPath.Draw((StandardFrameData)CurrentFrameData);

            DisplayRef.RenderTarget2D.BeginDraw();
            _frame2DRenderer.Draw((Standard2DFrameData)Current2DFrameData);
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
        /// Draw non-indexed, non-instanced primitives. Wrapper for collect draw calls statistics.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer.</param>
        internal void DrawWrapper(int vertexCount, int startVertexLocation)
        {
            DispatchDrawCall();
            Context.Draw(vertexCount, startVertexLocation);
        }

        /// <summary>
        /// Draw indexed, non-instanced primitives. Wrapper for collect draw calls statistics.
        /// </summary>
        /// <param name="indexCount">Number of indices to draw.</param>
        /// <param name="startIndexLocation">The location of the first index read by the GPU from the index buffer.</param>
        /// <param name="baseVertexLocation">A value added to each index before reading a vertex from the vertex buffer.</param>
        internal void DrawIndexedWrapper(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            DispatchDrawCall();
            Context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }

        public override void Deinitialize()
        {
            DisplayRef.OnRender -= OnRenderFrame;
            DisplayRef.OnInitRenderTarget -= OnInitRenderTarget;

            UnRegisterInputHandling();

            DisplayRef.OnResize -= OnDisplayResize;

            _frame2DRenderer?.Dispose();
            SharedRenderItems?.Dispose();
            RenderPath?.Dispose();
            DisplayRef.Dispose();
            DisplayRef = null;
        }

        public override event Action<ScreenProperties> OnScreenPropertiesChanged;
    }
}
