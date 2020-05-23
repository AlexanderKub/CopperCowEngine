using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace CopperCowEngine.EditorApp.UIControls.CustomWindowBehaviour
{
    public static class ShowSystemMenuBehavior
    {
        private static bool _leftButtonToggle = true;

        public static readonly DependencyProperty TargetWindow = DependencyProperty.RegisterAttached("TargetWindow", typeof(Window), typeof(ShowSystemMenuBehavior));  

        #region TargetWindow

        public static Window GetTargetWindow(DependencyObject obj)
        {
            return (Window)obj.GetValue(TargetWindow);
        }

        public static void SetTargetWindow(DependencyObject obj, Window window)
        {
            obj.SetValue(TargetWindow, window);
        }

        #endregion

        #region LeftButtonShowAt

        public static UIElement GetLeftButtonShowAt(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(LeftButtonShowAt);
        }

        public static void SetLeftButtonShowAt(DependencyObject obj, UIElement element)
        {
            obj.SetValue(LeftButtonShowAt, element);
        }

        public static readonly DependencyProperty LeftButtonShowAt = DependencyProperty.RegisterAttached("LeftButtonShowAt",
            typeof(UIElement), typeof(ShowSystemMenuBehavior),
            new UIPropertyMetadata(null, LeftButtonShowAtChanged));

        #endregion

        #region RightButtonShow

        public static bool GetRightButtonShow(DependencyObject obj)
        {
            return (bool)obj.GetValue(RightButtonShow);
        }

        public static void SetRightButtonShow(DependencyObject obj, bool arg)
        {
            obj.SetValue(RightButtonShow, arg);
        }

        public static readonly DependencyProperty RightButtonShow = DependencyProperty.RegisterAttached("RightButtonShow",
            typeof(bool), typeof(ShowSystemMenuBehavior),
            new UIPropertyMetadata(false, RightButtonShowChanged));

        #endregion

        #region LeftButtonShowAt

        private static void LeftButtonShowAtChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.MouseLeftButtonDown += LeftButtonDownShow;
            }
        }

        private static void LeftButtonDownShow(object sender, MouseButtonEventArgs e)
        {
            if (_leftButtonToggle)
            {
                var element = ((UIElement)sender).GetValue(LeftButtonShowAt);

                var showMenuAt = ((Visual)element).PointToScreen(new Point(0, 0));

                var targetWindow = ((UIElement)sender).GetValue(TargetWindow) as Window;

                SystemMenuManager.ShowMenu(targetWindow, showMenuAt);

                _leftButtonToggle = !_leftButtonToggle;
            }
            else
            {
                _leftButtonToggle = !_leftButtonToggle;
            }
        }

        #endregion

        #region RightButtonShow handlers

        private static void RightButtonShowChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element)
            {
                element.MouseRightButtonDown += RightButtonDownShow;
            }
        }

        private static void RightButtonDownShow(object sender, MouseButtonEventArgs e)
        {
            var element = (UIElement)sender;

            if (!(element.GetValue(TargetWindow) is Window targetWindow))
            {
                return;
            }
            var showMenuAt = targetWindow.PointToScreen(Mouse.GetPosition((targetWindow)));

            SystemMenuManager.ShowMenu(targetWindow, showMenuAt);
        }

        #endregion       
    }
}
