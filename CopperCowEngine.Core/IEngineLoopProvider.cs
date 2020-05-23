using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Core
{
    public interface IEngineLoopProvider
    {
        void Render();
        void Start();
        void Quit();
        void Update();

        event Action OnRender;
        event Action OnStart;
        event Action OnQuit;
        event Action OnUpdate;
    }
}
