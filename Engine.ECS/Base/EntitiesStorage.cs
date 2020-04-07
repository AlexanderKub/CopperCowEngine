using System.Collections.Generic;

namespace CopperCowEngine.ECS.Base
{
    internal sealed class EntitiesStorage
    {
        // TODO: calc memory usage
        private const int MaxEntitiesCount = 8192;

        private int _idGenerator;

        private readonly Queue<int> _freeIndices;

        private readonly List<EntitiesChunk> _entitiesChunks;

        public EntitiesStorage()
        {
            _entitiesChunks = new List<EntitiesChunk> { new EntitiesChunk() };
            _freeIndices = new Queue<int>();
        }

        private static (int index, int chunk) GetChunkPosition(int id)
        {
            return (id % MaxEntitiesCount, id / MaxEntitiesCount);
        }

        public Entity CreateEntity()
        {
            int id;
            if (_freeIndices.Count > 0) 
            {
                id = _freeIndices.Dequeue();
            } 
            else 
            {
                id = ++_idGenerator;
            }

            var (index, chunk) = GetChunkPosition(id);

            if (chunk >= _entitiesChunks.Count)
            {
                _entitiesChunks.Add(new EntitiesChunk());
            }

            _entitiesChunks[chunk].Entities[index] = Entity.Create(id);

            return _entitiesChunks[chunk].Entities[index];
        }

        public Entity GetEntity(int id)
        {
            if (id < 0 || id > _idGenerator)
            {
                return Entity.Null;
            }

            var (index, chunk) = GetChunkPosition(id);

            return _entitiesChunks[chunk].Entities[index];
        }

        public void RemoveEntity(Entity entity)
        {
            var (index, chunk) = GetChunkPosition(entity.Id);

            _entitiesChunks[chunk].Entities[index] = Entity.Null;

            _entitiesChunks[chunk].ArchetypeIndex[index] = -1;
            _entitiesChunks[chunk].IndicesInArchetypes[index] = -1;

            _freeIndices.Enqueue(entity.Id);
        }

        public void SetEntityArchetype(Entity entity, int archetypeIndex, int indexInArchetype)
        {
            var (index, chunk) = GetChunkPosition(entity.Id);

            _entitiesChunks[chunk].ArchetypeIndex[index] = archetypeIndex;
            _entitiesChunks[chunk].IndicesInArchetypes[index] = indexInArchetype;
        }

        public (int archetypeIndex, int index) GetEntityArchetypeWithIndex(Entity entity)
        {
            var (index, chunkIndex) = GetChunkPosition(entity.Id);

            var chunk = _entitiesChunks[chunkIndex];

            return (chunk.ArchetypeIndex[index], chunk.IndicesInArchetypes[index]);
        }

        public int GetEntityArchetypeIndex(Entity entity)
        {
            var (index, chunkIndex) = GetChunkPosition(entity.Id);

            var chunk = _entitiesChunks[chunkIndex];

            return chunk.ArchetypeIndex[index];
        }

        private class EntitiesChunk
        {
            // TODO: Possible not needed cause entity is just ID
            public readonly Entity[] Entities;

            public readonly int[] IndicesInArchetypes;

            public readonly int[] ArchetypeIndex;

            public EntitiesChunk()
            {
                IndicesInArchetypes = new int[MaxEntitiesCount];
                ArchetypeIndex = new int[MaxEntitiesCount];
                Entities = new Entity[MaxEntitiesCount];

                for (var i = 0; i < Entities.Length; i++)
                {
                    Entities[i] = Entity.Null;
                }
            }
        }
    }
}
