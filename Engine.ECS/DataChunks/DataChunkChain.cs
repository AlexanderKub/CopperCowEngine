using System;
using System.Collections.Generic;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkChain
    {
        private readonly DataChunkArchetype _archetype;

        private readonly IReadOnlyList<ComponentType> _componentTypes;

        public DataChunk[] Chunks;

        public int Count => Chunks.Length;

        public DataChunkChain(in DataChunkArchetype archetype, IReadOnlyList<ComponentType> componentTypes)
        {
            _archetype = archetype;
            _componentTypes = componentTypes;
            Chunks = new[] { new DataChunk(archetype, componentTypes) };
        }

        private int CalculateIndexInArchetype(int chunkIndex, int indexInChunk)
        {
            return _archetype.ChunkCapacity * chunkIndex + indexInChunk;
        }

        private (int chunkIndex, int entityIndex) SplitIndexInArchetype(int index)
        {
            return (index / _archetype.ChunkCapacity, index % _archetype.ChunkCapacity);
        }

        public ref T GetDataByIndex<T>(int index, int componentTypeId) where T : struct, IComponentData
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
            var dataArrayIndex = _archetype.ComponentTypes.IndexOf(componentTypeId);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            return ref Chunks[chunkIndex].GetDataByIndex<T>(entityIndex, dataArrayIndex);
        }

        public object GetBoxedDataByIndex(int index, int componentTypeId)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
            var dataArrayIndex = _archetype.ComponentTypes.IndexOf(componentTypeId);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            return Chunks[chunkIndex].GetBoxedDataByIndex(entityIndex, dataArrayIndex);
        }

        public int GetEntityIdByIndex(int index)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            return Chunks[chunkIndex].GetEntityIdByIndex(entityIndex);
        }

        public void SetBoxedDataByIndex(int index, int componentTypeId, object data)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
            var dataArrayIndex = _archetype.ComponentTypes.IndexOf(componentTypeId);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            Chunks[chunkIndex].SetBoxedDataByIndex(entityIndex, dataArrayIndex, data);
        }

        public void SetDataByIndex<T>(int index, int componentTypeId, T data) where T : struct, IComponentData
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
            var dataArrayIndex = _archetype.ComponentTypes.IndexOf(componentTypeId);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            Chunks[chunkIndex].SetDataByIndex(entityIndex, dataArrayIndex, data);
        }

        public int Add(int id)
        {
            for (var j = 0; j < Chunks.Length; j++)
            {
                if (Chunks[j].Full)
                {
                    continue;
                }

                ref var chunk = ref Chunks[j];

                return CalculateIndexInArchetype(j, chunk.Add(id));
            }

            var oldCount = Chunks.Length;
            Array.Resize(ref Chunks, oldCount + 1);
            Chunks[oldCount] = new DataChunk(_archetype, _componentTypes);

            ref var newChunk = ref Chunks[oldCount];

            return CalculateIndexInArchetype(oldCount, newChunk.Add(id));
        }

        public int RemoveByIndex(int index)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);

            return Chunks[chunkIndex].RemoveByIndex(entityIndex);
        }

        public ref DataChunk this[int i] => ref Chunks[i];
    }
}
