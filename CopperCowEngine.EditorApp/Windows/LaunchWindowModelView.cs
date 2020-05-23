using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CopperCowEngine.EditorApp.MVVM;
using CopperCowEngine.EditorApp.ProjectFiles;
using System.Windows.Forms;

namespace CopperCowEngine.EditorApp.Windows
{
    public class LaunchWindowModelView : BaseModelView
    {
        public string EngineName { get; }
        public string EngineVersion { get; }
        public ProjectLink NewProject { get; }
        public ObservableCollection<ProjectLink> ProjectsList { get; }

        public Action<ProjectLink> ProjectLoadRequest;

        #region Properties
        private bool _isNewProjectState;
        public bool IsNewProjectState
        {
            get => _isNewProjectState;
            set 
            {
                _isNewProjectState = value;
                NotifyPropertyChanged("IsNewProjectState");
            }
        }

        private ProjectLink _selectedProject;
        public ProjectLink SelectedProject
        {
            get => _selectedProject;
            set 
            {
                _selectedProject = value;
                LoadProject(_selectedProject, true);
            }
        }

        private string _statusText;
        public string StatusText 
        {
            get => _statusText;
            set 
            {
                _statusText = value;
                NotifyPropertyChanged("StatusText");
            }
        }
        #endregion

        public LaunchWindowModelView() 
        {
            EngineName = ProjectFilesWorker.Instance.GetEngineName();
            StatusText = EngineName;
            EngineVersion = "v" + ProjectFilesWorker.Instance.GetEngineVersion();
            IsNewProjectState = false;
            NewProject = new ProjectLink 
            {
                Name = "NewProject",
                EngineVersion = EngineVersion,
                Src = "",
            };
            ProjectsList = ProjectFilesWorker.Instance.RecentProjectsList;
        }

        #region Commands
        private BaseCommand _mainTabCommand;
        public BaseCommand MainTabCommand {
            get {
                return _mainTabCommand ??= new BaseCommand(obj => {
                    IsNewProjectState = false;
                });
            }
        }

        private BaseCommand _newProjectTabCommand;
        public BaseCommand NewProjectTabCommand 
        {
            get 
            {
                return _newProjectTabCommand ??= new BaseCommand(obj => {
                    NewProject.Name = "NewProject";
                    NewProject.Src = "";
                    IsNewProjectState = true;
                });
            }
        }

        private BaseCommand _createNewProjectCommand;
        public BaseCommand CreateNewProjectCommand {
            get {
                return _createNewProjectCommand ??= new BaseCommand(CreateNewProject, (obj) => 
                    !string.IsNullOrEmpty(NewProject.Name) && !string.IsNullOrEmpty(NewProject.Src));
            }
        }
        

        private BaseCommand _browseNewLocationCommand;
        public BaseCommand BrowseNewLocationCommand {
            get 
            {
                return _browseNewLocationCommand ??= new BaseCommand(BrowseNewLocation);
            }
        }

        private BaseCommand _browseExistProjectCommand;
        public BaseCommand BrowseExistProjectCommand
        {
            get 
            {
                return _browseExistProjectCommand ??= new BaseCommand(BrowseExistProject);
            }
        }
        #endregion

        public void BrowseNewLocation(object obj)
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) 
            {
                NewProject.Src = fbd.SelectedPath;
            }
        }

        public void BrowseExistProject(object obj) 
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Copper Cow Engine project|*.cceproj"
            };
            if (openFileDialog.ShowDialog() == DialogResult.Cancel) 
            {
                return;
            }
            var filePath = openFileDialog.FileName;
            StatusText = "Project loading...";
            LoadProject(filePath);
            StatusText = EngineName;
        }

        public void CreateNewProject(object obj) 
        {
            StatusText = "Project creating...";
            if (!ProjectFilesWorker.Instance.CreateNewProject(NewProject.Name, NewProject.Src, out var tmp)) 
            {
                //TODO: Error handle
                return;
            }
            LoadProject(tmp, true);
        }

        public void LoadProject(ProjectLink project, bool andRun) 
        {
            if (project == null || !LoadProject(project.Src) || !andRun) 
            {
                return;
            }

            StatusText = "Project loading...";
            Console.WriteLine("Run project {0}, EngineVersion: {1}\nPath: {2}", 
                project.Name, project.EngineVersion, project.Src);

            ProjectLoadRequest?.Invoke(project);
        }

        public bool LoadProject(string path) 
        {
            if (string.IsNullOrEmpty(path)) 
            {
                return false;
            }
            if (!ProjectFilesWorker.Instance.LoadProject(path)) 
            {
                //TODO: Error handle
                return false;
            }
            return true;
        }
    }

    public class ProjectLink : INotifyPropertyChanged
    {
        private string _name;
        private string _engineVersion;
        private string _src;

        public string Name
        {
            get => _name;
            set 
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string EngineVersion
        {
            get => _engineVersion;
            set 
            {
                _engineVersion = value;
                NotifyPropertyChanged("EngineVersion");
            }
        }

        public string Src
        {
            get => _src;
            set 
            {
                _src = value;
                NotifyPropertyChanged("Src");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
