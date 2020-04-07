using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkStorage
    {
        internal readonly TypesStorage TypesStorage;

        internal readonly ArchetypesStorage ArchetypesStorage;

        public readonly List<DataChunkChain> ChunksStorage;

        private readonly EcsContext _context;

        public DataChunkStorage(EcsContext context)
        {
            _context = context;
            TypesStorage = new TypesStorage();
            ArchetypesStorage = new ArchetypesStorage();
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
                var index = TypesStorage.TryRegisterType(types[i], out var componentType);
                componentsSize += componentType.Size;
                array[i] = index;
            }

            return new DataChunkArchetype(componentsSize, array.ToArray());
        }

        public ref T GetData<T>(int indexInArchetype, int archetypeIndex) where T : struct, IComponentData
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

        public int RemoveEntity(Entity entity, int indexInArchetype, int archetypeIndex)
        {
            return ChunksStorage[archetypeIndex].RemoveByIndex(indexInArchetype);
        }

        public void SetData<T>(int indexInArchetype, int archetypeIndex, T data) where T : struct, IComponentData
        {
            ChunksStorage[archetypeIndex].SetDataByIndex(indexInArchetype, data);
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

            ChunksStorage.Add(new DataChunkChain(in archetype));

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

                foreach (var type in oldArchetype.Types)
                {
                    if (!DataChunkArchetype.Compatibility(in newArchetype, type))
                    {
                        continue;
                    }
                    
                    var movedData = GetBoxedData(indexInOldArchetype, oldArchetypeIndex, type);
                    SetBoxedData(indexInArchetype, newArchetypeIndex, type, movedData);
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

        private object GetBoxedData(int indexInArchetype, int archetypeIndex, Type type)
        {
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), type))
            {
                throw new NullReferenceException();
            }
#endif
            return ChunksStorage[archetypeIndex].GetBoxedDataByIndex(indexInArchetype, type);
        }

        private void SetBoxedData(int indexInArchetype, int archetypeIndex, Type type, object data)
        {
#if DEBUG
            if (!DataChunkArchetype.Compatibility(in ArchetypesStorage.GetAt(archetypeIndex), type))
            {
                throw new NullReferenceException();
            }
#endif
            ChunksStorage[archetypeIndex].SetBoxedDataByIndex(indexInArchetype, type, data);
        }
    }
}
