using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Unsafe.Collections
{
    [DebuggerDisplay("Length = {" + nameof(Length) + "}")]
    [DebuggerTypeProxy(typeof(UnmanagedListDebugView<>))]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public struct UnmanagedList<T> : IDisposable, IEquatable<UnmanagedList<T>> where T : unmanaged
    {
        internal IntPtr Buffer;

        public int ElementSize { get; }

        public int Length { get; private set; }

        public int MaxCapacity { get; }

        public UnmanagedList(int length, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            Length = length;
            ElementSize = Marshal.SizeOf<T>();
            Buffer = UnsafeUtility.MemAlloc(Length, ElementSize);
        }

        public UnmanagedList(T[] array, int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            Length = array.Length;
            ElementSize = Marshal.SizeOf<T>();
            Buffer = UnsafeUtility.MemAlloc(Length, ElementSize);

            var source = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
            UnsafeUtility.MemCopy(source, Buffer, Length * ElementSize);
        }

        public T this[int index]
        {
            get
            {
                if (index >= Length)
                {
                    Expand();
                }
                return UnsafeUtility.ReadElement<T>(Buffer, index);
            }
            set
            {
                if (index >= Length)
                {
                    Expand();
                }
                UnsafeUtility.WriteElement(Buffer, index, value);
            }
        }

        public void Dispose()
        {
            if (Buffer == IntPtr.Zero)
            {
                return;
            }
            UnsafeUtility.MemFree(Buffer);
            Buffer = IntPtr.Zero;
            Length = 0;
        }

        public bool Equals(UnmanagedList<T> other)
        {
            return false;
        }

        public T[] ToArray()
        {
            return UnsafeUtility.GetArray<T>(Buffer, Length);
        }

        private void Expand()
        {
            var newLength = Length * 2;

            if (newLength > MaxCapacity)
            {
                newLength = MaxCapacity;

                if (newLength - Length == 0)
                {
                    throw new OutOfMemoryException("UnmanagedList maximum capacity achieved");
                }
            }

            var newBuffer = UnsafeUtility.MemAlloc(newLength, ElementSize);
            UnsafeUtility.MemCopy(Buffer, newBuffer, Length * ElementSize);
            UnsafeUtility.MemFree(Buffer);

            Length = newLength;
            Buffer = newBuffer;
        }
    }

    internal sealed class UnmanagedListDebugView<T> where T : unmanaged
    {
        private UnmanagedList<T> _list;

        public UnmanagedListDebugView(UnmanagedList<T> list)
        {
            _list = list;
        }

        public T[] Items => _list.ToArray();
    }
}
