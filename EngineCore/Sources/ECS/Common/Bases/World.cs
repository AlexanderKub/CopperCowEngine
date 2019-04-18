using EngineCore.ECS.Components;
using EngineCore.ECS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// A manager for Entities.
    /// </summary>
    public class World
    {
        /// <summary>
        /// Cache of entities reference.
        /// </summary>
        private EntityCache m_EntitiesCache;

        /// <summary>
        /// Entity pool reference.
        /// </summary>
        private EntityPool m_EntityPool;

        /// <summary>
        /// Components storage reference.
        /// </summary>
        internal readonly EntityComponentsStorage ComponentsStorage;

        /// <summary>
        /// Systems storage reference.
        /// </summary>
        internal readonly EntitySystemsStorage SystemsStorage;

        /// <summary>
        /// System filters reference.
        /// </summary>
        internal readonly SystemComponentsFilters SystemFiltersStorage;

        /// <summary>
        /// Constructor
        /// </summary>
        public World()
        {
            m_EntityPool = new EntityPool(this);
            m_EntitiesCache = new EntityCache();

            SystemsStorage = new EntitySystemsStorage(this);
            ComponentsStorage = new EntityComponentsStorage();
            SystemFiltersStorage = new SystemComponentsFilters(m_EntitiesCache);
        }

        #region Systems block
        /// <summary>
        /// Create and add system of given type.
        /// </summary>
        /// <typeparam name="T">Given system type.</typeparam>
        /// <returns>Created system.</returns>
        public T AddSystem<T>() where T : BaseSystem, new()
        {
            T newSystem = new T();
            AddSystem(newSystem);
            return newSystem;
        }

        /// <summary>
        /// Add given System to World.
        /// </summary>
        /// <param name="system">Given System</param>
        public void AddSystem(BaseSystem system)
        {
            SystemsStorage.AddSystem(system);
        }

        /// <summary>
        /// Remove given System from World.
        /// </summary>
        /// <param name="system">Given System.</param>
        internal void RemoveSystem(BaseSystem system)
        {
            SystemsStorage.RemoveSystem(system.GetType());
        }

        //TODO: single component for world (not real singleton)
        private Dictionary<Type, ISingletonEntityComponent> m_SingletonComponents;
        public T GetSingletonComponent<T>() where T : ISingletonEntityComponent, new()
        {
            if (m_SingletonComponents == null)
            {
                m_SingletonComponents = new Dictionary<Type, ISingletonEntityComponent>();
            }
            if (!m_SingletonComponents.ContainsKey(typeof(T)))
            {
                m_SingletonComponents.Add(typeof(T), new T());
            }
            return (T)m_SingletonComponents[typeof(T)];
        }
        #endregion

        #region Entities block

        /// <summary>
        /// Create new Entity instance.
        /// </summary>
        /// <returns>Created Entity instance</returns>
        public Entity CreateEntity()
        {
            return CreateEntity(string.Empty);
        }

        public Entity CreateEntity(string name)
        {
            var entity = m_EntityPool.GetNewEntity();
            entity.Name = string.IsNullOrEmpty(name) ? $"NewEntity_{entity.ID}" : name;
            m_EntitiesCache.Created.Add(entity);
            return entity;
        }

        public Entity CreateEntityWith<T>(string name) where T : IEntityComponent, new()
        {
            Entity entity = CreateEntity(name);
            entity.AddComponent<T>();
            return entity;
        }

        public Entity CreateEntityWith<T, Y>(string name) where T : IEntityComponent, new() where Y : IEntityComponent, new()
        {
            Entity entity = CreateEntity(name);
            entity.AddComponent<T>();
            entity.AddComponent<Y>();
            return entity;
        }

        /// <summary>
        /// Change given Entity active state.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <param name="active">Active state.</param>
        internal void SetEntityActivate(Entity entity, bool active)
        {
            if (!entity.IsValid) {
                //TODO: throw exception?
                return;
            }

            if (active) {
                if (entity.IsActive) {
                    //TODO: throw exception?
                    return;
                }
                m_EntitiesCache.Activated.Add(entity);
            } else {
                if (!entity.IsActive) {
                    //TODO: throw exception?
                    return;
                }
                m_EntitiesCache.Deactivated.Add(entity);
            }
        }

        /// <summary>
        /// Destroy given Entity and return to pool/
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        internal void DestroyEntity(Entity entity)
        {
            if (!entity.IsValid) {
                //TODO: throw exception?
                return;
            }
            SetEntityActivate(entity, false);
            ComponentsStorage.RemoveAllComponents(entity);
            m_EntitiesCache.Destroyed.Add(entity);
        }

        /// <summary>
        /// Get created Entity instance by given ID.
        /// </summary>
        /// <returns>Founded valid Entity, null otherwise.</returns>
        /// <param name="id">Given ID.</param>
        public Entity GetEntity(int id)
        {
            /*return m_EntitiesCache.Created.Find((Entity e) => {
               return e.ID == id;
            });*/
            return m_EntitiesCache.GetEntity(id);
        }

        public List<Entity> GetEntities()
        {
            return m_EntitiesCache.Created;
        }
        #endregion

        /// <summary>
        /// Refresh world state.
        /// </summary>
        public void Refresh()
        {
            List<BaseSystem> SystemsList = SystemsStorage.GetSystems();

            foreach (Entity entity in m_EntitiesCache.Activated)
            {
                entity.IsActive = true;
                SystemFiltersStorage.FilterActivatedEntities(entity);
            }

            foreach (Entity entity in m_EntitiesCache.Deactivated)
            {
                entity.IsActive = false;
                SystemFiltersStorage.RemoveEntityFromAllFiltered(entity);
            }

            foreach (Entity entity in m_EntitiesCache.Destroyed)
            {
                m_EntitiesCache.Created.Remove(entity);
                ComponentsStorage.RemoveAllComponents(entity);
                m_EntityPool.RemoveEntity(entity);
            }

            // clear the temp cache
            m_EntitiesCache.ClearTemp();
        }

        /// <summary>
        /// Destroy this world
        /// </summary>
        public void Destroy()
        {
            foreach (var entity in m_EntitiesCache.Created) {
                DestroyEntity(entity);
            } 
            m_EntitiesCache.Clear();
            m_EntityPool.Destroy();

            SystemsStorage.Destroy();
            SystemFiltersStorage.Destroy();
            ComponentsStorage.Destroy();
            m_SingletonComponents.Clear();
        }
    }
}
