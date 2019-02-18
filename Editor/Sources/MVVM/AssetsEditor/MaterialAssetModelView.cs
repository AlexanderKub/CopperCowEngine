using AssetsManager.AssetsMeta;
using Editor.AssetsEditor;
using Editor.UIControls;
using System.Collections.Generic;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Color = System.Windows.Media.Color;
using Math = System.Math;

namespace Editor.MVVM
{
    internal class MaterialAssetModelView
    {
        static private string NoneTexture = "--NONE--";

        [Category("Common")]
        [DisplayName("Albedo Color (RGB)")]
        [Description("This property used only then Albedo Map is NONE")]
        public Color AlbedoColor
        {
            get {
                var c = new Color() {
                    R = (byte)Math.Floor(m_Asset.AlbedoColor.X * 255),
                    G = (byte)Math.Floor(m_Asset.AlbedoColor.Y * 255),
                    B = (byte)Math.Floor(m_Asset.AlbedoColor.Z * 255),
                    A = 255,
                };
                return c;
            }
            set {
                m_Asset.AlbedoColor = new SharpDX.Vector3(value.R / 255f, value.G / 255f, value.B / 255f);
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Common")]
        [DisplayName("Metallic Value")]
        [Description("This property used only then Metallic Map is NONE")]
        public float MetallicValue
        {
            get {
                return m_Asset.MetallicValue;
            }
            set {
                m_Asset.MetallicValue = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Common")]
        [DisplayName("Roughness Value")]
        [Description("This property used only then Roughness Map is NONE")]
        public float RoughnessValue
        {
            get {
                return m_Asset.RoughnessValue;
            }
            set {
                m_Asset.RoughnessValue = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }

        [Category("Texture Maps")]
        [DisplayName("Albedo Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string AlbedoMapAsset {
            get {
                if (!string.IsNullOrEmpty(m_Asset.AlbedoMapAsset)) {
                    return m_Asset.AlbedoMapAsset;
                }
                return NoneTexture;
            }
            set {
                m_Asset.AlbedoMapAsset = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Texture Maps")]
        [DisplayName("Metallic Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string MetallicMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(m_Asset.MetallicMapAsset)) {
                    return m_Asset.MetallicMapAsset;
                }
                return NoneTexture;
            }
            set {
                m_Asset.MetallicMapAsset = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Texture Maps")]
        [DisplayName("Normal Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string NormalMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(m_Asset.NormalMapAsset)) {
                    return m_Asset.NormalMapAsset;
                }
                return NoneTexture;
            }
            set {
                m_Asset.NormalMapAsset = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Texture Maps")]
        [DisplayName("Occlusion Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string OcclusionMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(m_Asset.OcclusionMapAsset)) {
                    return m_Asset.OcclusionMapAsset;
                }
                return NoneTexture;
            }
            set {
                m_Asset.OcclusionMapAsset = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }
        [Category("Texture Maps")]
        [DisplayName("Roughness Map")]
        [ItemsSource(typeof(TextureMapAssetItemsSource))]
        public string RoughnessMapAsset
        {
            get {
                if (!string.IsNullOrEmpty(m_Asset.OcclusionMapAsset)) {
                    return m_Asset.OcclusionMapAsset;
                }
                return m_Asset.RoughnessMapAsset;
            }
            set {
                m_Asset.RoughnessMapAsset = value;
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }

        [Category("UV Transforms")]
        [DisplayName("UV Shift")]
        [Editor(typeof(UIVector2UserControlEditor), typeof(UIVector2UserControlEditor))]
        public UIVector2 Shift
        {
            get {
                return new UIVector2(m_Asset.Shift.X, m_Asset.Shift.Y);
            }
            set {
                m_Asset.Shift = new SharpDX.Vector2((float)value.X, (float)value.Y);
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }

        [Category("UV Transforms")]
        [DisplayName("UV Tile")]
        [Editor(typeof(UIVector2UserControlEditor), typeof(UIVector2UserControlEditor))]
        public UIVector2 Tile
        {
            get {
                return new UIVector2(m_Asset.Tile.X, m_Asset.Tile.Y);
            }
            set {
                m_Asset.Tile = new SharpDX.Vector2((float)value.X, (float)value.Y);
                EngineRef?.UpdateAssetPreview(m_Asset);
            }
        }

        private PreviewEngine EngineRef;
        private MaterialAsset m_Asset;
        private MaterialAsset m_BackupAsset;

        private bool DirtyFlag;

        public MaterialAssetModelView(MaterialAsset asset, PreviewEngine engine) {
            EngineRef = engine;
            m_Asset = asset;
            m_BackupAsset = new MaterialAsset(asset);
        }

        public void ResetAsset() {
            if (!DirtyFlag) {
                return;
            }
            //TODO: copy properties from backup asset
            DirtyFlag = false;
        }

        /*private static object GetPropValue(object src, string propName) {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }*/

        public void SaveChanging() {
            if (!DirtyFlag) {
                return;
            }
            //TODO: save changing
            DirtyFlag = false;
        }
    }

    public class TextureMapAssetItemsSource : IItemsSource
    {
        public ItemCollection GetValues() {
            var AM = AssetsManager.AssetsManagerInstance.GetManager();
            var table = AM.LoadProjectAssets(false);
            var list = table[AssetTypes.Texture2D];

            ItemCollection textures = new ItemCollection();
            textures.Add("--NONE--");
            foreach (var item in list) {
                textures.Add(item.Name);
            }
            return textures;
        }
    }
}
