using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public struct EngineHolder : ISingletonComponentData
    {
        private Engine _engine;

        public Engine Engine
        {
            get => _engine;
            set
            {
                if (_engine == null)
                {
                    _engine = value;
                }
            }
        }
    }
}
