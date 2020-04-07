using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.Rendering
{
    public abstract class BaseRenderBackend : IRenderBackend
    {
        public abstract event Action OnDrawCall;

        public abstract event Action OnFrameRenderStart;

        public abstract event Action OnFrameRenderEnd;

        public abstract event Action<ScreenProperties> OnScreenPropertiesChanged;

        public abstract ScreenProperties ScreenProps { get; protected set; }

        public abstract bool IsInitialized { get; protected set; }

        public abstract bool IsExitRequest { get; protected set; }

        public FrameData CurrentFrameData { get; protected set; }

        public abstract RenderingConfiguration Configuration { get; protected set; }

        public abstract void BrdfIntegrate(string outputName);

        public abstract byte[] CompileAndImportShader(string path, ShaderType type, string entryPoint,
            Dictionary<string, object> macro);

        public abstract void CubeMapPrerender(string path, string outputName);

        public abstract void Deinitialize();

        public abstract void ExitRequest();

        public virtual void Initialize(RenderingConfiguration config, params object[] parameters)
        {
            Configuration = config;
        }

        public abstract TextureCubeAssetData ImportCubeTexture(string path);

        public abstract TextureAssetData ImportTexture(string path, bool forceSRgb);

        public abstract void RenderFrame();
    }
}