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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window {
        
        private LaunchWindowModelView ModelView;
        public LaunchWindow() {
            InitializeComponent();
            ModelView = new LaunchWindowModelView();
            ModelView.OnProjectLoadRequest += (ProjectLink project) => {
                LaunchProjectEditor(project);
            };
            DataContext = ModelView;
        }

        public void LaunchProjectEditor(ProjectLink project) {
            this.Hide();
            Window editorWindowRef = new AssetEditorWindow();
            editorWindowRef.Owner = this.Owner;
            editorWindowRef.ShowDialog ();
            this.Close();
        }
    }

    
}
