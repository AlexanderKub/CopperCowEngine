using CopperCowEngine.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Engine
{
    public class DefaultEngineLoopProvider : IEngineLoopProvider
    {
        public event Action OnRender;
        public event Action OnStart;
        public event Action OnQuit;
        public event Action OnUpdate;

        public void Render()
        {
            OnRender?.Invoke();
        }

        public void Start()
        {
            OnStart?.Invoke();
        }

        public void Quit()
        {
            OnQuit?.Invoke();
        }

        public void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}
