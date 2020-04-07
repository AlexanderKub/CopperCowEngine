using System;
using System.Collections.Generic;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.Rendering
{
    public abstract class FrameData
    {
        public virtual void Reset()
        {
        }
    }

    public class ScreenProperties
    {
        public float AspectRatio;
        public int Height;
        public int Width;
    }

    public interface IRenderBackend
    {
        RenderingConfiguration Configuration { get; }

        FrameData CurrentFrameData { get; }

        bool IsExitRequest { get; }

        bool IsInitialized { get; }

        ScreenProperties ScreenProps { get; }

        void BrdfIntegrate(string outputName);


        byte[] CompileAndImportShader(string path, ShaderType type, string entryPoint,
            Dictionary<string, object> macro);

        void CubeMapPrerender(string path, string outputName);

        void Deinitialize();

        void ExitRequest();

        TextureCubeAssetData ImportCubeTexture(string path);

        TextureAssetData ImportTexture(string path, bool forceSRgb);

        void Initialize(RenderingConfiguration config, params object[] parameters);

        void RenderFrame();

        event Action OnDrawCall;

        event Action OnFrameRenderStart;

        event Action OnFrameRenderEnd;

        event Action<ScreenProperties> OnScreenPropertiesChanged;
    }
}