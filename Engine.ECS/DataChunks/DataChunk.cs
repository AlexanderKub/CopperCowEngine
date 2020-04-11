using System;
using System.Collections.Generic;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal struct DataChunk
    {
        public const int ChunkSize = 16 * 1024;

        private readonly IDataArray[] _dataArrays;

        private readonly int[] _ids;

        public int Count { get; private set; }

        public int Capacity { get; }

        public bool Full => Count == Capacity;

        public DataChunk(in DataChunkArchetype archetype, IReadOnlyList<ComponentType> componentTypes)
        {
            var n = archetype.ComponentTypes.Length;

            Count = 0;
            Capacity = archetype.ChunkCapacity;

            _ids = new int[Capacity];
            for (var i = 0; i < _ids.Length; i++)
            {
                _ids[i] = -1;
            }

            _dataArrays = new IDataArray[n];
            for (var i = 0; i < n; i++)
            {
                var genericType = typeof(DataArray<>).MakeGenericType(componentTypes[i].BackedType);
                _dataArrays[i] = (IDataArray)Activator.CreateInstance(genericType, Capacity);
            }
        }

        public int GetEntityIdByIndex(int index)
        {
            return _ids[index];
        }

        public ref T GetDataByIndex<T>(int index, int dataArrayIndex) where T : struct, IComponentData
        {
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            return ref ((DataArray<T>)_dataArrays[dataArrayIndex])[index];
        }

        public object GetBoxedDataByIndex(int index, int dataArrayIndex)
        {
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            return _dataArrays[dataArrayIndex].GetBoxedData(index);
        }

        public void SetBoxedDataByIndex(int index, int dataArrayIndex, object data)
        {
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            _dataArrays[dataArrayIndex].SetBoxedData(index, data);
        }

        public void SetDataByIndex<T>(int index, int dataArrayIndex, T data) where T : struct, IComponentData
        {
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            ((DataArray<T>)_dataArrays[dataArrayIndex])[index] = data;
        }

        public int Add(int id)
        {
            _ids[Count] = id;
            return Count++;
        }

        public int RemoveByIndex(int index)
        {
            Count--;

            if (index == Count)
            {
                return -1;
            }

            _ids[index] = _ids[Count];
            _ids[Count] = -1;

            foreach (var dataArray in _dataArrays)
            {
                dataArray.RemoveElement(index, Count);
            }

            // Return moved entity ID for setting new index in archetype
            return _ids[index];
        }
    }
}
