using CopperCowEngine.ECS.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class DataChunkArchetypesStorage : ChunkedArray<DataChunkArchetype>
    {
        public DataChunkArchetypesStorage() : base(256)
        {
            
        }
    }
}
