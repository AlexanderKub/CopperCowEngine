using System.Windows;

namespace CopperCowEngine.EditorApp.Windows
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window {
        public LaunchWindow() 
        {
            InitializeComponent();
            var modelView = new LaunchWindowModelView();
            modelView.ProjectLoadRequest += LaunchProjectEditor;
            DataContext = modelView;
        }

        public void LaunchProjectEditor(ProjectLink project) 
        {
            this.Hide();
            Window editorWindowRef = new AssetEditorWindow(project);
            editorWindowRef.Owner = Owner;
            editorWindowRef.ShowDialog ();
            this.Close();
        }
    }

    
}
