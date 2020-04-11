using System.Runtime.InteropServices;

namespace CopperCowEngine.Unsafe.Collections
{
    public struct UnmanagedChunkMapping
    {
        public int TypeId;
        public int ElementSize;
        public int StartOffset;
    }

    public unsafe struct UnmanagedChunk
    {
        private const int ChunkMemorySize = 16 * 1024;

        private const int HeaderMemorySize = sizeof(int) * 2;

        public int Capacity { get; }

        public int Count { get; }

        private UnmanagedArray<UnmanagedChunkMapping> _mapping;

        private void* Buffer;

        public UnmanagedChunk(UnmanagedChunkMapping[] mapping)
        {
            _mapping = new UnmanagedArray<UnmanagedChunkMapping>(mapping);

            var rowSize = sizeof(int);

            for (var i = 0; i < mapping.Length; i++)
            {
                rowSize += mapping[i].ElementSize;
            }

            var mappingSize = mapping.Length * Marshal.SizeOf<UnmanagedChunkMapping>();

            Capacity = (ChunkMemorySize - HeaderMemorySize - mappingSize) / rowSize;

            Buffer = UnsafeUtility.MemAlloc(ChunkMemorySize, 1);

            Count = 0;
        }

        public T GetData<T>(int index) where T : struct 
        {
            return UnsafeUtility.ReadElement<T>(Buffer, index);
        }

        public void SetData<T>(int index, T data) where T : struct 
        {
            UnsafeUtility.WriteElement(Buffer, index, data);
        }
    }
}
