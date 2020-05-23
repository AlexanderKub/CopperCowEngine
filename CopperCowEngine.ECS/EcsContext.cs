using System;
using System.Runtime.CompilerServices;
using CopperCowEngine.ECS.Base;
using CopperCowEngine.ECS.DataChunks;
using CopperCowEngine.Unsafe;

[assembly: InternalsVisibleTo("CopperCowEngine.ECS.Tests")]

namespace CopperCowEngine.ECS
{
    public partial class EcsContext : IDisposable
    {
        //internal readonly EntitiesStorage EntitiesStorage;
        internal readonly UnmanagedEntitiesStorage EntitiesStorage;

        internal readonly DataChunkStorage DataChunkStorage;

        private readonly SingletonComponentsDataStorage _singletonComponentsStorage;

        public EcsContext()
        {
            //EntitiesStorage = new EntitiesStorage();
            EntitiesStorage = new UnmanagedEntitiesStorage();

            DataChunkStorage = new DataChunkStorage(this);

            _singletonComponentsStorage = new SingletonComponentsDataStorage();
        }

        public void Update()
        {
            foreach (var system in _componentSystems)
            {
                system.Value.DeferredContext.Execute();
            }

            foreach (var system in _componentSystems)
            {
                system.Value.InternalUpdate();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            EntitiesStorage.Dispose();
            DataChunkStorage.Dispose();
            DisposeSystems();
            UnsafeUtility.Cleanup();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
