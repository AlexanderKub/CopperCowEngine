using CopperCowEngine.ECS.DataChunks;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS
{
    public partial class EcsContext
    {
        internal void AddComponent(Entity entity, int componentTypeId)
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);
            var componentSize = DataChunkStorage.TypesStorage.GetComponentTypeAtIndex(componentTypeId).Size;

            var archetype = oldArchetypeIndex == -1
                ? DataChunkArchetype.Null
                : DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex);
            var newArchetype = archetype.Extend(componentSize, componentTypeId);

            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);
            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);

            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }
        
        internal void RemoveComponent(Entity entity, int componentTypeId)
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            var componentSize = DataChunkStorage.TypesStorage.GetComponentTypeAtIndex(componentTypeId).Size;
            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Reduce(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);

            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);
            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }

        internal void SetComponentData(Entity entity, UnmanagedContainer data)
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (archetypeIndex, indexInArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);
            DataChunkStorage.SetDataFromContainer(indexInArchetype, archetypeIndex, data);
        }

        public void AddComponent<T>(Entity entity) where T : struct, IComponentData
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);
            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Extend(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);
            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);

            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }

        public void AddComponent<T>(Entity entity, T data) where T : unmanaged, IComponentData
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);
            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Extend(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);
            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);

            DataChunkStorage.SetData(indexInArchetype, newArchetypeIndex, data);
            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }

        public ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            var (archetypeIndex, index) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            return ref DataChunkStorage.GetData<T>(index, archetypeIndex);
        }

        public ref T GetSingletonComponent<T>() where T : struct, ISingletonComponentData
        {
            return ref _singletonComponentsStorage.GetSingletonComponent<T>();
        }

        public bool HasComponent<T>(Entity entity) where T : struct, IComponentData
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return false;
            }

            var archetypeIndex = EntitiesStorage.GetEntityArchetypeIndex(entity);

            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T));

            return DataChunkArchetype.Compatibility(in DataChunkStorage.ArchetypesStorage.GetAt(archetypeIndex), componentTypeId);
        }

        public void RemoveComponent<T>(Entity entity) where T : struct, IComponentData
        {
            if (!EntitiesStorage.IsEntityAlive(entity))
            {
                return;
            }

            var (oldArchetypeIndex, indexInOldArchetype) = EntitiesStorage.GetEntityArchetypeWithIndex(entity);

            var componentTypeId = DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            var newArchetype = DataChunkStorage.ArchetypesStorage.GetAt(oldArchetypeIndex).Reduce(componentSize, componentTypeId);
            var newArchetypeIndex = DataChunkStorage.TryRegisterArchetype(newArchetype);

            var indexInArchetype = DataChunkStorage.UpdateEntity(entity, indexInOldArchetype, oldArchetypeIndex, newArchetypeIndex);
            EntitiesStorage.SetEntityArchetype(entity, newArchetypeIndex, indexInArchetype);
        }
    }
}
