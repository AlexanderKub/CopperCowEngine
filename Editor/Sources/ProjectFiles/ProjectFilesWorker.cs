using EngineCore;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace Editor
{
    internal class ProjectFilesWorker
    {
        private struct ProjectData
        {
            public string Name;
            public string EngineVersion;

            public bool IsEmpty() {
                return string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(EngineVersion);
            }
        }

        public static ProjectFilesWorker Instance
        {
            get {
                if (m_Instance == null) {
                    m_Instance = new ProjectFilesWorker();
                }
                return m_Instance;
            }
        }

        private static ProjectFilesWorker m_Instance;

        public ObservableCollection<ProjectLink> RecentProjectsList;
        private ProjectFilesWorker() {
            RecentProjectsList = new ObservableCollection<ProjectLink>();
            LoadCachedProjectsList();
        }

        #region Cached projects list
        private List<string> CachedProjectsSrc;
        private string CachedPListPath = "CachedPList.mdxprop";
        private void LoadCachedProjectsList() {
            try {
                using (FileStream stream = new FileStream(CachedPListPath, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream)) {
                    CachedProjectsSrc = new List<string>();
                    int n = reader.ReadInt32();
                    for (int i = 0; i < n; i++) {
                        CachedProjectsSrc.Add(reader.ReadString());
                    }
                }

                ProjectData data;
                RecentProjectsList.Clear();
                List<string> wrongPaths = new List<string>();
                foreach (string src in CachedProjectsSrc) {
                    LoadProjectMeta(src, out data);
                    if (!data.IsEmpty()) {
                        RecentProjectsList.Add(new ProjectLink {
                            Name = data.Name,
                            EngineVersion = data.EngineVersion,
                            Src = src,
                        });
                    } else {
                        wrongPaths.Add(src);
                    }
                }

                if (wrongPaths.Count > 0) {
                    foreach (string item in wrongPaths) {
                        CachedProjectsSrc.Remove(item);
                    }
                }
                Console.WriteLine("LoadCachedProjectsList Done!");
            } catch (FileNotFoundException ex) {
                SaveCachedProjectsList();
            } catch {
                Console.WriteLine("Invalid File");
            }
        }

        private void SaveCachedProjectsList() {
            if (CachedProjectsSrc == null) {
                CachedProjectsSrc = new List<string>();
            }
            using (FileStream stream = new FileStream(CachedPListPath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                int n = CachedProjectsSrc.Count;
                writer.Write(n);
                for (int i = 0; i < n; i++) {
                    writer.Write(CachedProjectsSrc[i]);
                }
            }
            Console.WriteLine("SaveCachedProjectsList Done!");
        }
        #endregion

        public bool CreateNewProject(string Name, string Location, out ProjectLink project) {
            string path = Path.Combine(Location, Name + ".mdproj");
            if (File.Exists(path)) {
                project = null;
                return false;
            }

            ProjectData data = new ProjectData() {
                Name = Name,
                EngineVersion = GetEngineVersion(),
            };

            using (StreamWriter file = File.CreateText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, data);
            }

            CachedProjectsSrc.Add(path);
            SaveCachedProjectsList();
            project = new ProjectLink() {
                Name = data.Name,
                EngineVersion = data.EngineVersion,
                Src = path,
            };
            RecentProjectsList.Add(project);

            string sourceDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            sourceDir = Path.Combine(sourceDir, "Assets");
            string targetDir = Path.GetDirectoryName(path);
            targetDir = Path.Combine(targetDir, "Assets");
            Copy(sourceDir, targetDir);

            return true;
        }

        private bool LoadProjectMeta(string path, out ProjectData data) {
            data = new ProjectData();
            if (!File.Exists(path)) {
                return false;
            }

            using (StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                data = (ProjectData)serializer.Deserialize(file, typeof(ProjectData));
            }

            if (!CachedProjectsSrc.Contains(path)) {
                CachedProjectsSrc.Add(path);
                SaveCachedProjectsList();
                RecentProjectsList.Add(new ProjectLink() {
                    Name = data.Name,
                    EngineVersion = data.EngineVersion,
                    Src = path,
                });
            }
            return true;
        }

        public bool LoadProject(string path) {
            ProjectData data;
            if (!LoadProjectMeta(path, out data)) {
                return false;
            }
            return true;
        }

        #region Engine Info
        public string GetEngineVersion() {
            int[] MainMajorMinor = Engine.GetVersion();
            string version = MainMajorMinor[0] + ".";
            version += MainMajorMinor[1] + ".";
            version += MainMajorMinor[2];
            return version;
        }

        public string GetEngineName() {
            return Engine.GetName();
        }
        #endregion

        #region File System Utils
        private static void Copy(string sourceDirectory, string targetDirectory) {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles()) {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion
    }
}
