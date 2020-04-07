using System;
using System.Collections.Generic;

namespace CopperCowEngine.ECS.Collections
{
    internal class ChunkedArray<T> where T : struct
    {
        public readonly int ChunkCapacity;

        public int Count { get; private set; }

        private readonly List<Chunk> _chunks;

        // private readonly Queue<int> _freeIndices;

        public ChunkedArray(int chunkCapacity)
        {
            ChunkCapacity = chunkCapacity;

            _chunks = new List<Chunk>() { new Chunk(chunkCapacity) };
            // _freeIndices = new Queue<int>();
            Count = 0;
        }

        private (int chunk, int itemIndex) CalculateChunkIndex(int index)
        {
            return (index / ChunkCapacity, index % ChunkCapacity);
        }

        public int Add(T item)
        {
            var (chunk, index) = CalculateChunkIndex(Count);

            if (chunk >= _chunks.Count)
            {
                _chunks.Add(new Chunk(ChunkCapacity));
            }

            _chunks[chunk].Items[index] = item;

            return Count++;
        }

        public ref T GetAt(int itemIndex)
        {
            var (chunk, index) = CalculateChunkIndex(itemIndex);
#if DEBUG
            if (itemIndex >= Count || chunk >= _chunks.Count)
            {
                throw new NullReferenceException();
            }
#endif
            return ref _chunks[chunk].Items[index];
        }

        public bool TryFind(in T item, out int index)
        {
            for (var i = 0; i < Count; i++)
            {
                if (!Equals(in item, ref GetAt(i)))
                {
                    continue;
                }
                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        public virtual bool Equals(in T item, ref T other)
        {
            return item.Equals(other);
        }

        /*
        public void RemoveAt(int itemIndex)
        {
            var (chunk, index) = CalculateChunkIndex(itemIndex);
            if (chunk >= _chunks.Count)
            {
                return;
            }

            _freeIndices.Enqueue(index);

            Count--;
        }
        */

        private class Chunk
        {
            public readonly T[] Items;

            public Chunk(int capacity)
            {
                Items = new T[capacity];
            }
        }
    }
    
    internal class ChunkedManagedArray<T> where T : class
    {
        public readonly int ChunkCapacity;

        public int Count { get; private set; }

        private readonly List<Chunk> _chunks;

        public ChunkedManagedArray(int chunkCapacity)
        {
            ChunkCapacity = chunkCapacity;
            _chunks = new List<Chunk>() { new Chunk(chunkCapacity) };
            Count = 0;
        }

        private (int chunk, int itemIndex) CalculateChunkIndex(int index)
        {
            return (index / ChunkCapacity, index % ChunkCapacity);
        }

        public int Add(T item)
        {
            var (chunk, index) = CalculateChunkIndex(Count);
            if (chunk >= _chunks.Count)
            {
                _chunks.Add(new Chunk(ChunkCapacity));
            }
            _chunks[chunk].Items[index] = item;

            return Count++;
        }

        public T GetAt(int itemIndex)
        {
            var (chunk, index) = CalculateChunkIndex(itemIndex);
#if DEBUG
            if (itemIndex >= Count || chunk >= _chunks.Count)
            {
                throw new NullReferenceException();
            }
#endif
            return _chunks[chunk].Items[index];
        }

        public bool TryFind(T item, out int index)
        {
            for (var i = 0; i < Count; i++)
            {
                if (!Equals(item, GetAt(i)))
                {
                    continue;
                }
                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        public virtual bool Equals(T item, T other)
        {
            return item.Equals(other);
        }

        private class Chunk
        {
            public readonly T[] Items;

            public Chunk(int capacity)
            {
                Items = new T[capacity];
            }
        }
    }
}
