using EngineCore.ECS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// A class to store components for entities within a world.
    /// </summary>
    internal class EntityComponentsStorage
    {
        /// <summary>
        /// A class to describe the components
        /// </summary>
        private class EntityComponents {
            public Dictionary<Type, IEntityComponent> components;

            public EntityComponents()
            {
                components = new Dictionary<Type, IEntityComponent>();
            }

            public void Destroy()
            {
                components.Clear();
            }
        }
        
        /// <summary>
        /// List for store Entities components data.
        /// </summary>
        private List<EntityComponents> entitiesData;

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityComponentsStorage()
        {
            entitiesData = new List<EntityComponents>();
        }

        /// <summary>
        /// Extend data list.
        /// </summary>
        public void AddEntityData() {
            entitiesData.Add(new EntityComponents());
        }

        /// <summary>
        /// Determines if the given Entity has a IEntityComponent.
        /// </summary>
        /// <returns><c>true</c>, if the Entity has the IEntityComponent, <c>false</c> otherwise.</returns>
        /// <param name="entity">Given Entity.</param>
        /// <typeparam name="T">IEntityComponent to check.</typeparam>
        public bool HasComponent<T>(Entity entity) where T : IEntityComponent
        {
            return HasComponent(entity, typeof(T));
        }

        /// <summary>
        /// Determines if the given Entity has a IEntityComponent.
        /// </summary>
        /// <returns><c>true</c>, if the Entity has the IEntityComponent, <c>false</c> otherwise.</returns>
        /// <param name="entity">Given Entity.</param>
        /// <param name="componentType">IEntityComponent type to check.</param>
        public bool HasComponent(Entity entity, Type componentType)
        {
#if DEBUG
            if (!componentType.IsComponent())
            {
                throw new TypeNotComponentException();
            }
#endif
            return entitiesData[entity.ID].components.ContainsKey(componentType);
        }

        /// <summary>
        /// Retrieves an IEntityComponent instance for given Entity.
        /// </summary>
        /// <returns>IEntityComponent instance if found, null otherwise.</returns>
        /// <param name="entity">Given Entity.</param>
        /// <typeparam name="T">IEntityComponent to retrieve.</typeparam>
        public T GetComponent<T>(Entity entity) where T : IEntityComponent
        {
            return (T)GetComponent(entity, typeof(T));
        }

        /// <summary>
        /// Retrieves an all IEntityComponent instance for given Entity.
        /// </summary>
        /// <returns>IEntityComponent instances list.</returns>
        /// <param name="entity">Given Entity.</param>
        public List<IEntityComponent> GetComponents(Entity entity)
        {
            return entitiesData[entity.ID].components.Values.ToList();
        }

        /// <summary>
        /// Retrieves a component from the given Entity.
        /// </summary>
        /// <returns>IEntityComponent instance if found, null otherwise.</returns>
        /// <param name="entity">Given Entity.</param>
        /// <param name="componentType">IEntityComponent type to retrieve.</param>
        public object GetComponent(Entity entity, Type componentType)
        {
#if DEBUG
            if (!componentType.IsComponent())
            {
                throw new TypeNotComponentException();
            }
#endif

            entitiesData[entity.ID].components.TryGetValue(componentType, out IEntityComponent foundComp);
            return foundComp;
        }

        /// <summary>
        /// Add a component to given Entity.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <param name="component">Component to add.</param>
        public void AddComponent(Entity entity, IEntityComponent component)
        {
            Type componentType = component.GetType();
#if DEBUG
            if (HasComponent(entity, componentType))
            {
                throw new ComponentAlreadyExistsException();
            }
#endif
            if (componentType.IsComponentWithEntityId())
            {
                ((IEntityComponentWithEntityId)component).EntityId = entity.ID;
            }
            entitiesData[entity.ID].components.Add(componentType, component);
        }

        /// <summary>
        /// Removes a component from the given Entity.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <typeparam name="T">IEntityComponent to remove.</typeparam>
        public void RemoveComponent<T>(Entity entity) where T : IEntityComponent
        {
            RemoveComponent(entity, typeof(T));
        }

        /// <summary>
        /// Removes a component from the given Entity.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <param name="componentType">IEntityComponent type to remove.</param>
        public void RemoveComponent(Entity entity, Type componentType)
        {
#if DEBUG
            if (!componentType.IsComponent())
            {
                throw new TypeNotComponentException();
            }

            if (!HasComponent(entity, componentType))
            {
                throw new ComponentNotFoundException();
            }
#endif

            entitiesData[entity.ID].components.Remove(componentType);
        }
        
        /// <summary>
        /// Removes all components from the given Entity.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        public void RemoveAllComponents(Entity entity)
        {
            entitiesData[entity.ID].components.Clear();
        }

        /// <summary>
        /// Cleanup method.
        /// </summary>
        internal void Destroy()
        {
            entitiesData.Clear();
        }
    }
}
