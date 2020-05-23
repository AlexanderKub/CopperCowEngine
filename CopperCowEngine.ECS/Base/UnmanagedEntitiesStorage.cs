using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.Base
{
    internal sealed class UnmanagedEntitiesStorage : IDisposable
    {
        public const int MaxEntitiesCount = 21844; // ~256kb

        private UnmanagedList<StoredEntity> _entitiesUnmanagedArray;

        private readonly Queue<int> _freeIndices;

        private int _idGenerator = -1;

        public UnmanagedEntitiesStorage()
        {
            _entitiesUnmanagedArray = new UnmanagedList<StoredEntity>(512, MaxEntitiesCount);

            _freeIndices = new Queue<int>();
        }
        
        #region Entity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity()
        {
            var recycle = 0;
            if (!_freeIndices.TryDequeue(out var id))
            {
                recycle = 1;
                id = ++_idGenerator;
            }

            //ref var entityStore = ref _entitiesUnmanagedArray[id];
            var storedEntity = _entitiesUnmanagedArray[id];

            storedEntity.Version = (storedEntity.Version + 1) * recycle;
            storedEntity.ArchetypeIndex = -1;
            storedEntity.IndexInArchetype = -1;

            _entitiesUnmanagedArray[id] = storedEntity;

            return Entity.Recycle(id, storedEntity.Version);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(Entity entity)
        {
            //ref var stored = ref _entitiesUnmanagedArray[entity.Id];
            var storedEntity = _entitiesUnmanagedArray[entity.Id];

            storedEntity.ArchetypeIndex = -1;
            storedEntity.IndexInArchetype = -1;

            _entitiesUnmanagedArray[entity.Id] = storedEntity;

            _freeIndices.Enqueue(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntity(int id)
        {
            if (id < 0 || id > _idGenerator)
            {
                return Entity.Null;
            }
            return Entity.Recycle(id, _entitiesUnmanagedArray[id].Version);
        }

        public bool IsEntityAlive(Entity entity)
        {
            return GetEntity(entity.Id).Equals(entity);
        }
        #endregion

        #region Archetype
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetEntityArchetypeIndex(Entity entity)
        {
            return _entitiesUnmanagedArray[entity.Id].ArchetypeIndex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (int archetypeIndex, int index) GetEntityArchetypeWithIndex(Entity entity)
        {
            var storedEntity = _entitiesUnmanagedArray[entity.Id];

            return (storedEntity.ArchetypeIndex, storedEntity.IndexInArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetEntityArchetype(Entity entity, int archetypeIndex, int indexInArchetype)
        {
            //ref var storedEntity = ref _entitiesUnmanagedArray[entity.Id];
            var storedEntity = _entitiesUnmanagedArray[entity.Id];

            storedEntity.ArchetypeIndex = archetypeIndex;
            storedEntity.IndexInArchetype = indexInArchetype;

            _entitiesUnmanagedArray[entity.Id] = storedEntity;
        }
        #endregion
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _entitiesUnmanagedArray.Dispose();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct StoredEntity
        {
            public int ArchetypeIndex;

            public int IndexInArchetype;

            public int Version;
        }
    }
}
