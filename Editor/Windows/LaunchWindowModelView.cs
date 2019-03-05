using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Editor.MVVM;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using FDialogResult = System.Windows.Forms.DialogResult;

namespace Editor
{
    public class LaunchWindowModelView : BaseModelView
    {
        public string EngineName { get; set; }
        public string EngineVersion { get; set; }
        public ProjectLink NewProject { get; set; }
        public ObservableCollection<ProjectLink> ProjectsList { get; set; }

        public Action<ProjectLink> OnProjectLoadRequest;

        #region Properties
        private bool m_IsNewProjectState;
        public bool IsNewProjectState
        {
            get {
                return m_IsNewProjectState;
            }
            set {
                m_IsNewProjectState = value;
                NotifyPropertyChanged("IsNewProjectState");
            }
        }

        private ProjectLink m_SelectedProject;
        public ProjectLink SelectedProject
        {
            get {
                return m_SelectedProject;
            }
            set {
                m_SelectedProject = value;
                LoadProject(m_SelectedProject, true);
            }
        }

        private string m_StatusText;
        public string StatusText {
            get {
                return m_StatusText;
            }
            set {
                m_StatusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }
        #endregion

        public LaunchWindowModelView() {
            EngineName = ProjectFilesWorker.Instance.GetEngineName();
            StatusText = EngineName;
            EngineVersion = "v" + ProjectFilesWorker.Instance.GetEngineVersion();
            IsNewProjectState = false;
            NewProject = new ProjectLink() {
                Name = "NewProject",
                EngineVersion = EngineVersion,
                Src = "",
            };
            ProjectsList = ProjectFilesWorker.Instance.RecentProjectsList;
        }

        #region Commands
        private BaseCommand m_MainTabCommand;
        public BaseCommand MainTabCommand {
            get {
                return m_MainTabCommand ??
                  (m_MainTabCommand = new BaseCommand(obj => {
                      IsNewProjectState = false;
                  }));
            }
        }

        private BaseCommand m_NewProjectTabCommand;
        public BaseCommand NewProjectTabCommand {
            get {
                return m_NewProjectTabCommand ??
                  (m_NewProjectTabCommand = new BaseCommand(obj => {
                      NewProject.Name = "NewProject";
                      NewProject.Src = "";
                      IsNewProjectState = true;
                  }));
            }
        }

        private BaseCommand m_CreateNewProjectCommand;
        public BaseCommand CreateNewProjectCommand {
            get {
                return m_CreateNewProjectCommand ??
                    (m_CreateNewProjectCommand = new BaseCommand(CreateNewProject, (obj) => {
                        if (string.IsNullOrEmpty(NewProject.Name) || string.IsNullOrEmpty(NewProject.Src)) {
                            return false;
                        }
                        return true;
                    }));
            }
        }
        

        private BaseCommand m_BrowseNewLocationCommand;
        public BaseCommand BrowseNewLocationCommand {
            get {
                return m_BrowseNewLocationCommand ??
                    (m_BrowseNewLocationCommand = new BaseCommand(BrowseNewLocation));
            }
        }

        private BaseCommand m_BrowseExistProjectCommand;
        public BaseCommand BrowseExistProjectCommand
        {
            get {
                return m_BrowseExistProjectCommand ??
                    (m_BrowseExistProjectCommand = new BaseCommand(BrowseExistProject));
            }
        }
        #endregion

        public void BrowseNewLocation(object obj) {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog()) {
                FDialogResult result = fbd.ShowDialog();
                if (result == FDialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    NewProject.Src = fbd.SelectedPath;
                }
            }
        }

        public void BrowseExistProject(object obj) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Copper Cow Engine project|*.cceproj";
            if (openFileDialog.ShowDialog() == FDialogResult.Cancel) {
                return;
            }
            string FilePath = openFileDialog.FileName;
            StatusText = "Project loading...";
            LoadProject(FilePath);
            StatusText = EngineName;
        }

        public void CreateNewProject(object obj) {
            ProjectLink tmp;
            StatusText = "Project creating...";
            if (!ProjectFilesWorker.Instance.CreateNewProject(NewProject.Name, NewProject.Src, out tmp)) {
                //TODO: Error handle
                return;
            }
            LoadProject(tmp, true);
        }

        public void LoadProject(ProjectLink project, bool AndRun) {
            if (project == null || !LoadProject(project.Src) || !AndRun) {
                return;
            }
            StatusText = "Project loading...";
            Console.WriteLine("Run project {0}, EngineVersion: {1}\nPath: {2}", 
                project.Name, project.EngineVersion, project.Src);
            OnProjectLoadRequest?.Invoke(project);
        }

        public bool LoadProject(string path) {
            if (string.IsNullOrEmpty(path)) {
                return false;
            }
            if (!ProjectFilesWorker.Instance.LoadProject(path)) {
                //TODO: Error handle
                return false;
            }
            return true;
        }
    }

    public class ProjectLink : INotifyPropertyChanged
    {
        private string name;
        private string engineVersion;
        private string src;

        public string Name
        {
            get {
                return name;
            }
            set {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        public string EngineVersion
        {
            get {
                return engineVersion;
            }
            set {
                engineVersion = value;
                NotifyPropertyChanged("EngineVersion");
            }
        }
        public string Src
        {
            get {
                return src;
            }
            set {
                src = value;
                NotifyPropertyChanged("Src");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
