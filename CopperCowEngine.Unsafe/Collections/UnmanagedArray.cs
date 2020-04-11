using System;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Unsafe.Collections
{
    public unsafe struct UnmanagedArray<T> : IDisposable, IEquatable<UnmanagedArray<T>> where T : struct
    {
        public int Length { get; }

        public int ElementSize { get; }

        internal void* Buffer;
        
        private UnmanagedArray(int length, int elementSize)
        {
            Length = length;
            ElementSize = elementSize;
            Buffer = UnsafeUtility.MemAlloc(length, elementSize);
            //new Span<T>(Buffer, Length).Fill(default(T));
            //new Span<T>(Buffer, Length).Clear();
        }

        public UnmanagedArray(T[] array)
        {
            Length = array.Length;
            ElementSize = Marshal.SizeOf<T>();
            Buffer = UnsafeUtility.MemAlloc(Length, ElementSize);

            var source = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
            UnsafeUtility.MemCopy(source.ToPointer(), Buffer, Length * ElementSize);
        }

        // TODO: Range checks and thread safe
        public T this[int index]
        {
            get => UnsafeUtility.ReadElement<T>(Buffer, index);
            set => UnsafeUtility.WriteElement(Buffer, index, value);
        }
        
        public void Dispose()
        {
            if (Buffer == null)
            {
                return;
            }
            UnsafeUtility.MemFree(Buffer);
            Buffer = null;
        }

        public bool Equals(UnmanagedArray<T> other)
        {
            return false;
        }

        public T[] ToArray()
        {
            return new Span<T>(Buffer, Length).ToArray();
        }
        
        public static void Allocate(int totalSize, out UnmanagedArray<T> array)
        {
            var elementSize = Marshal.SizeOf<T>();
            var length = totalSize / elementSize;
            array = new UnmanagedArray<T>(length, elementSize);
        }

        public static void Allocate(int length, int elementSize, out UnmanagedArray<T> array)
        {
            array = new UnmanagedArray<T>(length, elementSize);
        }
    }
}
