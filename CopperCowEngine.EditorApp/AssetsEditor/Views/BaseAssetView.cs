using CopperCowEngine.AssetsManagement.AssetsMeta;

namespace CopperCowEngine.EditorApp.AssetsEditor.Views
{
    internal abstract class BaseAssetView
    {
        protected PreviewEngine EngineRef;

        public virtual void Init(PreviewEngine engine) 
        {
            EngineRef = engine;
        }

        public virtual void Show(string assetName) { }
        public virtual void Hide() { }
        public virtual void Update(BaseAsset asset) { }

        public virtual void ChangeZoom(float value) { }
        public virtual void ChangeYaw(float value) { }
        public virtual void ChangePitch(float value) { }
        public virtual void ChangePosView(int v) { }
    }
}
