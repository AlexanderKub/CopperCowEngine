using Editor.AssetsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor.UIControls
{
    /// <summary>
    /// Interaction logic for InteropRenderer.xaml
    /// </summary>
    public partial class InteropRenderer : UserControl
    {
        public EngineCore.Engine EngineRef {
            get {
                return (EngineCore.Engine)GetValue(EngineProperty);
            }
            set {
                SetValue(EngineProperty, value);
            }
        }

        public static readonly DependencyProperty EngineProperty = DependencyProperty.Register(
            "EngineRef", 
            typeof(EngineCore.Engine),
            typeof(InteropRenderer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, 
                new PropertyChangedCallback(OnEngineRefChanged))
        );

        private static void OnEngineRefChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((InteropRenderer)d).EngineRefSetter((EngineCore.Engine)e.NewValue);
        }

        private TimeSpan lastRender;
        private bool lastVisible;

        public InteropRenderer() {
            InitializeComponent();
            this.Focus();
            this.Host.Loaded += new RoutedEventHandler(this.Host_Loaded);
            this.Host.SizeChanged += new SizeChangedEventHandler(this.Host_SizeChanged);

            //this.Host.KeyDown += new KeyEventHandler(InputHandler_KeyDown);
            //this.Host.PreviewTextInput += new TextCompositionEventHandler(InputHandler_PreviewTextInput);
        }

        #region Input Handlers
        // override focus TODO: fix unfocus by double arrows
        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            EngineRef?.WpfKeyboardInput(false, (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key));
            switch (e.Key) {
                case Key.Left:
                    EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Left);
                    e.Handled = true;
                    return;
                case Key.Up:
                    EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Up);
                    e.Handled = true;
                    return;
                case Key.Right:
                    EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Right);
                    e.Handled = true;
                    return;
                case Key.Down:
                    EngineRef?.OnSpecialKeyPressedWpf(System.Windows.Forms.Keys.Down);
                    e.Handled = true;
                    return;
                default:
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            EngineRef?.WpfKeyboardInput(true, (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(e.Key));
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
            //base.OnPreviewTextInput(e);
            string key = e.Text;
            if (e.Text.Length == 0) {
                return;
            }
            EngineRef?.OnKeyCharPressWpf(e.Text[0]);
        }
        #endregion

        #region Lifecycle Events
        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            this.Focus();
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            EngineRef?.WpfKeyboardInputReset();
        }

        private void EngineRefSetter(EngineCore.Engine engine) {
            EngineRef = engine;
            if (this.IsLoaded) {
                ItializeRendering();
            }
        }

        private void Host_Loaded(object sender, RoutedEventArgs e) {
            if (EngineRef != null) {
                ItializeRendering();
            }
        }

        private void Host_SizeChanged(object sender, SizeChangedEventArgs e) {
            ImageSetPixelSize();
        }
        #endregion

        #region Rendering
        private void ImageSetPixelSize() {
            double dpiScale = 1.0;

            var hwndTarget = PresentationSource.FromVisual(this).CompositionTarget as HwndTarget;
            if (hwndTarget != null) {
                dpiScale = hwndTarget.TransformToDevice.M11;
            }

            int surfWidth = (int)(Host.ActualWidth < 0 ? 0 : Math.Ceiling(Host.ActualWidth * dpiScale));
            int surfHeight = (int)(Host.ActualHeight < 0 ? 0 : Math.Ceiling(Host.ActualHeight * dpiScale));

            InteropImage.SetPixelSize(surfWidth, surfHeight);

            bool isVisible = (surfWidth != 0 && surfHeight != 0);
            if (lastVisible != isVisible) {
                lastVisible = isVisible;
                if (lastVisible) {
                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                } else {
                    CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
        }

        private void ItializeRendering() {
            //EngineRef = new PreviewEngine();
            EngineRef.Run();
            Window parentWindow = Window.GetWindow(this);
            InteropImage.WindowOwner = (new WindowInteropHelper(parentWindow)).Handle;
            parentWindow.Closing += new System.ComponentModel.CancelEventHandler((object o, System.ComponentModel.CancelEventArgs t) => {
                UninitializeRendering();
            });
            InteropImage.OnRender = this.DoRender;

            InteropImage.RequestRender();
        }

        private void DoRender(IntPtr surface, bool isNewSurface) {
            EngineRef?.RunFrame(surface, isNewSurface);
        }

        private void UninitializeRendering() {
            CompositionTarget.Rendering -= this.CompositionTarget_Rendering;
            EngineRef?.Quit();
        }

        void CompositionTarget_Rendering(object sender, EventArgs e) {
            RenderingEventArgs args = (RenderingEventArgs)e;

            if (lastRender != args.RenderingTime) {
                InteropImage.RequestRender();
                lastRender = args.RenderingTime;
            }
        }
        #endregion

    }
}
