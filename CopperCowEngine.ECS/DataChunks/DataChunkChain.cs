using System;
using System.Collections.Generic;
using System.Linq;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkChain : IDisposable, IEquatable<DataChunkChain>
    {
        private readonly DataChunkArchetype _archetype;

        private readonly int _chunkCapacity;

        // TODO: Interface to chunk api with this layout
        private readonly UnmanagedChunkLayoutElement[] _layout;

        public UnmanagedChunk[] Chunks;

        public int Count => Chunks.Length;

        public DataChunkChain(in DataChunkArchetype archetype, IEnumerable<ComponentType> componentTypes)
        {
            _archetype = archetype;

            var nonTagComponentTypes = componentTypes.Where(x => x.Size > 1).ToArray();

            _layout = new UnmanagedChunkLayoutElement[nonTagComponentTypes.Count()];

            for (var i = 0; i < nonTagComponentTypes.Length; i++)
            {
                var componentType = nonTagComponentTypes[i];
                _layout[i] = new UnmanagedChunkLayoutElement
                {
                    ItemSize = componentType.Size,
                    StartOffset = 0,
                    TypeHashCode = componentType.GetHashCode(),
                    TypeId = componentType.Id,
                };
            }

            Chunks = new[] { new UnmanagedChunk(_layout) };

            _chunkCapacity = this[0].Capacity;
        }

        private int CalculateIndexInArchetype(int chunkIndex, int indexInChunk)
        {
            return _chunkCapacity * chunkIndex + indexInChunk;
        }

        private (int chunkIndex, int entityIndex) SplitIndexInArchetype(int index)
        {
            return (index / _chunkCapacity, index % _chunkCapacity);
        }

        public ref T GetDataByIndex<T>(int index) where T : unmanaged, IComponentData
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            return ref Chunks[chunkIndex].GetDataByIndex<T>(entityIndex);
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

        public void SetDataByIndex<T>(int index, T data) where T : unmanaged, IComponentData
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            Chunks[chunkIndex].SetDataByIndex(entityIndex, data);
        }
        
        public void SetDataFromContainer(int index, UnmanagedContainer data)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);
#if DEBUG
            if (Chunks.Length <= chunkIndex)
            {
                throw new NullReferenceException();
            }
#endif
            Chunks[chunkIndex].SetDataFromContainer(entityIndex, data);
        }

        public void MoveDataToAnotherChunk(int index, int targetIndex, DataChunkChain targetChunkChain)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);

            var (targetChunkIndex, targetEntityIndex) = targetChunkChain.SplitIndexInArchetype(targetIndex);

            Chunks[chunkIndex].CopyDataToAnotherChunk(targetChunkChain.Chunks[targetChunkIndex], entityIndex, targetEntityIndex);
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
            Chunks[oldCount] = new UnmanagedChunk(_layout);

            ref var newChunk = ref Chunks[oldCount];

            return CalculateIndexInArchetype(oldCount, newChunk.Add(id));
        }

        public int RemoveByIndex(int index)
        {
            var (chunkIndex, entityIndex) = SplitIndexInArchetype(index);

            return Chunks[chunkIndex].RemoveByIndex(entityIndex);
        }

        private ref UnmanagedChunk this[int i] => ref Chunks[i];
        
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            
            for (var i = 0; i < Chunks.Length; i++)
            {
                this[i].Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DataChunkChain()
        {
            Dispose(false);
        }

        public bool Equals(DataChunkChain other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return  _chunkCapacity == other._chunkCapacity && _archetype.Equals(other._archetype);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is DataChunkChain other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_archetype, _chunkCapacity);
        }
    }
}
