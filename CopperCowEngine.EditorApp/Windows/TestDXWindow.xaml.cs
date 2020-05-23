using System.Windows;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.EditorApp.AssetsEditor;
using CopperCowEngine.EditorApp.UIControls;

namespace CopperCowEngine.EditorApp.Windows
{
    /// <summary>
    /// Interaction logic for TestDXWindow.xaml
    /// </summary>
    public partial class TestDXWindow : Window
    {
        internal PreviewEngine EngineRef { get; set; }
        
        public TestDXWindow() 
        {
            InitializeComponent();
            DataContext = this;
            EngineRef = new PreviewEngine();
            RendererElement.EngineRef = EngineRef;
            (AssetsTree.DataContext as AssetsTreeViewModel).OnAssetSelect += SetPreviewAsset;
        }

        private void SetPreviewAsset(MetaAsset SelectedAsset) 
        {
            EngineRef?.PreviewAsset(SelectedAsset);
        }
    }
}
