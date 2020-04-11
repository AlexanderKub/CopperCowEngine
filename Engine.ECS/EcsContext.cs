using System;
using System.Collections.Generic;
using CopperCowEngine.ECS.Base;
using CopperCowEngine.ECS.DataChunks;

namespace CopperCowEngine.ECS
{
    public partial class EcsContext : IDisposable
    {
        internal readonly EntitiesStorage EntitiesStorage;

        internal readonly DataChunkStorage DataChunkStorage;

        internal readonly SingletonComponentsDataStorage SingletonComponentsStorage;

        public EcsContext()
        {
            EntitiesStorage = new EntitiesStorage();

            DataChunkStorage = new DataChunkStorage(this);

            SingletonComponentsStorage = new SingletonComponentsDataStorage();
        }

        public void AddComponent<T>(Entity entity, T data) where T : struct, IComponentData
        {
            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);
            // TODO: -1 out index
            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Extend(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);

            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);
            DataChunkStorage.SetData(indexInArchetype, newArchetypeIndex, data);

            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }

        public ref T GetComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var (archetypeIndex, index) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            return ref DataChunkStorage.GetData<T>(index, archetypeIndex);
        }

        public ref T GetSingletonComponent<T>() where T : struct, ISingletonComponentData
        {
            return ref SingletonComponentsStorage.GetSingletonComponent<T>();
        }

        public bool HasComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var archetypeIndex = EntitiesStorage.GetEntityArchetypeIndex(entity);

            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T));

            return DataChunkArchetype.Compatibility(in DataChunkStorage.ArchetypesStorage.GetAt(archetypeIndex), componentTypeId);
        }

        public void RemoveComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Reduce(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);

            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);
            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }

        public void RemoveEntity(int entityId)
        {
            RemoveEntity(EntitiesStorage.GetEntity(entityId));
        }

        public void RemoveEntity(Entity entity)
        {
            var (archetypeIndex, index) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            var movedEntityId = DataChunkStorage.RemoveEntity(entity, index, archetypeIndex);

            EntitiesStorage.SetEntityArchetype(EntitiesStorage.GetEntity(movedEntityId), archetypeIndex, index);

            EntitiesStorage.RemoveEntity(entity);
        }

        // TODO: Refactoring
        private readonly List<ComponentSystem> _componentSystems = new List<ComponentSystem>();

        public T CreateSystem<T>() where T : ComponentSystem, new()
        {
            var system = new T();
            system.Init(this);
            _componentSystems.Add(system);
            return system;
        }

        public void Update()
        {
            foreach (var system in _componentSystems)
            {
                system.InternalUpdate();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // TODO: Dispose allocations
        }
    }
}
