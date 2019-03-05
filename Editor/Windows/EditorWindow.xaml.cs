using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor;
using Editor.AssetsEditor.Views;
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
using Xceed.Wpf.Toolkit;

namespace Editor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public MaterialAsset TestPropObj { get; set; }
        //public object RendererComponent { get; set; }

        private PreviewEngine engine;
        public EditorWindow() {
            InitializeComponent();
            TestPropObj = new MaterialAsset() {
                AlbedoMapAsset = "Test it!",
            };
            StartEngine();
            DataContext = this;
        }

        private void StartEngine() {
            engine = new PreviewEngine();
            RendererElement.EngineRef = engine;
            engine.OnSetViewsControlsEnabled += (bool zoom, bool yaw, bool pitch, bool viewPos, bool meshType) => {
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
            engine?.MeshViewChangePivotAndFileScale(new SharpDX.Vector3(0, 0, -300f), 0.003f);
            //var AM = AssetsManagerInstance.GetManager();
            //AM.ImportAsset("PBR/DefferedPBRQuadShader.hlsl", "DefferedPBRQuadShader");
            //AM.ImportAsset("Unlit/ReflectionShader.hlsl", "ReflectionShader");
            //AM.ImportAsset("PBR/DefferedPBRShader.hlsl", "DefferedPBRShader");

            //RenderStarted = true;
            //InitAssetsManager();
            //StartEngine();
        }

        Dictionary<AssetTypes, List<MetaAsset>> MetaAssets;
        private void InitAssetsManager() {
            AssetsManagerInstance AssetManager = AssetsManagerInstance.GetManager();
            //AssetManager.CreateMaterialAsset("CopperMaterial");
            MetaAssets = AssetManager.LoadProjectAssets();
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
            AssetTypesDataList = MetaAssets.Keys;
            SetAssetTypesListValues();
            this.AssetTypesList.SelectedIndex = 0;
        }

        private void RefreshAssetsTable() {
            AssetsManagerInstance AssetManager = AssetsManagerInstance.GetManager();
            MetaAssets = AssetManager.LoadProjectAssets();
            int k = 0;
            foreach (AssetTypes key in MetaAssets.Keys) {
                string[] names = new string[MetaAssets[key].Count];
                int i = 0;
                foreach (MetaAsset asset in MetaAssets[key]) {
                    names[i] = asset.Name;
                    i++;
                }
                k++;
            }
            AssetTypesDataList = MetaAssets.Keys;
            AssetTypesList.ItemsSource = AssetTypesDataList;
            AssetNamesList.ItemsSource = MetaAssets[SelectedType];
        }

        //WPF Logic code
        private AssetTypes SelectedType = AssetTypes.Invalid;
        private MetaAsset SelectedAsset;
        public IEnumerable<AssetTypes> AssetTypesDataList { get; private set; }

        private void SetAssetTypesListValues() {
            AssetTypesList.ItemsSource = AssetTypesDataList;
            AssetTypesList.SelectionChanged += new SelectionChangedEventHandler((object o, SelectionChangedEventArgs e) => {
                SelectedType = (AssetTypes)AssetTypesList.SelectedItem;
                AssetNamesList.ItemsSource = MetaAssets[SelectedType];
            });
            AssetNamesList.SelectionChanged += new SelectionChangedEventHandler((object o, SelectionChangedEventArgs e) => {
                SelectedAsset = (MetaAsset)AssetNamesList.SelectedItem;
                SetPreviewAsset(SelectedAsset);
                ZoomSlider.Value = 1.0;
                YawSlider.Value = 0;
                PitchSlider.Value = 0;
                ViewsList.SelectedIndex = 0;
            });
        }

        private void SetPreviewAsset(MetaAsset SelectedAsset) {
            engine?.PreviewAsset(SelectedAsset);
        }

        private System.Windows.Forms.OpenFileDialog ofDialog;
        private void Button_Click(object sender, RoutedEventArgs e) {
            if (ofDialog == null) {
                ofDialog = new System.Windows.Forms.OpenFileDialog();
                ofDialog.Filter = "Raw assets|*.obj;*.fbx;*.bmp;*.jpg;*.png|Shader source code|*.hlsl|All files|*.*";
            }
            ofDialog.ShowDialog();
        }

        private void Create_Click(object sender, RoutedEventArgs e) {
            if (ofDialog == null) {
                return;
            }
            AssetsManagerInstance AM = AssetsManagerInstance.GetManager();
            string assetName = ofDialog.SafeFileName.Split('.')[0];
            BaseAsset asset;
            if (AM.ImportAsset(ofDialog.FileName, assetName, false, out asset)) {
                RefreshAssetsTable();
                AssetTypesList.SelectedValue = asset.Type;
                AssetNamesList.SelectedValue = asset.Name;
            } else {
                System.Windows.MessageBox.Show("Import Failed");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            engine.TestSave();
        }

        private void PropertiesResetHandler(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show("PropertiesResetHandler");
        }

        private void PropertiesSaveHandler(object sender, RoutedEventArgs e) {
            System.Windows.MessageBox.Show("PropertiesSaveHandler");
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            engine?.ChangeZoom((float)e.NewValue);
        }
        
        private void YawSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (sender == YawSlider) {
                engine?.ChangeYaw((float)e.NewValue);
            } else if (sender == PitchSlider) {
                engine?.ChangePitch((float)e.NewValue);
            }
        }

        private void RenderPanelFill_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e) {
            double t = ZoomSlider.Value;
            if (e.Delta < 0) {
                t *= 0.95f;
            } else {
                t *= 1.05f;
            }
            t = Math.Round(t * 1000) / 1000;
            ZoomSlider.Value = t;
        }

        private void ViewsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int v = ((ComboBox)sender).SelectedIndex;
            engine?.ChangePosView(v);
        }

        private void ZoomLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ZoomSlider.Value = 1.0;
        }

        private void YawLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            YawSlider.Value = 0.0;
        }

        private void PitchLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            PitchSlider.Value = 0.0;
        }

        private void RendererElement_MouseWheel(object sender, MouseWheelEventArgs e) {

        }
    }
}
