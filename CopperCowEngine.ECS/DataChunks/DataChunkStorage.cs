using System;
using System.Collections.Generic;
using System.Linq;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkStorage : IDisposable
    {
        private readonly EcsContext _context;

        public readonly ComponentTypesStorage TypesStorage;

        public DataChunkArchetypesStorage ArchetypesStorage { get; }

        public readonly List<DataChunkChain> ChunksStorage;

        public DataChunkStorage(EcsContext context)
        {
            _context = context;
            TypesStorage = new ComponentTypesStorage();
            ArchetypesStorage = new DataChunkArchetypesStorage();
            ChunksStorage = new List<DataChunkChain>();
        }

        private void MoveData(int indexInOldArchetype, int oldArchetypeIndex, int indexInArchetype, int newArchetypeIndex)
        {
            ChunksStorage[oldArchetypeIndex].MoveDataToAnotherChunk(indexInOldArchetype, indexInArchetype,
                ChunksStorage[newArchetypeIndex]);
        }

        public int AddEntity(Entity entity, int archetypeIndex)
        {
            return ChunksStorage[archetypeIndex].Add(entity.Id);
        }

        public DataChunkArchetype GetArchetype(params Type[] types)
        {
            var componentsSize = 0;

            Span<int> array = stackalloc int[types.Length];

            for (var i = 0; i < types.Length; i++)
            {
                var index = TypesStorage.TryRegisterType(types[i], out var componentSize);
                componentsSize += componentSize;
                array[i] = index;
            }

            return new DataChunkArchetype(componentsSize, array.ToArray());
        }

        public ref T GetData<T>(int indexInArchetype, int archetypeIndex) where T : unmanaged, IComponentData
        {
#if DEBUG
            var componentTypeId = TypesStorage.TryRegisterType(typeof(T));
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), componentTypeId))
            {
                throw new NullReferenceException();
            }
#endif
            return ref ChunksStorage[archetypeIndex].GetDataByIndex<T>(indexInArchetype);
        }

        public int RemoveEntity(int indexInArchetype, int archetypeIndex)
        {
            return ChunksStorage[archetypeIndex].RemoveByIndex(indexInArchetype);
        }

        public void SetData<T>(int indexInArchetype, int archetypeIndex, T data) where T : unmanaged, IComponentData
        {
#if DEBUG
            var componentTypeId = TypesStorage.TryRegisterType(typeof(T));
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), componentTypeId))
            {
                throw new NullReferenceException();
            }
#endif
            ChunksStorage[archetypeIndex].SetDataByIndex(indexInArchetype, data);
        }
        
        public void SetDataFromContainer(int indexInArchetype, int archetypeIndex, UnmanagedContainer data)
        {
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), data.TypeId))
            {
                throw new NullReferenceException();
            }
#endif
            ChunksStorage[archetypeIndex].SetDataFromContainer(indexInArchetype, data);
        }
        
        public int TryRegisterArchetype(in DataChunkArchetype archetype)
        {
            if (archetype.Equals(DataChunkArchetype.Null))
            {
                return -1;
            }

            if (ArchetypesStorage.TryFind(in archetype, out var index))
            {
                return index;
            }

            var archetypeIndex = ArchetypesStorage.Add(archetype);

            var componentTypes = archetype.ComponentTypes.Select(t => TypesStorage.GetComponentTypeAtIndex(t)).ToArray();

            ChunksStorage.Add(new DataChunkChain(in archetype, componentTypes));

            return archetypeIndex;
        }

        public int UpdateEntity(Entity entity, int indexInOldArchetype, int oldArchetypeIndex, int newArchetypeIndex)
        {
            var indexInArchetype = -1;

            var nullNewArchetype = ArchetypesStorage.GetAt(newArchetypeIndex).Equals(DataChunkArchetype.Null);
            if (!nullNewArchetype)
            {
                indexInArchetype = AddEntity(entity, newArchetypeIndex);
            }

            var nullOldArchetype = oldArchetypeIndex == -1 || ArchetypesStorage.GetAt(oldArchetypeIndex).Equals(DataChunkArchetype.Null);

            if (!nullOldArchetype && !nullNewArchetype)
            {
                MoveData(indexInOldArchetype, oldArchetypeIndex, indexInArchetype, newArchetypeIndex);
            }

            if (nullOldArchetype)
            {
                return indexInArchetype;
            }

            var movedEntityId = RemoveEntity(indexInOldArchetype, oldArchetypeIndex);

            if (movedEntityId >= 0)
            {
                _context.EntitiesStorage.SetEntityArchetype(_context.EntitiesStorage.GetEntity(movedEntityId),
                    oldArchetypeIndex, indexInOldArchetype);
            }

            return indexInArchetype;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (var chunkChain in ChunksStorage)
            {
                chunkChain.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataChunkStorage()
        {
            Dispose(false);
        }
    }
}
