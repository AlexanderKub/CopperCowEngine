using System.Windows;
using Editor.UIControls;
using AssetsManager.AssetsMeta;
using Editor.MVVM;

namespace Editor
{
    /// <summary>
    /// Interaction logic for AssetEditorWindow.xaml
    /// </summary>
    public partial class AssetEditorWindow : Window
    {

        public AssetEditorWindow() {
            InitializeComponent();
            AssetEditorModelView ModelView = new AssetEditorModelView();
            DataContext = ModelView;
            RendererElement.EngineRef = ModelView.EngineRef;
            (AssetsTree.DataContext as AssetsTreeViewModel).OnAssetSelect += ModelView.SetPreviewAsset;
            ModelView.MAMV += (MaterialAssetModelView assetView) => {
                PropertyGridRef.SelectedObject = assetView;
            };
        }
    }
}
