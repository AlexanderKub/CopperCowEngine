using CopperCowEngine.ECS.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class ArchetypesStorage : ChunkedArray<DataChunkArchetype>
    {
        public ArchetypesStorage() : base(256)
        {
            
        }

        public override bool Equals(in DataChunkArchetype item, ref DataChunkArchetype other)
        {
            return item.Equals(other);
        }
    }
}
