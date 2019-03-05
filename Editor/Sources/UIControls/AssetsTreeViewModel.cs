using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor.MVVM;
using AssetsManager;
using AssetsManager.AssetsMeta;

namespace Editor.UIControls
{
    public class AssetsTreeViewModel : BaseModelView
    {
        private AssetTypes m_SelectedFolder;
        public AssetTypes SelectedFolder {
            get {
                return m_SelectedFolder;
            }
            set {
                m_SelectedFolder = value;
                NotifyPropertyChanged("SelectedFolder");
                NotifyPropertyChanged("Files");
            }
        }

        private MetaAsset m_SelectedFile;
        public MetaAsset SelectedFile
        {
            get {
                return m_SelectedFile;
            }
            set {
                m_SelectedFile = value;
                NotifyPropertyChanged("SelectedFile");
                if (m_SelectedFile != null && OnAssetSelect != null) {
                    OnAssetSelect(m_SelectedFile);
                }
            }
        }

        public AssetTypes[] Folders
        {
            get {
                return MetaAssets?.Keys.ToArray();
            }
        }

        public MetaAsset[] Files
        {
            get {
                if (MetaAssets == null) {
                    return null;
                }
                return MetaAssets[SelectedFolder].ToArray();
            }
        }

        private Dictionary<AssetTypes, List<MetaAsset>> MetaAssets;
        public Action<MetaAsset> OnAssetSelect;

        public AssetsTreeViewModel() {
            InitAssetsManager();
        }

        private void InitAssetsManager() {
            RefreshAssetsTable();
        }

        private void RefreshAssetsTable() {
            AssetsManagerInstance AssetManager = AssetsManagerInstance.GetManager();
            //AssetManager.CreateCubeMapAsset("C:\\Repos\\CopperCowEngine\\RawContent\\Textures\\Skybox\\miramarirrad.bmp", "MiraSkyboxIrradianceCubeMap");
            MetaAssets = AssetManager.LoadProjectAssets();
            SelectedFolder = MetaAssets.Keys.ToArray()[0];
            NotifyPropertyChanged("Folders");
            NotifyPropertyChanged("Files");
            //FilesTreeDebugPrint();
        }

        private void CreateNewAsset(string type) {
            if (type != "Material") {
                return;
            }
            
            AssetsManagerInstance AssetManager = AssetsManagerInstance.GetManager();
            AssetManager.CreateMaterialAsset();
            RefreshAssetsTable();
            SelectedFolder = AssetTypes.Material;
        }

        private BaseCommand m_CreateCommand;
        public BaseCommand CreateCommand {
            get {
                return m_CreateCommand ??
                  (m_CreateCommand = new BaseCommand(obj => {
                      CreateNewAsset(obj.ToString());
                  }, obj => true));
            }
        }
        #region Debug
        private void FilesTreeDebugPrint() {
            int k = 0;
            foreach (AssetTypes key in MetaAssets.Keys) {
                Console.WriteLine("[{0} assets]", key);
                string[] names = new string[MetaAssets[key].Count];
                if (MetaAssets[key].Count == 0) {
                    Console.WriteLine("  - [empty list]");
                }
                int i = 0;
                foreach (MetaAsset asset in MetaAssets[key]) {
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
