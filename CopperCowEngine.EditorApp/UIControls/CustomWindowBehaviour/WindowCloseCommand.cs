using System;
using System.Windows;
using System.Windows.Input;

namespace CopperCowEngine.EditorApp.UIControls.CustomWindowBehaviour
{
    public class WindowCloseCommand :ICommand
    {     

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}
