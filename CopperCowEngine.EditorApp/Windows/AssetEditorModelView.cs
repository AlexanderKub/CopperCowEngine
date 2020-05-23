using System;
using System.IO;
using CopperCowEngine.AssetsManagement;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.EditorApp.AssetsEditor;
using CopperCowEngine.EditorApp.MVVM;
using CopperCowEngine.EditorApp.MVVM.AssetsEditor;

namespace CopperCowEngine.EditorApp.Windows
{
    public class AssetEditorModelView : BaseModelView
    {
        private double _zoomValue = 1f;
        private float _yawValue;
        private float _pitchValue;
        private int _viewValue;

        private bool[] _controlsVisibleFlags = { false, false, false, false, false };

        private BaseCommand _resetPreviewValueCommand;
        private BaseCommand _saveChangingCommand;
        private MaterialAssetModelView _assetObject;
        private BaseCommand _resetAssetValues;

        internal Action<MaterialAssetModelView> MaterialAssetModelView;
        internal PreviewEngine EngineRef { get; set; }

        public AssetEditorModelView(ProjectLink project) 
        {
            AssetsManager.GetManager().RootPath = Path.GetDirectoryName(project.Src);
            //AssetsManagerInstance.GetManager().ImportAsset("PBR/DefferedPBRShader.hlsl", "DefferedPBRShader");
            //AssetsManagerInstance.GetManager().ImportAsset("PBR/DefferedPBRQuadShader.hlsl", "DefferedPBRQuadShader");
        }

        #region Preview controls

        public double ZoomValue 
        {
            get => _zoomValue;
            set 
            {
                _zoomValue = value;
                //_zoomValue = Math.Floor(value * 1000) / 1000;
                NotifyPropertyChanged("ZoomValue");
                EngineRef?.ChangeZoom((float)_zoomValue);
            }
        }

        public float YawValue
        {
            get => _yawValue;
            set 
            {
                _yawValue = value;
                NotifyPropertyChanged("YawValue");
                EngineRef?.ChangeYaw(_yawValue);
            }
        }

        public float PitchValue
        {
            get => _pitchValue;
            set {
                _pitchValue = value;
                NotifyPropertyChanged("PitchValue");
                EngineRef?.ChangePitch(_pitchValue);
            }
        }

        public int ViewValue
        {
            get => _viewValue;
            set {
                _viewValue = value;
                NotifyPropertyChanged("ViewValue");
                EngineRef?.ChangePosView(_viewValue);
            }
        }

        public bool[] ControlsVisibleFlags {
            get => _controlsVisibleFlags;
            set 
            {
                _controlsVisibleFlags = value;
                NotifyPropertyChanged("ControlsVisibleFlags");
            }
        }

        public BaseCommand ResetPreviewValueCommand
        {
            get 
            {
                return _resetPreviewValueCommand ??= new BaseCommand(obj =>
                {
                    switch (obj.ToString())
                    {
                        case "Zoom":
                            ZoomValue = 1f;
                            break;
                        case "Yaw":
                            YawValue = 0;
                            break;
                        case "Pitch":
                            PitchValue = 0;
                            break;
                    }
                });
            }
        }

        public BaseCommand SaveChangingCommand {
            get {
                return _saveChangingCommand ??= new BaseCommand(obj => 
                {
                    _assetObject.SaveChanging();
                });
            }
        }

        public BaseCommand ResetAssetValues {
            get {
                return _resetAssetValues ??= new BaseCommand(obj => {
                    AssetObject.ResetAsset();
                    MaterialAssetModelView?.Invoke(AssetObject);
                }, (obj) => AssetObject != null && AssetObject.IsEdited());
            }
        }

        internal MaterialAssetModelView AssetObject
        {
            get => _assetObject;
            set 
            {
                _assetObject = value;
                NotifyPropertyChanged("AssetObject");
            }
        }
        #endregion

        public void SetPreviewAsset(MetaAsset selectedAsset) 
        {
            ZoomValue = 1;
            YawValue = 0;
            PitchValue = 0;
            ViewValue = 0;
            ControlsVisibleFlags = new[] { false, false, false, false, false };

            var assetsManager = AssetsManager.GetManager();
            switch (selectedAsset.InfoType) 
            {
                case AssetTypes.Invalid:
                    break;
                case AssetTypes.Mesh:
                    break;
                case AssetTypes.Texture2D:
                    break;
                case AssetTypes.TextureCube:
                    break;
                case AssetTypes.Material:
                    var asset = assetsManager.LoadAsset<MaterialAsset>(selectedAsset.Name);
                    AssetObject = new MaterialAssetModelView(asset, EngineRef);
                    MaterialAssetModelView?.Invoke(AssetObject);
                    break;
                case AssetTypes.Shader:
                    break;
                case AssetTypes.Meta:
                    break;
                default:
                    break;
            }
            EngineRef?.PreviewAsset(selectedAsset);
        }
        
        internal void SetEngineRef(PreviewEngine engine)
        {
            EngineRef = engine;
            EngineRef.OnSetViewsControlsEnabled += (zoom, yaw, pitch, viewPos, meshType) => 
            {
                ControlsVisibleFlags = new[] { zoom, yaw, pitch, viewPos, meshType };
            };
        }
    }
}
