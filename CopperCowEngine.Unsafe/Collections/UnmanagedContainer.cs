using System;
using System.Collections.Generic;
using System.Text;

namespace CopperCowEngine.Unsafe.Collections
{
    public struct UnmanagedContainer : IDisposable
    {
        public IntPtr Buffer;

        public int TypeId { get; private set; }

        public int ItemSize { get; private set; }

        public UnmanagedContainer(int typeId, int itemSize)
        {
            TypeId = typeId;
            ItemSize = itemSize;
            Buffer = UnsafeUtility.MemAlloc(ItemSize, 1);
        }

        public UnmanagedContainer SetData<T>(T data) where T : unmanaged 
        {
            // TODO: checks
            UnsafeUtility.WriteElementDirectP(Buffer, 0, data);
            return this;
        }

        public void Dispose()
        {
            if (Buffer == IntPtr.Zero)
            {
                return;
            }

            TypeId = -1;
            ItemSize = 0;
            UnsafeUtility.MemFree(Buffer);
        }

        public int[] ToArray()
        {
            return UnsafeUtility.GetArray<int>(Buffer, 1);
        }
    }
}
