using System.Windows.Controls;

namespace CopperCowEngine.EditorApp.UIControls
{
    /// <summary>
    /// Interaction logic for AssetsTreeControl.xaml
    /// </summary>
    public partial class AssetsTreeControl : UserControl
    {
        public AssetsTreeControl() 
        {
            InitializeComponent();
            DataContext = new AssetsTreeViewModel();
        }
    }
}
