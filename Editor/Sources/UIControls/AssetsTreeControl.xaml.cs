using System;
using System.Windows.Controls;
using AssetsManager.AssetsMeta;

namespace Editor.UIControls
{
    /// <summary>
    /// Interaction logic for AssetsTreeControl.xaml
    /// </summary>
    public partial class AssetsTreeControl : UserControl
    {
        public AssetsTreeControl() {
            InitializeComponent();
            DataContext = new AssetsTreeViewModel();
        }
    }
}
