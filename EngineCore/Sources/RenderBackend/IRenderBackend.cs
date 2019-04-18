using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    public enum RenderPathEnum
    {
        Forward,
        Deffered,
        TiledForward,
    };

    public abstract class IFrameData
    {
        public virtual void Reset() { }
    }

    public class ScreenProperties
    {
        public int Width;
        public int Height;
        public float AspectRatio;
    }

    public interface IRenderBackend
    {
        ScreenProperties ScreenProps { get; }
        bool IsInitialized { get; }
        bool IsExitRequest { get; }

        void Initialize(Engine engine, params object[] prms);

        void ExitRequest();
        //void RenderFrame(IFrameData frameData);

        void Deinitialize();
    }
}
