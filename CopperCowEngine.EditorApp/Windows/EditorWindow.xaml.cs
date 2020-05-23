using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.EditorApp.AssetsEditor;

namespace CopperCowEngine.EditorApp.Windows
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public MaterialAsset TestPropObj { get; set; }
        //public object RendererComponent { get; set; }

        private PreviewEngine _engine;
        public EditorWindow() {
            InitializeComponent();
            TestPropObj = new MaterialAsset() {
                AlbedoMapAsset = "Test it!",
            };
            StartEngine();
            DataContext = this;
        }

        private void StartEngine() 
        {
            _engine = new PreviewEngine();
            RendererElement.EngineRef = _engine;
            _engine.OnSetViewsControlsEnabled += (bool zoom, bool yaw, bool pitch, bool viewPos, bool meshType) => {
                Visibility vis = zoom ? Visibility.Visible : Visibility.Collapsed;
                ZoomSlider.Visibility = vis;
                ZoomLabel.Visibility = vis;
                vis = yaw ? Visibility.Visible : Visibility.Collapsed;
                YawSlider.Visibility = zoom ? Visibility.Visible : Visibility.Collapsed;
                YawLabel.Visibility = zoom ? Visibility.Visible : Visibility.Collapsed;
                vis = pitch ? Visibility.Visible : Visibility.Collapsed;
                PitchSlider.Visibility = vis;
                PitchLabel.Visibility = vis;
                vis = viewPos ? Visibility.Visible : Visibility.Collapsed;
                ViewsList.Visibility = vis;
                vis = meshType ? Visibility.Visible : Visibility.Collapsed;
                MeshList.Visibility = vis;
            };
            InitAssetsManager();
        }
        
        private void Panel_Click(object sender, EventArgs e) {
            _engine?.MeshViewChangePivotAndFileScale(new Vector3(0, 0, -300f), 0.003f);
            //var AM = AssetsManagerInstance.GetManager();
            //AM.ImportAsset("PBR/DefferedPBRQuadShader.hlsl", "DefferedPBRQuadShader");
            //AM.ImportAsset("Unlit/ReflectionShader.hlsl", "ReflectionShader");
            //AM.ImportAsset("PBR/DefferedPBRShader.hlsl", "DefferedPBRShader");

            //RenderStarted = true;
            //InitAssetsManager();
            //StartEngine();
        }

        private Dictionary<AssetTypes, List<MetaAsset>> _metaAssets;
        private void InitAssetsManager() {
            var assetManager = EditorAssetsManager.GetManager();
            //AssetManager.CreateMaterialAsset("CopperMaterial");
            _metaAssets = assetManager.LoadProjectAssets();
            //Debug print tree
            //int k = 0;
            //foreach (AssetTypes key in MetaAssets.Keys) {
            //    Console.WriteLine("[{0} assets]", key);
            //    string[] names = new string[MetaAssets[key].Count];
            //    if (MetaAssets[key].Count == 0) {
            //        Console.WriteLine("  - [empty list]");
            //    }
            //    int i = 0;
            //    foreach (MetaAsset asset in MetaAssets[key]) {
            //        names[i] = asset.Name;
            //        i++;
            //        Console.WriteLine("  - {0}", asset.Name);
            //    }
            //    k++;
            //}
            AssetTypesDataList = _metaAssets.Keys;
            SetAssetTypesListValues();
            AssetTypesList.SelectedIndex = 0;
        }

        private void RefreshAssetsTable() 
        {
            var assetManager = EditorAssetsManager.GetManager();
            _metaAssets = assetManager.LoadProjectAssets();
            var k = 0;
            foreach (var key in _metaAssets.Keys) 
            {
                var names = new string[_metaAssets[key].Count];
                var i = 0;
                foreach (var asset in _metaAssets[key]) 
                {
                    names[i] = asset.Name;
                    i++;
                }
                k++;
            }
            AssetTypesDataList = _metaAssets.Keys;
            AssetTypesList.ItemsSource = AssetTypesDataList;
            AssetNamesList.ItemsSource = _metaAssets[_selectedType];
        }

        //WPF Logic code
        private AssetTypes _selectedType = AssetTypes.Invalid;
        private MetaAsset _selectedAsset;
        public IEnumerable<AssetTypes> AssetTypesDataList { get; private set; }

        private void SetAssetTypesListValues() 
        {
            AssetTypesList.ItemsSource = AssetTypesDataList;
            AssetTypesList.SelectionChanged += new SelectionChangedEventHandler((o, e) => {
                _selectedType = (AssetTypes)AssetTypesList.SelectedItem;
                AssetNamesList.ItemsSource = _metaAssets[_selectedType];
            });
            AssetNamesList.SelectionChanged += new SelectionChangedEventHandler((o, e) => {
                _selectedAsset = (MetaAsset)AssetNamesList.SelectedItem;
                SetPreviewAsset(_selectedAsset);
                ZoomSlider.Value = 1.0;
                YawSlider.Value = 0;
                PitchSlider.Value = 0;
                ViewsList.SelectedIndex = 0;
            });
        }

        private void SetPreviewAsset(MetaAsset selectedAsset) 
        {
            _engine?.PreviewAsset(selectedAsset);
        }

        private System.Windows.Forms.OpenFileDialog _ofDialog;
        private void Button_Click(object sender, RoutedEventArgs e) 
        {
            if (_ofDialog == null)
            {
                _ofDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = "Raw assets|*.obj;*.fbx;*.bmp;*.jpg;*.png|Shader source code|*.hlsl|All files|*.*"
                };
            }
            _ofDialog.ShowDialog();
        }

        private void Create_Click(object sender, RoutedEventArgs e) 
        {
            if (_ofDialog?.SafeFileName == null)
            {
                return;
            }

            var assetName = _ofDialog.SafeFileName.Split('.')[0];
            if (AssetsImporter.ImportAsset(_ofDialog.FileName, assetName, false, out var asset)) 
            {
                RefreshAssetsTable();
                AssetTypesList.SelectedValue = asset.Type;
                AssetNamesList.SelectedValue = asset.Name;
            }
            else 
            {
                MessageBox.Show("Import Failed");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) 
        {
            _engine.TestSave();
        }

        private void PropertiesResetHandler(object sender, RoutedEventArgs e) 
        {
            MessageBox.Show("PropertiesResetHandler");
        }

        private void PropertiesSaveHandler(object sender, RoutedEventArgs e) 
        {
            MessageBox.Show("PropertiesSaveHandler");
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
        {
            _engine?.ChangeZoom((float)e.NewValue);
        }
        
        private void YawSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
        {
            if (Equals(sender, YawSlider)) 
            {
                _engine?.ChangeYaw((float)e.NewValue);
            } 
            else if (Equals(sender, PitchSlider)) 
            {
                _engine?.ChangePitch((float)e.NewValue);
            }
        }

        private void RenderPanelFill_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) 
        {
            var t = ZoomSlider.Value;
            if (e.Delta < 0) 
            {
                t *= 0.95f;
            } 
            else
            {
                t *= 1.05f;
            }
            t = Math.Round(t * 1000) / 1000;
            ZoomSlider.Value = t;
        }

        private void ViewsList_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            var v = ((ComboBox)sender).SelectedIndex;
            _engine?.ChangePosView(v);
        }

        private void ZoomLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            ZoomSlider.Value = 1.0;
        }

        private void YawLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            YawSlider.Value = 0.0;
        }

        private void PitchLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) 
        {
            PitchSlider.Value = 0.0;
        }

        private void RendererElement_MouseWheel(object sender, MouseWheelEventArgs e) {

        }
    }
}
