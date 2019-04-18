using AssetsManager;
using AssetsManager.AssetsMeta;
using Editor.AssetsEditor;
using Editor.MVVM;
using System;
using System.IO;

namespace Editor
{
    public class AssetEditorModelView : BaseModelView
    {
        internal PreviewEngine EngineRef { get; set; }

        #region Preview controls
        private double m_ZoomValue = 1f;
        public double ZoomValue {
            get {
                return m_ZoomValue;
            }
            set {
                m_ZoomValue = value;
                //m_ZoomValue = Math.Floor(value * 1000) / 1000;
                NotifyPropertyChanged("ZoomValue");
                EngineRef?.ChangeZoom((float)m_ZoomValue);
            }
        }

        private float m_YawValue = 0;
        public float YawValue
        {
            get {
                return m_YawValue;
            }
            set {
                m_YawValue = value;
                NotifyPropertyChanged("YawValue");
                EngineRef?.ChangeYaw(m_YawValue);
            }
        }

        private float m_PitchValue = 0;
        public float PitchValue
        {
            get {
                return m_PitchValue;
            }
            set {
                m_PitchValue = value;
                NotifyPropertyChanged("PitchValue");
                EngineRef?.ChangePitch(m_PitchValue);
            }
        }

        private int m_ViewValue = 0;
        public int ViewValue
        {
            get {
                return m_ViewValue;
            }
            set {
                m_ViewValue = value;
                NotifyPropertyChanged("ViewValue");
                EngineRef?.ChangePosView(m_ViewValue);
            }
        }

        private bool[] m_ControlsVisibleFlags = new bool[] { false, false, false, false, false };
        public bool[] ControlsVisibleFlags {
            get {
                return m_ControlsVisibleFlags;
            }
            set {
                m_ControlsVisibleFlags = value;
                NotifyPropertyChanged("ControlsVisibleFlags");
            }
        }

        private BaseCommand m_ResetPreviewValueCommand;
        public BaseCommand ResetPreviewValueCommand
        {
            get {
                return m_ResetPreviewValueCommand ??
                  (m_ResetPreviewValueCommand = new BaseCommand(obj => {
                      if (obj.ToString() == "Zoom") {
                          ZoomValue = 1f;
                          return;
                      }
                      if (obj.ToString() == "Yaw") {
                          YawValue = 0;
                      }
                      if (obj.ToString() == "Pitch") {
                          PitchValue = 0;
                      }
                  }));
            }
        }

        private BaseCommand m_SaveChangingCommand;
        public BaseCommand SaveChangingCommand {
            get {
                return m_SaveChangingCommand ??
                  (m_SaveChangingCommand = new BaseCommand(obj => {
                      m_AssetObject.SaveChanging();
                  }));
            }
        }

        private MaterialAssetModelView m_AssetObject;
        internal MaterialAssetModelView AssetObject
        {
            get {
                return m_AssetObject;
            }
            set {
                m_AssetObject = value;
                NotifyPropertyChanged("AssetObject");
            }
        }
        #endregion

        public AssetEditorModelView(ProjectLink project) {
            AssetsManagerInstance.GetManager().RootPath = Path.GetDirectoryName(project.Src);
            //AssetsManagerInstance.GetManager().ImportAsset("PBR/DefferedPBRShader.hlsl", "DefferedPBRShader");
            //AssetsManagerInstance.GetManager().ImportAsset("PBR/DefferedPBRQuadShader.hlsl", "DefferedPBRQuadShader");
            EngineRef = new PreviewEngine();

            EngineRef.OnSetViewsControlsEnabled += (bool zoom, bool yaw, bool pitch, bool viewPos, bool meshType) => {
                ControlsVisibleFlags = new bool[] { zoom, yaw, pitch, viewPos, meshType };
            };
        }

        private BaseCommand m_ResetAssetValues;
        public BaseCommand ResetAssetValues {
            get {
                return m_ResetAssetValues ??
                  (m_ResetAssetValues = new BaseCommand(obj => {
                      AssetObject.ResetAsset();
                      MAMV?.Invoke(AssetObject);
                  }, (obj) => {
                      return AssetObject != null && AssetObject.IsEdited();
                  }));
            }
        }

        internal Action<MaterialAssetModelView> MAMV;
        public void SetPreviewAsset(MetaAsset SelectedAsset) {
            ZoomValue = 1;
            YawValue = 0;
            PitchValue = 0;
            ViewValue = 0;
            ControlsVisibleFlags = new bool[] { false, false, false, false, false };

            var AM = AssetsManager.AssetsManagerInstance.GetManager();
            switch (SelectedAsset.InfoType) {
                case AssetTypes.Invalid:
                    break;
                case AssetTypes.Mesh:
                    break;
                case AssetTypes.Texture2D:
                    break;
                case AssetTypes.TextureCube:
                    break;
                case AssetTypes.Material:
                    MaterialAsset asset = AM.LoadAsset<MaterialAsset>(SelectedAsset.Name);
                    AssetObject = new MaterialAssetModelView(asset, EngineRef);
                    MAMV?.Invoke(AssetObject);
                    break;
                case AssetTypes.Shader:
                    break;
                case AssetTypes.Meta:
                    break;
                default:
                    break;
            }
            EngineRef?.PreviewAsset(SelectedAsset);
        }
    }
}
