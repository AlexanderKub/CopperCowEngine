using System;
using System.Numerics;
using CopperCowEngine.AssetsManagement.AssetsMeta;
using CopperCowEngine.ECS;
using CopperCowEngine.ECS.Builtin;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.EditorApp.AssetsEditor.Views;
using CopperCowEngine.Engine;
using CopperCowEngine.Rendering;
using CopperCowEngine.Rendering.D3D11;

namespace CopperCowEngine.EditorApp.AssetsEditor
{
    internal class PreviewEngine : IDisposable
    {
        private readonly Engine.Engine _engine;

        internal readonly EngineEcsContext EcsContext;

        private Entity CameraEntity { get; set; }

        private MeshAssetView _meshAssetViewRef;
        private Texture2DAssetView _texture2DAssetViewRef;
        private TextureCubeAssetView _textureCubeAssetViewRef;
        private MaterialAssetView _materialAssetViewRef;
        private BaseAssetView _currentAssetView;

        public PreviewEngine()
        {
            var renderingOption = new RenderingOptions<D3D11RenderBackend>(
                new RenderingConfiguration
                {
                    AppName = "PureProject",
                    RenderPath = RenderPathType.Forward,
                    DebugMode = true,
                    EnableMsaa = MsaaEnabled.Off,
                    EnableHdr = false,
                }, true, false);
            
            var loopProvider = new DefaultEngineLoopProvider();
            loopProvider.OnUpdate += OnAfterFrame;
            loopProvider.OnStart += OnEngineStart;
            loopProvider.OnQuit += OnEngineQuit;

            var configuration = new EngineConfiguration()
            {
                Rendering = renderingOption,
                EngineLoopProvider = loopProvider,
            };

            _engine = new Engine.Engine(configuration);

            EcsContext = new EngineEcsContext(_engine);

            _engine.Bootstrap();
        }

        /*public void AttachRenderPanel()
        {
        }*/

        public void MeshViewChangePivotAndFileScale(Vector3 pivot, float scale) 
        {
            //_meshAssetViewRef.ChangePivotAndFileScale(pivot, scale);
        }

        public void TestSave() 
        {
            //_meshAssetViewRef.ChangePivotAndFileScale(Vector3.BackwardLH * -0.5f, 1f);
           // _meshAssetViewRef.SaveChanging();
        }

        public void UpdateAssetPreview(BaseAsset asset) 
        {
            _currentAssetView.Update(asset);
        }
        
        public void PreviewAsset(MetaAsset asset) 
        {
            if (asset == null) {
                return;
            }
            
            _currentAssetView?.Hide();
            _currentAssetView = null;

            switch (asset.InfoType) {
                case AssetTypes.Invalid:
                    break;
                case AssetTypes.Mesh:
                    _currentAssetView = _meshAssetViewRef;
                    break;
                case AssetTypes.Texture2D:
                    _currentAssetView = _texture2DAssetViewRef;
                    break;
                case AssetTypes.TextureCube:
                    _currentAssetView = _textureCubeAssetViewRef;
                    break;
                case AssetTypes.Material:
                    _currentAssetView = _materialAssetViewRef;
                    break;
                case AssetTypes.Shader:
                    break;
                case AssetTypes.Meta:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _currentAssetView?.Show(asset.Name);
        }

        #region View UIControls
        public void ChangeZoom(float value) 
        {
            _currentAssetView?.ChangeZoom(value);
        }

        public void ChangeYaw(float value) 
        {
            _currentAssetView?.ChangeYaw(value / 180f * (float)Math.PI);
        }

        public void ChangePitch(float value) 
        {
            _currentAssetView?.ChangePitch(value / 180f * (float)Math.PI);
        }

        public void ChangePosView(int v) {
            _currentAssetView?.ChangePosView(v);
        }

        public Action<bool, bool, bool, bool, bool> OnSetViewsControlsEnabled;
        internal void SetViewsControlsEnabled(bool zoom, bool yaw, bool pitch, bool viewPos, bool meshType) 
        {
            OnSetViewsControlsEnabled?.Invoke(zoom, yaw, pitch, viewPos, meshType);
        }
        #endregion

        public void RequestFrame(IntPtr surface, bool isNewSurface)
        {
            EcsContext?.RequestFrame(surface, isNewSurface);
        }
        
        private void OnEngineStart()
        {
            CreateScene();
        }

        private void OnAfterFrame()
        {
            EcsContext.Update();
        }

        private void OnEngineQuit()
        {
            EcsContext.Dispose();
        }

        private void CreateScene() 
        {
            CameraEntity = EcsContext.CreateCameraEntity(CameraSetup.Default);
            /*EcsContext.AddComponent(CameraEntity, new FreeControl
            {
                Speed = 2f,
            });*/

            AssetViewsInit();
        }

        private void AssetViewsInit() 
        {
            _meshAssetViewRef = new MeshAssetView();
            _meshAssetViewRef.Init(this);
            _texture2DAssetViewRef = new Texture2DAssetView();
            _texture2DAssetViewRef.Init(this);
            _textureCubeAssetViewRef = new TextureCubeAssetView();
            _textureCubeAssetViewRef.Init(this);
            _materialAssetViewRef = new MaterialAssetView();
            _materialAssetViewRef.Init(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                EcsContext.RequestQuit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PreviewEngine()
        {
            Dispose(false);
        }
    }
}
