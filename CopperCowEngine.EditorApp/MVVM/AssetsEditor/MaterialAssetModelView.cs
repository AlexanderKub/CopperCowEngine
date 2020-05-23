using System.ComponentModel;
using System.Numerics;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.AssetsManagement.Editor;
using CopperCowEngine.EditorApp.AssetsEditor;
using CopperCowEngine.EditorApp.UIControls;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Color = System.Windows.Media.Color;
using Math = System.Math;

namespace CopperCowEngine.EditorApp.MVVM.AssetsEditor
{
    internal class MaterialAssetModelView
    {
        private const string NoneTexture = "--NONE--";

        private PreviewEngine EngineRef;
        private MaterialAsset _materialAsset;
        private MaterialAsset _backupAsset;

        private bool DirtyFlag;

        [Category("Common")]
        [DisplayName("Albedo Color (RGB)")]
        [Description("This property used only then Albedo Map is NONE")]
        public Color AlbedoColor
        {
            get {
                var c = new Color() {
                    R = (byte)Math.Floor(_materialAsset.AlbedoColor.X * 255),
                    G = (byte)Math.Floor(_materialAsset.AlbedoColor.Y * 255),
                    B = (byte)Math.Floor(_materialAsset.AlbedoColor.Z * 255),
                    A = 255,
                };
                return c;
            }
            set 
            {
                _materialAsset.AlbedoColor = new Vector3(value.R / 255f, value.G / 255f, value.B / 255f);
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Common")]
        [DisplayName("Metallic Value")]
        [Description("This property used only then Metallic Map is NONE")]
        public float MetallicValue
        {
            get => _materialAsset.MetallicValue;
            set 
            {
                _materialAsset.MetallicValue = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Common")]
        [DisplayName("Roughness Value")]
        [Description("This property used only then Roughness Map is NONE")]
        public float RoughnessValue
        {
            get => _materialAsset.RoughnessValue;
            set 
            {
                _materialAsset.RoughnessValue = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Texture Maps")]
        [DisplayName("Albedo Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string AlbedoMapAsset {
            get {
                if (!string.IsNullOrEmpty(_materialAsset.AlbedoMapAsset)) 
                {
                    return _materialAsset.AlbedoMapAsset;
                }
                return NoneTexture;
            }
            set 
            {
                _materialAsset.AlbedoMapAsset = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Texture Maps")]
        [DisplayName("Metallic Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string MetallicMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(_materialAsset.MetallicMapAsset))
                {
                    return _materialAsset.MetallicMapAsset;
                }
                return NoneTexture;
            }
            set 
            {
                _materialAsset.MetallicMapAsset = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Texture Maps")]
        [DisplayName("Normal Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string NormalMapAsset
        {
            get 
            {
                if (!string.IsNullOrEmpty(_materialAsset.NormalMapAsset)) 
                {
                    return _materialAsset.NormalMapAsset;
                }
                return NoneTexture;
            }
            set 
            {
                _materialAsset.NormalMapAsset = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("Texture Maps")]
        [DisplayName("Occlusion Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string OcclusionMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(_materialAsset.OcclusionMapAsset)) 
                {
                    return _materialAsset.OcclusionMapAsset;
                }
                return NoneTexture;
            }
            set 
            {
                _materialAsset.OcclusionMapAsset = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }
        [Category("Texture Maps")]
        [DisplayName("Roughness Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string RoughnessMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(_materialAsset.RoughnessMapAsset)) 
                {
                    return _materialAsset.RoughnessMapAsset;
                }
                return NoneTexture;
            }
            set 
            {
                _materialAsset.RoughnessMapAsset = value;
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("UV Transforms")]
        [DisplayName("UV Shift")]
        [Editor(typeof(UiVector2UserControlEditor), typeof(UiVector2UserControlEditor))]
        public UIVector2 Shift
        {
            get => new UIVector2(_materialAsset.Shift.X, _materialAsset.Shift.Y);
            set 
            {
                _materialAsset.Shift = new Vector2((float)value.X, (float)value.Y);
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        [Category("UV Transforms")]
        [DisplayName("UV Tile")]
        [Editor(typeof(UiVector2UserControlEditor), typeof(UiVector2UserControlEditor))]
        public UIVector2 Tile
        {
            get => new UIVector2(_materialAsset.Tile.X, _materialAsset.Tile.Y);
            set 
            {
                _materialAsset.Tile = new Vector2((float)value.X, (float)value.Y);
                EngineRef?.UpdateAssetPreview(_materialAsset);
            }
        }

        public bool IsEdited() 
        {
            DirtyFlag = !_materialAsset.IsSame(_backupAsset);
            return DirtyFlag;
        }

        public MaterialAssetModelView(MaterialAsset asset, PreviewEngine engine) 
        {
            EngineRef = engine;
            _materialAsset = asset;

            _backupAsset = EditorAssetsManager.CopyAsset(asset);
        }

        public void ResetAsset() 
        {
            if (!DirtyFlag) {
                return;
            }
            _materialAsset.CopyValues(_backupAsset);
            EngineRef?.UpdateAssetPreview(_materialAsset);
            DirtyFlag = false;
        }

        /*private static object GetPropValue(object src, string propName) {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }*/

        public void SaveChanging() 
        {
            if (!DirtyFlag) 
            {
                return;
            }

            var assetsManager = EditorAssetsManager.GetManager();
            assetsManager.SaveAssetChanging(_materialAsset);
            //TODO: save changing
            DirtyFlag = false;
        }
    }

    public class TextureMapAssetItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var assetsManager = EditorAssetsManager.GetManager();
            var table = assetsManager.LoadProjectAssets(false);
            var list = table[AssetTypes.Texture2D];

            var textures = new ItemCollection { "--NONE--" };

            foreach (var item in list) 
            {
                textures.Add(item.Name);
            }

            return textures;
        }
    }
}
