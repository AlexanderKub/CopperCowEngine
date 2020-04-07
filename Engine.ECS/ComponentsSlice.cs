﻿using CopperCowEngine.ECS.DataChunks;

namespace CopperCowEngine.ECS
{
    public sealed class ComponentsSlice
    {
        private readonly EcsContext _context;

        private readonly int _archetypePosition;

        private readonly int _chunkPosition;

        private readonly int _entityPosition;

        internal ComponentsSlice(EcsContext context, int archetypePosition, int chunkPosition, int entityPosition)
        {
            _context = context;
            _archetypePosition = archetypePosition;
            _chunkPosition = chunkPosition;
            _entityPosition = entityPosition;
        }

        internal ref DataChunk Chunk => ref _context.DataChunkStorage
            .ChunksStorage[_archetypePosition].Chunks[_chunkPosition];

        public Entity Entity => _context.EntitiesStorage.GetEntity(_context.DataChunkStorage
            .ChunksStorage[_archetypePosition].Chunks[_chunkPosition].GetEntityIdByIndex(_entityPosition));

        public bool HasSibling<T>() where T : struct, IComponentData
        {
            ref var archetype = ref _context.DataChunkStorage.ArchetypesStorage.GetAt(_archetypePosition);

            return DataChunkArchetype.Compatibility(in archetype, typeof(T));
        }

        public ref T Sibling<T>() where T : struct, IComponentData
        {
            return ref Chunk.GetDataByIndex<T>(_entityPosition);
        }
    }
}
