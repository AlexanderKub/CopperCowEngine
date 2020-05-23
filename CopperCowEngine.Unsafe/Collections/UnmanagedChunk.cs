using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Unsafe.Collections
{
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    [DebuggerDisplay("Capacity = {" + nameof(Capacity) + "}")]
    [DebuggerTypeProxy(typeof(UnmanagedChunkDebugView))]
    public struct UnmanagedChunk : IDisposable, IEquatable<UnmanagedChunk>
    {
        private const int ChunkMemorySize = 16 * 1024;

        private const int IntSize = sizeof(int);

        private IntPtr _buffer;

        // TODO: move to chunk chain
        private readonly UnmanagedChunkLayoutElement[] _layout;

        public int Capacity { get; private set; }

        public int Count { get; private set;  }

        public bool Full => Count == Capacity;

        public UnmanagedChunk(UnmanagedChunkLayoutElement[] layout)
        {
            _layout = layout;

            var rowSize = IntSize; // EntityId

            for (var i = 0; i < _layout.Length; i++)
            {
                rowSize += _layout[i].ItemSize;
            }

            _buffer = UnsafeUtility.MemAlloc(ChunkMemorySize, 1);

            Capacity = ChunkMemorySize / rowSize;

            var offset = Capacity * IntSize;

            for (var i = 0; i < _layout.Length; i++)
            {
                var l = _layout[i];
                l.StartOffset = offset;
                _layout[i] = l;

                offset += Capacity * l.ItemSize;
            }

            Count = 0;
        }

        private void CopyData(int fromIndex, int toIndex)
        {
            for (var i = 0; i < _layout.Length; i++)
            {
                var startOffset = _layout[i].StartOffset;
                var itemSize = _layout[i].ItemSize;

                var fromPtr =  _buffer + startOffset + itemSize * fromIndex;
                var toPtr = _buffer + startOffset + itemSize * toIndex;

                UnsafeUtility.MemCopy(fromPtr ,toPtr, itemSize);
            }
        }
        
        private UnmanagedChunkLayoutElement GetLayoutElement(Type type)
        {
            var hashCode = type.GetHashCode();
            for (var i = 0; i < _layout.Length; i++)
            {
                if (_layout[i].TypeHashCode == hashCode)
                {
                    return _layout[i];
                }
            }

            return default;
        }

        private void SetEntityId(int index, int id)
        {
            UnsafeUtility.WriteElement(_buffer,  index, id);
        }

        public int Add(int id)
        {
            SetEntityId(Count, id);
            return Count++;
        }

        public void CopyDataToAnotherChunk(UnmanagedChunk chunk, int fromIndex, int toIndex)
        {
            for (var i = 0; i < _layout.Length; i++)
            {
                var itemSize = _layout[i].ItemSize;
                var fromPtr =  _buffer + _layout[i].StartOffset + itemSize * fromIndex;

                for (var j = 0; j < chunk._layout.Length; j++)
                {
                    if (_layout[i].TypeId != chunk._layout[j].TypeId)
                    {
                        continue;
                    }
                    var toPtr = chunk._buffer + chunk._layout[j].StartOffset + itemSize * toIndex;
                    UnsafeUtility.MemCopy(fromPtr ,toPtr, itemSize);
                    break;
                }
            }
        }

        public int GetEntityIdByIndex(int index)
        {
            return UnsafeUtility.ReadElement<int>(_buffer, index);
        }

        public ref T GetDataByIndex<T>(int index) where T : unmanaged
        {
            var layout = GetLayoutElement(typeof(T));
            // TODO: checks
            //return ref UnsafeUtility.ElementDirect<T>(_buffer, layout.StartOffset + layout.ItemSize * index);
            return ref UnsafeUtility.ElementDirect<T>(_buffer, layout.StartOffset + layout.ItemSize * index);
        }
        
        public int RemoveByIndex(int index)
        {
            if (index == Count)
            {
                return -1;
            }
            Count--;

            var movedId = GetEntityIdByIndex(Count);
            SetEntityId(index, movedId);
            SetEntityId(Count, -1);

            CopyData(Count, index);

            // Return moved entity ID for setting new index in archetype
            return movedId;
        }

        public void SetDataByIndex<T>(int index, T data) where T : unmanaged 
        {
            var layout = GetLayoutElement(typeof(T));
            // TODO: checks
            //UnsafeUtility.WriteElementDirect(_buffer, layout.StartOffset + layout.ItemSize * index, data);
            UnsafeUtility.WriteElementDirectP(_buffer, layout.StartOffset + layout.ItemSize * index, data);
        }
        
        public void SetDataFromContainer(int toIndex, UnmanagedContainer container)
        {
            var itemSize = container.ItemSize;
            var fromPtr = container.Buffer;

            for (var j = 0; j < _layout.Length; j++)
            {
                if (container.TypeId != _layout[j].TypeId)
                {
                    continue;
                }
                var toPtr = _buffer + _layout[j].StartOffset + itemSize * toIndex;
                UnsafeUtility.MemCopy(fromPtr ,toPtr, itemSize);
                break;
            }
            container.Dispose();
        }

        public void Dispose()
        {
            if (_buffer == IntPtr.Zero)
            {
                return;
            }
            UnsafeUtility.MemFree(_buffer);
            _buffer = IntPtr.Zero;
            Capacity = 0;
            Count = 0;
        }

        public bool Equals(UnmanagedChunk other)
        {
            return false;
        }

        public int[] ToArray()
        {
            return UnsafeUtility.GetArray<int>(_buffer, Capacity);
        }
    }

    internal sealed class UnmanagedChunkDebugView
    {
        private UnmanagedChunk _chunk;

        public UnmanagedChunkDebugView(UnmanagedChunk chunk)
        {
            _chunk = chunk;
        }

        public int[] Items => _chunk.ToArray();
    }
}
