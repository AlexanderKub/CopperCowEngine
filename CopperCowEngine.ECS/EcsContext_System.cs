using System.Collections.Generic;

namespace CopperCowEngine.ECS
{
    public partial class EcsContext
    {
        private readonly SortedList<int, ComponentSystem> _componentSystems = new SortedList<int, ComponentSystem>();

        protected T CreateSystem<T>(int order) where T : ComponentSystem, new()
        {
            var system = CreateSystemWithoutAutoUpdate<T>();

            AddSystemToContext(order, system);

            return system;
        }

        public T CreateSystemWithoutAutoUpdate<T>() where T : ComponentSystem, new()
        {
            var system = new T();

            system.InternalInit(this);

            return system;
        }

        private void AddSystemToContext(int order, ComponentSystem system)
        {
            _componentSystems.Add(order, system);
        }
        
        private void DisposeSystems()
        {
            foreach (var system in _componentSystems)
            {
                system.Value.Dispose();
            }
        }
    }
}
