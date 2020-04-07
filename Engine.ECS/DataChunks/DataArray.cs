
namespace CopperCowEngine.ECS.DataChunks
{
    internal struct DataArray<T> : IDataArray where T : struct, IComponentData
    {
        private readonly T[] _array;

        public DataArray(int capacity)
        {
            _array = new T[capacity];
        }

        public ref T this[int index] => ref _array[index];

        public object GetBoxedData(int index) => _array[index];

        public void SetBoxedData(int index, object data)
        {
            _array[index] = (T)data;
        }

        public void RemoveElement(int index, int lastIndex)
        {
            _array[index] = _array[lastIndex];
            _array[lastIndex] = default;
        }
    }
}
