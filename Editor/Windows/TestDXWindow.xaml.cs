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
using System.Windows.Shapes;
using AssetsManager.AssetsMeta;
using Editor.UIControls;

namespace Editor.Windows
{
    /// <summary>
    /// Interaction logic for TestDXWindow.xaml
    /// </summary>
    public partial class TestDXWindow : Window
    {
        internal PreviewEngine EngineRef { get; set; }
        
        public TestDXWindow() {
            InitializeComponent();
            DataContext = this;
            EngineRef = new PreviewEngine();
            RendererElement.EngineRef = EngineRef;
            (AssetsTree.DataContext as AssetsTreeViewModel).OnAssetSelect += SetPreviewAsset;
        }

        private void SetPreviewAsset(MetaAsset SelectedAsset) {
            EngineRef?.PreviewAsset(SelectedAsset);
        }
    }
}
