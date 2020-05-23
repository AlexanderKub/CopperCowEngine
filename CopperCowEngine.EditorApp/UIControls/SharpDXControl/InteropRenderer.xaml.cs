using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using CopperCowEngine.EditorApp.AssetsEditor;

namespace CopperCowEngine.EditorApp.UIControls.SharpDXControl
{
    /// <summary>
    /// Interaction logic for InteropRenderer.xaml
    /// </summary>
    public partial class InteropRenderer : UserControl
    {
        internal PreviewEngine EngineRef 
        {
            get => (PreviewEngine)GetValue(EngineProperty);
            set => SetValue(EngineProperty, value);
        }

        public static readonly DependencyProperty EngineProperty = DependencyProperty.Register(
            "EngineRef", 
            typeof(PreviewEngine),
            typeof(InteropRenderer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, 
                OnEngineRefChanged)
        );

        private static void OnEngineRefChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            ((InteropRenderer)d).EngineRefSetter((PreviewEngine)e.NewValue);
        }

        private TimeSpan _lastRender;

        private bool _lastVisible;

        public InteropRenderer() 
        {
            InitializeComponent();
            Focus();
            Host.Loaded += Host_Loaded;
            Host.SizeChanged += Host_SizeChanged;

            //this.Host.KeyDown += new KeyEventHandler(InputHandler_KeyDown);
            //this.Host.PreviewTextInput += new TextCompositionEventHandler(InputHandler_PreviewTextInput);
        }

        #region Input Handlers
        // override focus TODO: fix unfocus by double arrows
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
           // EngineRef?.WpfKeyboardInput(false, (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key));
            switch (e.Key) 
            {
                case Key.Left:
                    //EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Left);
                    e.Handled = true;
                    return;
                case Key.Up:
                    //EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Up);
                    e.Handled = true;
                    return;
                case Key.Right:
                    //EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Right);
                    e.Handled = true;
                    return;
                case Key.Down:
                    //EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Down);
                    e.Handled = true;
                    return;
                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) 
        {
            base.OnKeyUp(e);
            //EngineRef?.WpfKeyboardInput(true, (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key));
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e) 
        {
            //base.OnPreviewTextInput(e);
            if (e.Text.Length == 0) 
            {
                return;
            }
            //EngineRef?.OnKeyCharPressWpf(e.Text[0]);
        }
        #endregion

        #region Lifecycle Events
        protected override void OnMouseEnter(MouseEventArgs e) 
        {
            base.OnMouseEnter(e);
            Focus();
        }

        protected override void OnMouseLeave(MouseEventArgs e) 
        {
            base.OnMouseLeave(e);
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            //EngineRef?.WpfKeyboardInputReset();
        }

        private void EngineRefSetter(PreviewEngine engine) 
        {
            EngineRef = engine;
            if (IsLoaded) 
            {
                InitializeRendering();
            }
        }

        private void Host_Loaded(object sender, RoutedEventArgs e) 
        {
            if (EngineRef != null) 
            {
                InitializeRendering();
            }
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e) 
        {
            ImageSetPixelSize();
        }
        #endregion

        #region Rendering
        private void ImageSetPixelSize() 
        {
            var dpiScale = 1.0;

            if (PresentationSource.FromVisual(this)?.CompositionTarget is HwndTarget hwndTarget) 
            {
                dpiScale = hwndTarget.TransformToDevice.M11;
            }

            var surfWidth = (int)(Host.ActualWidth < 0 ? 0 : Math.Ceiling(Host.ActualWidth * dpiScale));
            var surfHeight = (int)(Host.ActualHeight < 0 ? 0 : Math.Ceiling(Host.ActualHeight * dpiScale));

            InteropImage.SetPixelSize(surfWidth, surfHeight);

            var isVisible = (surfWidth != 0 && surfHeight != 0);

            if (_lastVisible == isVisible)
            {
                return;
            }

            _lastVisible = isVisible;
            if (_lastVisible) 
            {
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            } 
            else 
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        private void InitializeRendering() 
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow == null)
            {
                return;
            }
            
            InteropImage.WindowOwner = new WindowInteropHelper(parentWindow).Handle;
            parentWindow.Closing += (o, t) => { UninitializeRendering(); };

            InteropImage.OnRender = DoRender;
            InteropImage.RequestRender();
        }

        private void DoRender(IntPtr surface, bool isNewSurface) 
        {
            EngineRef?.RequestFrame(surface, isNewSurface);
        }

        private void UninitializeRendering()
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            EngineRef?.Dispose();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e) 
        {
            var args = (RenderingEventArgs)e;

            if (_lastRender == args.RenderingTime)
            {
                return;
            }
            InteropImage.RequestRender();
            _lastRender = args.RenderingTime;
        }
        #endregion

    }
}
