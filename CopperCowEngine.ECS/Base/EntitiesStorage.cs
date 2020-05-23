using System.Collections.Generic;
using System.Runtime.InteropServices;
using CopperCowEngine.Unsafe;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.Base
{
    internal sealed class EntitiesStorage
    {
        // TODO: calc memory usage
        private const int MaxEntitiesCountPerChunk = 1024;

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
            return (id % MaxEntitiesCountPerChunk, id / MaxEntitiesCountPerChunk);
        }

        public Entity CreateEntity()
        {
            if (!_freeIndices.TryDequeue(out var id))
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

        public void DestroyEntity(Entity entity)
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
                IndicesInArchetypes = new int[MaxEntitiesCountPerChunk];
                ArchetypeIndex = new int[MaxEntitiesCountPerChunk];
                Entities = new Entity[MaxEntitiesCountPerChunk];

                for (var i = 0; i < Entities.Length; i++)
                {
                    Entities[i] = Entity.Null;
                }
            }
        }
    }
}
