using System.Windows;
using Editor.UIControls;
using AssetsManager.AssetsMeta;
using Editor.MVVM;
using AssetsManager;
using System.IO;

namespace Editor
{
    /// <summary>
    /// Interaction logic for AssetEditorWindow.xaml
    /// </summary>
    public partial class AssetEditorWindow : Window
    {
        //
        public AssetEditorWindow() {
            ProjectLink project = new ProjectLink() {
                Src = "C:\\Repos\\TestProject\\TestProject.cceproj",
            };
            CommonInitialize(project);
        }

        public AssetEditorWindow(ProjectLink project) {
            CommonInitialize(project);
        }

        private void CommonInitialize(ProjectLink project) {
            AssetsManagerInstance.GetManager().RootPath = Path.GetDirectoryName(project.Src);
            InitializeComponent();
            AssetEditorModelView ModelView = new AssetEditorModelView(project);
            DataContext = ModelView;
            RendererElement.EngineRef = ModelView.EngineRef;
            (AssetsTree.DataContext as AssetsTreeViewModel).OnAssetSelect += ModelView.SetPreviewAsset;
            ModelView.MAMV += (MaterialAssetModelView assetView) => {
                PropertyGridRef.SelectedObject = null;
                PropertyGridRef.SelectedObject = assetView;
            };
        }
    }
}
