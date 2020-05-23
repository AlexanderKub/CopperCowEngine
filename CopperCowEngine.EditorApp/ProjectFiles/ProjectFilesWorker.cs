using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CopperCowEngine.EditorApp.Windows;

namespace CopperCowEngine.EditorApp.ProjectFiles
{
    internal class ProjectFilesWorker
    {
        private struct ProjectData
        {
            public string Name {get; set; }

            public string EngineVersion {get; set; }

            public bool IsEmpty() 
            {
                return string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(EngineVersion);
            }
        }

        public static ProjectFilesWorker Instance => _instance ??= new ProjectFilesWorker();

        private static ProjectFilesWorker _instance;

        public ObservableCollection<ProjectLink> RecentProjectsList { get; }

        private List<string> _cachedProjectsSrc;

        private const string CachedPListPath = "CachedPList.cceprop";

        private ProjectFilesWorker() 
        {
            RecentProjectsList = new ObservableCollection<ProjectLink>();
            LoadCachedProjectsList();
        }

        #region Cached projects list

        private void LoadCachedProjectsList() 
        {
            try 
            {
                using (var stream = new FileStream(CachedPListPath, FileMode.Open))
                {
                    using var reader = new BinaryReader(stream);
                    _cachedProjectsSrc = new List<string>();
                    var n = reader.ReadInt32();
                    for (var i = 0; i < n; i++) 
                    {
                        _cachedProjectsSrc.Add(reader.ReadString());
                    }
                }

                RecentProjectsList.Clear();
                var wrongPaths = new List<string>();

                foreach (var src in _cachedProjectsSrc) 
                {
                    LoadProjectMeta(src, out var data);

                    if (!data.IsEmpty()) 
                    {
                        RecentProjectsList.Add(new ProjectLink {
                            Name = data.Name,
                            EngineVersion = data.EngineVersion,
                            Src = src,
                        });
                    } 
                    else 
                    {
                        wrongPaths.Add(src);
                    }
                }

                if (wrongPaths.Count > 0) 
                {
                    foreach (var item in wrongPaths) 
                    {
                        _cachedProjectsSrc.Remove(item);
                    }
                }
                Console.WriteLine("LoadCachedProjectsList Done!");
            } 
            catch (FileNotFoundException) 
            {
                SaveCachedProjectsList();
            } 
            catch 
            {
                Console.WriteLine("Invalid File");
            }
        }

        private void SaveCachedProjectsList() 
        {
            if (_cachedProjectsSrc == null) 
            {
                _cachedProjectsSrc = new List<string>();
            }
            using (var stream = new FileStream(CachedPListPath, FileMode.Create))
            {
                using var writer = new BinaryWriter(stream);
                var n = _cachedProjectsSrc.Count;
                writer.Write(n);
                for (var i = 0; i < n; i++) 
                {
                    writer.Write(_cachedProjectsSrc[i]);
                }
            }

            Console.WriteLine("SaveCachedProjectsList Done!");
        }
        #endregion

        public bool CreateNewProject(string name, string location, out ProjectLink project) 
        {
            var path = Path.Combine(location, name + ".cceproj");

            if (File.Exists(path)) 
            {
                project = null;
                return false;
            }

            var data = new ProjectData 
            {
                Name = name,
                EngineVersion = GetEngineVersion(),
            };

            var jsonString = JsonSerializer.Serialize(data);
            File.WriteAllText(path, jsonString);

            _cachedProjectsSrc.Add(path);
            SaveCachedProjectsList();
            project = new ProjectLink
            {
                Name = data.Name,
                EngineVersion = data.EngineVersion,
                Src = path,
            };
            RecentProjectsList.Add(project);

            var sourceDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            sourceDir = Path.Combine(sourceDir, "Assets");
            var targetDir = Path.GetDirectoryName(path);
            targetDir = Path.Combine(targetDir, "Assets");
            Copy(sourceDir, targetDir);

            return true;
        }

        private bool LoadProjectMeta(string path, out ProjectData data)
        {
            data = new ProjectData();

            if (!File.Exists(path)) 
            {
                return false;
            }

            var test = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<ProjectData>(File.ReadAllText(path));

            if (_cachedProjectsSrc.Contains(path))
            {
                return true;
            }

            _cachedProjectsSrc.Add(path);
            SaveCachedProjectsList();
            RecentProjectsList.Add(new ProjectLink
            {
                Name = data.Name,
                EngineVersion = data.EngineVersion,
                Src = path,
            });
            return true;
        }

        public bool LoadProject(string path)
        {
            return LoadProjectMeta(path, out _);
        }

        #region Engine Info
        public string GetEngineVersion()
        {
            int[] mainMajorMinor = {0, 0, 1};//Engine.GetVersion();
            var version = mainMajorMinor[0] + ".";
            version += mainMajorMinor[1] + ".";
            version += mainMajorMinor[2];
            return version;
        }

        public string GetEngineName()
        {
            return "CopperCowEngine"; //Engine.GetName();
        }
        #endregion

        #region File System Utils
        private static void Copy(string sourceDirectory, string targetDirectory) 
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles()) 
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories()) 
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
        #endregion
    }
}
