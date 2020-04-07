namespace CopperCowEngine.ECS.DataChunks
{
    internal interface IDataArray
    {
        void RemoveElement(int index, int lastIndex);

        object GetBoxedData(int index);

        void SetBoxedData(int index, object data);
    }
}
