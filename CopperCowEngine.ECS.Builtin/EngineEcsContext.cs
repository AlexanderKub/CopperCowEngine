using System;
using System.Windows.Forms;
using CopperCowEngine.ECS.Builtin.Singletons;

namespace CopperCowEngine.ECS.Builtin
{
    public partial class EngineEcsContext : EcsContext
    {
        private readonly Engine.Engine _engine;

        public EngineEcsContext(Engine.Engine engine)
        {
            _engine = engine;
            GetSingletonComponent<EngineHolder>().Engine = _engine;

            CreateEngineSystems();

            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.Init();
            
            _engine.Input.OnKeyDown += KeyDown;
            _engine.Input.OnKeyUp += KeyUp;
            _engine.Input.OnKeyPress += KeyPress;
        }

        private void KeyDown(Keys keys)
        {
            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.KeyDown(keys);
        }

        private void KeyUp(Keys keys)
        {
            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.KeyUp(keys);
        }

        private void KeyPress(char keyChar)
        {
            ref var input = ref GetSingletonComponent<InputSingleton>();
            input.KeyPress(keyChar);
        }
        
        protected override void Dispose(bool disposing)
        {
            _engine.Input.OnKeyDown -= KeyDown;
            _engine.Input.OnKeyUp -= KeyUp;
            _engine.Input.OnKeyPress -= KeyPress;

            base.Dispose(disposing);
        }

        public void RequestQuit()
        {
            _engine.Quit();
        }

        public void RequestFrame(IntPtr surface, bool isNewSurface)
        {
            _engine.RequestFrame(surface, isNewSurface);
        }
    }
}
