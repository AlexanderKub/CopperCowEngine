using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace EngineCore.D3D11
{
    internal sealed class D3D11RenderBackend : IRenderBackend
    {
        #region Surface and Display Properties
        internal Control Surface;
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

        internal Engine EngineRef;
        private RenderPathEnum m_RenderPathType;
        public BaseD3D11RenderPath RenderPath;
        private ConsoleRenderer m_ConsoleRenderer;
        public SharedRenderItemsStorage SharedRenderItems;

        public bool IsSingleFormMode { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsExitRequest { get; private set; }
        public ScreenProperties ScreenProps { get; private set; }

        #region Initialize

        public void Initialize(Engine engine, params object[] prms)
        {
            EngineRef = engine;
            m_RenderPathType = engine.CurrentConfig.RenderPath;
            AssetsLoader.RenderBackend = this;
            string name = engine.CurrentConfig.AppName;
            bool isInterop = (bool)prms[0];
            Control surface = prms[1] as Control;

            if (isInterop)
            {
                DisplayRef = new InteropDisplay(EngineRef.CurrentConfig.DebugMode, 
                    (int)EngineRef.CurrentConfig.EnableMSAA);
                InitializeViews();
                return;
            }

            if (surface == null)
            {
                IsSingleFormMode = true;
                surface = new EngineRenderForm()
                {
                    ClientSize = new System.Drawing.Size(1000, 700),
                };
                surface.Text = name;
            }
            InitializeSurface(surface);
        }
       
        /// <summary>
        /// HACK!!!!!!!!!!!!!!!
        /// </summary>
        internal Func<IFrameData> EngineUpdateAction;
        internal Action EngineQuitAction;
        internal Action EngineOnRunned;
        internal Action<char> EngineOnCharPressed;
        internal D3D11RenderBackend(Func<IFrameData> OnUpdate, Action OnQuit, Action OnRunned, Action<char> OnCharPressed)
        {
            EngineOnRunned = OnRunned;
            EngineUpdateAction = OnUpdate;
            EngineQuitAction = OnQuit;
            EngineOnCharPressed = OnCharPressed;
            ScreenProps = new ScreenProperties();
        }

        private KeyPressEventHandler m_KeyPressEventHandler;
        internal int SampleCount;
        private void InitializeSurface(Control surface)
        {
            SampleCount = EngineRef.CurrentConfig.RenderPath == RenderPathEnum.Deffered ? 1 : 
                (int)EngineRef.CurrentConfig.EnableMSAA;
            SampleCount = (int)EngineRef.CurrentConfig.EnableMSAA;
            Surface = surface;
            DisplayRef = new FormDisplay(EngineRef.CurrentConfig.DebugMode, SampleCount)
            {
                Surface = surface,
            };
            m_KeyPressEventHandler = new KeyPressEventHandler((object o, KeyPressEventArgs args) => {
                EngineOnCharPressed(args.KeyChar);
            });
            surface.KeyPress += m_KeyPressEventHandler;
            InitializeViews();
        }

        //Utils.GPUProfiler profiler;
        private void InitializeViews()
        {
            DisplayRef.OnResize += OnDisplayResize;
            DisplayRef.OnInitRenderTarget += OnInitRenderTarget;
            DisplayRef.OnRender += OnRenderFrame;
            DisplayRef.InitDevice();

            if (!IsSingleFormMode) {
                return;
            }

            DisplayRef.InitRenderTarget();
        }

        private void InitRenderStuff()
        {
            SharedRenderItems = new SharedRenderItemsStorage(this);

            switch (m_RenderPathType) {
                case RenderPathEnum.Forward:
                    RenderPath = new ForwardBaseD3D11RenderPath();
                    break;
                case RenderPathEnum.Deffered:
                    // TODO: Deffered
                    RenderPath = new DefferedD3D11RenderPath();
                    break;
                case RenderPathEnum.TiledForward:
                    // TODO: TiledForward
                    RenderPath = new TiledForwardD3D11RenderPath();
                    break;
            }
            RenderPath.Init(this);

            m_ConsoleRenderer = new ConsoleRenderer();
            m_ConsoleRenderer.Initialize(this);

            ScreenProps.Width = DisplayRef.Width;
            ScreenProps.Height = DisplayRef.Height;
            ScreenProps.AspectRatio = DisplayRef.Width / (float)DisplayRef.Height;

            EngineOnRunned?.Invoke();
            if (!IsSingleFormMode) {
                return;
            }

            using (var loop = new RenderLoop(Surface)) {
                while (loop.NextFrame()) {
                    if (IsExitRequest) {
                        if (IsSingleFormMode) {
                            ((Form)Surface).Close();
                        }
                        break;
                    }
                    //profiler.Begin(Context);
                    DisplayRef.Render(IntPtr.Zero, false);
                    //profiler.End(Context);
                }
            }
            Deinitialize();
        }

        public void ExitRequest()
        {
            IsExitRequest = true;

            if (IsSingleFormMode) {
                return;
            }

            Deinitialize();
        }

        private void OnInitRenderTarget()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                InitRenderStuff();
                //TODO: Engine callback
            }
        }
        #endregion
        
        /*public void RenderFrame(IFrameData frameData)
        {
            DisplayRef.Render(IntPtr.Zero, false);
        }*/

        private void OnDisplayResize()
        {
            ScreenProps.Width = DisplayRef.Width;
            ScreenProps.Height = DisplayRef.Height;
            ScreenProps.AspectRatio = DisplayRef.AspectRatio;
            RenderPath.Resize();
        }

        // HACK
        private bool firstFrame = true;
        public void OnRenderFrame()
        {
            if (!IsInitialized)  {
                return;
            }
            StandardFrameData sFrameData = (StandardFrameData)EngineUpdateAction();
            EngineRef.Statistics.ClearDrawcalls();
            RenderPath.Draw(sFrameData);

            //TODO: D2D draw
            DisplayRef.RenderTarget2D.BeginDraw();
            m_ConsoleRenderer.Draw();
            DisplayRef.RenderTarget2D.EndDraw();

            if (!firstFrame)
            {
                return;
            }
            firstFrame = false;
            if (DisplayRef is FormDisplay)
            {
                (DisplayRef as FormDisplay).Surface?.Show();
            }
        }

        /// <summary>
        /// Draw non-indexed, non-instanced primitives. Wrapper for collect drawcalls statistics.
        /// </summary>
        /// <param name="vertexCount">Number of vertices to draw.</param>
        /// <param name="startVertexLocation">Index of the first vertex, which is usually an offset in a vertex buffer.</param>
        internal void DrawWrapper(int vertexCount, int startVertexLocation)
        {
            EngineRef.Statistics.IncDrawcalls();
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
            EngineRef.Statistics.IncDrawcalls();
            Context.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }
        
        public void Deinitialize()
        {
            DisplayRef.OnRender -= OnRenderFrame;
            DisplayRef.OnInitRenderTarget -= OnInitRenderTarget;
            if (IsSingleFormMode) {
                Surface.KeyPress -= m_KeyPressEventHandler;
            }
            DisplayRef.OnResize -= OnDisplayResize;

            EngineQuitAction();
            m_ConsoleRenderer?.Dispose();
            SharedRenderItems?.Dispose();
            RenderPath?.Dispose();
            DisplayRef.Cleanup();
            DisplayRef = null;
        }
    }
}
