using System.IO;
using System.Windows;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.EditorApp.AssetsEditor;
using CopperCowEngine.EditorApp.MVVM.AssetsEditor;
using CopperCowEngine.EditorApp.UIControls;
using CopperCowEngine.EditorApp.UIControls.SharpDXControl;

namespace CopperCowEngine.EditorApp.Windows
{
    /// <summary>
    /// Interaction logic for AssetEditorWindow.xaml
    /// </summary>
    public partial class AssetEditorWindow : Window
    {
        //
        public AssetEditorWindow() 
        {
            var project = new ProjectLink
            {
                Src = "C:\\Repos\\TestProject\\TestProject.cceproj",
            };
            CommonInitialize(project);
        }

        public AssetEditorWindow(ProjectLink project) 
        {
            CommonInitialize(project);
        }

        private void CommonInitialize(ProjectLink project) 
        {
            AssetsManager.GetManager().RootPath = Path.GetDirectoryName(project.Src);

            InitializeComponent();

            var modelView = new AssetEditorModelView(project);
            
            DataContext = modelView;

            var previewEngine = new PreviewEngine();
            modelView.SetEngineRef(previewEngine);
            RendererElement.EngineRef = previewEngine;
            //RendererElement.OnTargetChanged += previewEngine.AttachRenderPanel;

            if (AssetsTree.DataContext is AssetsTreeViewModel assetsTreeDataContext)
            {
                assetsTreeDataContext.OnAssetSelect += modelView.SetPreviewAsset;
            }

            modelView.MaterialAssetModelView += (assetView) => 
            {
                PropertyGridRef.SelectedObject = null;
                PropertyGridRef.SelectedObject = assetView;
            };
        }
    }
}
