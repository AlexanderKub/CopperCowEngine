using System;
using System.Collections.Generic;
using CopperCowEngine.ECS.Base;
using CopperCowEngine.ECS.DataChunks;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS
{
    public sealed class CommandContext
    {
        private readonly EcsContext _context;

        private readonly Queue<Entity> _entitiesToRemove;

        private readonly Queue<EntityToArchetype> _entitiesToArchetype;
        
        private readonly Queue<ComponentToEntity> _componentsToAdd;

        private readonly Queue<ComponentToEntity> _componentsToRemove;

        private readonly Queue<ComponentDataToEntity> _componentsToSet;

        public CommandContext(EcsContext context)
        {
            _context = context;
            _entitiesToRemove = new Queue<Entity>();
            _entitiesToArchetype = new Queue<EntityToArchetype>();
            _componentsToAdd = new Queue<ComponentToEntity>();
            _componentsToRemove = new Queue<ComponentToEntity>();
            _componentsToSet = new Queue<ComponentDataToEntity>();
        }

        internal void Execute()
        {
            while (_entitiesToRemove.TryDequeue(out var entity))
            {
                _context.DestroyEntity(entity);
            }

            while (_entitiesToArchetype.TryDequeue(out var entityToArchetype))
            { 
                var indexInArchetype = _context.DataChunkStorage.AddEntity(
                    entityToArchetype.Entity, entityToArchetype.ArchetypeIndex);
                _context.EntitiesStorage.SetEntityArchetype(
                    entityToArchetype.Entity, entityToArchetype.ArchetypeIndex, indexInArchetype);
            }

            while (_componentsToAdd.TryDequeue(out var toAdd))
            {
                _context.AddComponent(toAdd.Entity, toAdd.TypeId);
            }

            while (_componentsToRemove.TryDequeue(out var toRemove))
            {
                _context.RemoveComponent(toRemove.Entity, toRemove.TypeId);
            }

            while (_componentsToSet.TryDequeue(out var toSet))
            {
               _context.SetComponentData(toSet.Entity, toSet.Data);
            }
        }

        public Entity CreateEntity()
        {
            return _context.EntitiesStorage.CreateEntity();
        }

        public Entity CreateEntity(params Type[] types)
        {
            var newArchetype = _context.DataChunkStorage.GetArchetype(types);

            var archetypeIndex = _context.DataChunkStorage.TryRegisterArchetype(in newArchetype);

            var entity = _context.EntitiesStorage.CreateEntity();
            
            _entitiesToArchetype.Enqueue(new EntityToArchetype
            {
                Entity = entity,
                ArchetypeIndex = archetypeIndex,
            });

            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            _entitiesToRemove.Enqueue(entity);
        }
        
        public void AddComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var typeId = _context.DataChunkStorage.TypesStorage.TryRegisterType(typeof(T));

            _componentsToAdd.Enqueue(new ComponentToEntity
            {
                Entity = entity,
                TypeId = typeId,
            });
        }

        public void AddComponent<T>(Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            var typeId = _context.DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            _componentsToAdd.Enqueue(new ComponentToEntity
            {
                Entity = entity,
                TypeId = typeId,
            });

            _componentsToSet.Enqueue(new ComponentDataToEntity
            {
                Entity = entity,
                Data = new UnmanagedContainer(typeId, componentSize).SetData(componentData)
            });
        }

        public ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return ref _context.GetComponent<T>(entity);
        }

        public void RemoveComponent<T>(Entity entity) where T : struct, IComponentData
        {
            var typeId = _context.DataChunkStorage.TypesStorage.TryRegisterType(typeof(T));

            _componentsToRemove.Enqueue(new ComponentToEntity
            {
                Entity = entity,
                TypeId = typeId,
            });
        }

        public void SetComponent<T>(Entity entity, T componentData) where T : unmanaged, IComponentData
        {
            var typeId = _context.DataChunkStorage.TypesStorage.TryRegisterType(typeof(T), out var componentSize);

            _componentsToSet.Enqueue(new ComponentDataToEntity
            {
                Entity = entity,
                Data = new UnmanagedContainer(typeId, componentSize).SetData(componentData)
            });
        }

        private struct EntityToArchetype
        {
            public Entity Entity;

            public int ArchetypeIndex;
        }

        private struct ComponentToEntity
        {
            public Entity Entity;

            public int TypeId;
        }

        private struct ComponentDataToEntity
        {
            public Entity Entity;

            public UnmanagedContainer Data;
        }
    }
}
