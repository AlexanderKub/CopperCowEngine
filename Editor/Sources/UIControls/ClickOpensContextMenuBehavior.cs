using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Editor.UIControls
{
    class ClickOpensContextMenuBehavior
    {
        private static readonly DependencyProperty ClickOpensContextMenuProperty =
        DependencyProperty.RegisterAttached(
          "Enabled", typeof(bool), typeof(ClickOpensContextMenuBehavior),
          new PropertyMetadata(new PropertyChangedCallback(HandlePropertyChanged))
        );

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(ClickOpensContextMenuProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(ClickOpensContextMenuProperty, value);
        }

        private static void HandlePropertyChanged(
          DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is Button)
            {
                var button = obj as Button;
                button.Click -= ExecuteClick;
                button.Click += ExecuteClick;
            }
        }

        /*private static void ExecuteMouseDown(object sender, MouseEventArgs args)
        {
            DependencyObject obj = sender as DependencyObject;
            bool enabled = (bool)obj.GetValue(ClickOpensContextMenuProperty);
            if (enabled)
            {
                if (sender is Button)
                {
                    var button = (Button)sender;
                    if (button.ContextMenu != null)
                        button.ContextMenu.IsOpen = true;
                }
            }
        }*/

        private static void ExecuteClick(object sender, RoutedEventArgs args)
        {
            DependencyObject obj = sender as DependencyObject;
            bool enabled = (bool)obj.GetValue(ClickOpensContextMenuProperty);
            if (enabled)
            {
                if (sender is Button)
                {
                    var button = (Button)sender;
                    if (button.ContextMenu != null)
                    {
                        button.ContextMenu.DataContext = button.DataContext;
                        button.ContextMenu.IsOpen = true;
                    }
                }
            }
        }
    }
}
