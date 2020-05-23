using CopperCowEngine.Engine;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public struct EngineHolder : ISingletonComponentData
    {
        private Engine.Engine _engine;

        public Engine.Engine Engine
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
