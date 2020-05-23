using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Unsafe.Collections
{
    [DebuggerDisplay("Length = {" + nameof(Length) + "}")]
    [DebuggerTypeProxy(typeof(UnmanagedArrayDebugView<>))]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct UnmanagedArray<T> : IDisposable, IEquatable<UnmanagedArray<T>> where T : unmanaged
    {
        private IntPtr _buffer;
        
        public int ElementSize { get; }

        public int Length { get; private set; }

        public T this[int index]
        {
            get => UnsafeUtility.ReadElement<T>(_buffer, index);
            set => UnsafeUtility.WriteElement(_buffer, index, value);
        }

        public UnmanagedArray(int length)
        {
            Length = length;
            ElementSize = Marshal.SizeOf<T>();
            _buffer = UnsafeUtility.MemAlloc(length, ElementSize);
        }

        public bool Equals(UnmanagedArray<T> other)
        {
            return false;
        }

        public void Dispose()
        {
            UnsafeUtility.MemFree(_buffer);
            Length = 0;
            _buffer = IntPtr.Zero;
        }

        public T[] ToArray()
        {
            return UnsafeUtility.GetArray<T>(_buffer, Length);
        }
    }

    internal sealed class UnmanagedArrayDebugView<T> where T : unmanaged
    {
        private UnmanagedArray<T> _array;

        public UnmanagedArrayDebugView(UnmanagedArray<T> array)
        {
            _array = array;
        }

        public T[] Items => _array.ToArray();
    }
}
