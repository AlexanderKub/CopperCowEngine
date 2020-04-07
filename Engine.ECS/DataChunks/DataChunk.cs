using System;

namespace CopperCowEngine.ECS.DataChunks
{
    internal struct DataChunk : IEquatable<DataChunk>
    {
        public const int ChunkSize = 16 * 1024;

        private readonly DataChunkArchetype _archetype;

        private readonly IDataArray[] _dataArrays;

        private readonly int[] _ids;

        public int Count { get; private set; }

        public int Capacity { get; }

        public bool Full => Count == Capacity;

        public DataChunk(in DataChunkArchetype archetype)
        {
            _archetype = archetype;
            var n = archetype.Types.Length;

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
                var genericType = typeof(DataArray<>).MakeGenericType(archetype.Types[i]);
                _dataArrays[i] = (IDataArray)Activator.CreateInstance(genericType, Capacity);
            }
        }

        public int GetEntityIdByIndex(int index)
        {
            return _ids[index];
        }

        public ref T GetDataByIndex<T>(int index) where T : struct, IComponentData
        {
            var dataArrayIndex = _archetype.Types.IndexOf(typeof(T));
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            return ref ((DataArray<T>)_dataArrays[dataArrayIndex])[index];
        }

        public object GetBoxedDataByIndex(int index, Type type)
        {
            var dataArrayIndex = _archetype.Types.IndexOf(type);
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            return _dataArrays[dataArrayIndex].GetBoxedData(index);
        }

        public void SetBoxedDataByIndex(int index, Type type, object data)
        {
            var dataArrayIndex = _archetype.Types.IndexOf(type);
#if DEBUG
            if (dataArrayIndex < 0)
            {
                throw new NullReferenceException();
            }
#endif
            _dataArrays[dataArrayIndex].SetBoxedData(index, data);
        }

        public void SetDataByIndex<T>(int index, T data) where T : struct, IComponentData
        {
            var dataArrayIndex = _archetype.Types.IndexOf(typeof(T));
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

            for (var i = 0; i < _archetype.Types.Length; i++)
            {
                _dataArrays[i].RemoveElement(index, Count);
            }

            // Return moved entity ID for setting new index in archetype
            return _ids[index];
        }

        public bool Equals(DataChunk other)
        {
            return _archetype.Equals(other._archetype);
        }
    }
}
