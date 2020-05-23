using System;
using System.Collections.Generic;
using System.Linq;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.EditorApp.MVVM;
using System.Windows.Forms;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.EditorApp.AssetsEditor;

namespace CopperCowEngine.EditorApp.UIControls
{
    public class AssetsTreeViewModel : BaseModelView
    {
        private AssetTypes _selectedFolder;
        public AssetTypes SelectedFolder 
        {
            get => _selectedFolder;
            set 
            {
                _selectedFolder = value;
                NotifyPropertyChanged("SelectedFolder");
                NotifyPropertyChanged("Files");
            }
        }

        private MetaAsset _selectedFile;
        public MetaAsset SelectedFile
        {
            get => _selectedFile;
            set 
            {
                _selectedFile = value;
                NotifyPropertyChanged("SelectedFile");
                if (_selectedFile != null) 
                {
                    OnAssetSelect?.Invoke(_selectedFile);
                }
            }
        }

        public AssetTypes[] Folders => MetaAssets?.Keys.ToArray();

        public MetaAsset[] Files => MetaAssets?[SelectedFolder].ToArray();

        private Dictionary<AssetTypes, List<MetaAsset>> MetaAssets;
        public Action<MetaAsset> OnAssetSelect;

        public AssetsTreeViewModel() {
            InitAssetsManager();
        }

        private void InitAssetsManager() {
            RefreshAssetsTable();
        }

        private void RefreshAssetsTable() 
        {
            var assetManager = EditorAssetsManager.GetManager();
            //AssetManager.CreateCubeMapAsset("C:\\Repos\\CopperCowEngine\\RawContent\\Textures\\Skybox\\miramarirrad.bmp", "MiraSkyboxIrradianceCubeMap");
            MetaAssets = assetManager.LoadProjectAssets();
            SelectedFolder = MetaAssets.Keys.ToArray()[0];
            NotifyPropertyChanged("Folders");
            NotifyPropertyChanged("Files");
            //FilesTreeDebugPrint();
        }

        private void CreateNewAsset(string type) {
            if (type != "Material") 
            {
                return;
            }
            
            var assetManager = EditorAssetsManager.GetManager();
            assetManager.CreateMaterialAsset();
            RefreshAssetsTable();
            SelectedFolder = AssetTypes.Material;
        }

        private void ImportAsset()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Raw assets|*.obj;*.fbx;*.bmp;*.jpg;*.png|Shader source code|*.hlsl|All files|*.*"
            };
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            var filePath = openFileDialog.FileName;
            var safeFileName = openFileDialog.SafeFileName;
            
            var assetName = safeFileName.Split('.')[0];
            if (AssetsImporter.ImportAsset(filePath, assetName, true, out var asset))
            {
                if (asset.Type == AssetTypes.Mesh)
                {
                    MeshAssetsLoader.DropCachedMesh(MeshAssetsLoader.GetGuid(assetName));
                }
                RefreshAssetsTable();
            } 
            else 
            {
                System.Windows.MessageBox.Show("Import Failed");
            }
            SelectedFolder = asset.Type;
        }

        private BaseCommand _createCommand;
        public BaseCommand CreateCommand {
            get 
            {
                return _createCommand ??= new BaseCommand(obj => {
                    CreateNewAsset(obj.ToString());
                }, obj => true);
            }
        }

        private BaseCommand _importCommand;
        public BaseCommand ImportCommand {
            get 
            {
                return _importCommand ??= new BaseCommand(obj => {
                    ImportAsset();
                }, obj => true);
            }
        }

        #region Debug
        private void FilesTreeDebugPrint() {
            var k = 0;
            foreach (var key in MetaAssets.Keys) 
            {
                Console.WriteLine("[{0} assets]", key);
                var names = new string[MetaAssets[key].Count];
                if (MetaAssets[key].Count == 0) 
                {
                    Console.WriteLine("  - [empty list]");
                }

                var i = 0;
                foreach (var asset in MetaAssets[key]) 
                {
                    names[i] = asset.Name;
                    i++;
                    Console.WriteLine("  - {0}", asset.Name);
                }
                k++;
            }
        }
        #endregion
    }
}
