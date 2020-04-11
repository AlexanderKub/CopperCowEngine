using System;

namespace CopperCowEngine.ECS.Collections
{
    internal class ChunkedArray<T> where T : struct, IEquatable<T>
    {
        public int ChunkCapacity { get; }

        public int Count { get; private set; }

        private Chunk[] _chunks;

        public ChunkedArray(int chunkCapacity)
        {
            ChunkCapacity = chunkCapacity;

            _chunks = new [] { new Chunk(chunkCapacity) };
            Count = 0;
        }

        private (int chunk, int itemIndex) CalculateChunkIndex(int index)
        {
            return (index / ChunkCapacity, index % ChunkCapacity);
        }

        public int Add(T item)
        {
            var (chunk, index) = CalculateChunkIndex(Count);

            if (chunk >= _chunks.Length)
            {
                var size = _chunks.Length;
                Array.Resize(ref _chunks, size + 1);
                _chunks[size] = new Chunk(ChunkCapacity);
            }

            _chunks[chunk].Items[index] = item;

            return Count++;
        }

        public ref T GetAt(int itemIndex)
        {
            var (chunk, index) = CalculateChunkIndex(itemIndex);
#if DEBUG
            if (itemIndex >= Count || chunk >= _chunks.Length)
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
                if (!item.Equals(GetAt(i)))
                {
                    continue;
                }
                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        private struct Chunk : IEquatable<Chunk>
        {
            public T[] Items { get; }

            public Chunk(int capacity)
            {
                Items = new T[capacity];
            }

            public override bool Equals(object obj) => obj != null && Equals((Chunk) obj);

            public bool Equals(Chunk other)
            {
                return Items.Length == other.Items.Length;
            }

            public override int GetHashCode()
            {
                return Items.Length.GetHashCode();
            }
        }

        /*
        // private readonly Queue<int> _freeIndices;
        // _freeIndices = new Queue<int>();
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
    }
}
