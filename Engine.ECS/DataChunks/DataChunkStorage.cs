using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CopperCowEngine.ECS.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkStorage
    {
        internal readonly ComponentTypesStorage TypesStorage;

        internal DataChunkArchetypesStorage ArchetypesStorage { get; }

        public readonly List<DataChunkChain> ChunksStorage;

        private readonly EcsContext _context;

        public DataChunkStorage(EcsContext context)
        {
            _context = context;
            TypesStorage = new ComponentTypesStorage();
            ArchetypesStorage = new DataChunkArchetypesStorage();
            ChunksStorage = new List<DataChunkChain>();
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

        public ref T GetData<T>(int indexInArchetype, int archetypeIndex) where T : struct, IComponentData
        {
            var componentTypeId = TypesStorage.TryRegisterType(typeof(T));
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), componentTypeId))
            {
                throw new NullReferenceException();
            }
#endif
            return ref ChunksStorage[archetypeIndex].GetDataByIndex<T>(indexInArchetype, componentTypeId);
        }

        public int RemoveEntity(Entity entity, int indexInArchetype, int archetypeIndex)
        {
            return ChunksStorage[archetypeIndex].RemoveByIndex(indexInArchetype);
        }

        public void SetData<T>(int indexInArchetype, int archetypeIndex, T data) where T : struct, IComponentData
        {
            var componentTypeId = TypesStorage.TryRegisterType(typeof(T));
            ChunksStorage[archetypeIndex].SetDataByIndex(indexInArchetype, componentTypeId, data);
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

            var nullOldArchetype = ArchetypesStorage.GetAt(oldArchetypeIndex).Equals(DataChunkArchetype.Null);

            if (!nullOldArchetype && !nullNewArchetype)
            {
                var oldArchetype = ArchetypesStorage.GetAt(oldArchetypeIndex);
                var newArchetype = ArchetypesStorage.GetAt(newArchetypeIndex);

                foreach (var componentType in oldArchetype.ComponentTypes)
                {
                    if (!DataChunkArchetype.Compatibility(in newArchetype, componentType))
                    {
                        continue;
                    }
                    
                    var movedData = GetBoxedData(indexInOldArchetype, oldArchetypeIndex, componentType);
                    SetBoxedData(indexInArchetype, newArchetypeIndex, componentType, movedData);
                }
            }

            if (nullOldArchetype)
            {
                return indexInArchetype;
            }

            var movedEntityId = RemoveEntity(entity, indexInOldArchetype, oldArchetypeIndex);

            if (movedEntityId >= 0)
            {
                _context.EntitiesStorage.SetEntityArchetype(_context.EntitiesStorage.GetEntity(movedEntityId),
                    oldArchetypeIndex, indexInOldArchetype);
            }

            return indexInArchetype;
        }

        private object GetBoxedData(int indexInArchetype, int archetypeIndex, int componentTypeId)
        {
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), componentTypeId))
            {
                throw new NullReferenceException();
            }
#endif
            return ChunksStorage[archetypeIndex].GetBoxedDataByIndex(indexInArchetype, componentTypeId);
        }

        private void SetBoxedData(int indexInArchetype, int archetypeIndex, int componentTypeId, object data)
        {
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), componentTypeId))
            {
                throw new NullReferenceException();
            }
#endif
            ChunksStorage[archetypeIndex].SetBoxedDataByIndex(indexInArchetype, componentTypeId, data);
        }
    }
}
