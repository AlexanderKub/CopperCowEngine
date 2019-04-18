using EngineCore.ECS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    public class Entity
    {
        /// <summary>
        /// Reference to World instance.
        /// </summary>
        private readonly World WorldRef;

        /// <summary>
        /// Unique Entity ID.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// Entity name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Is Entity active flag.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Is Entity valid flag.
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world">Reference to World instance.</param>
        /// <param name="id">Unique Entity ID.</param>
        public Entity(World world, int id) {
            WorldRef = world;
            ID = id;
        }

        /// <summary>
        /// Set entity active state.
        /// </summary>
        public void SetActive(bool active)
        {
            WorldRef.SetEntityActivate(this, active);
        }
        
        /// <summary>
        /// Init Entity after pooling.
        /// </summary>
        internal void Reinit()
        {
            IsValid = true;
        }

        /// <summary>
        /// Clear entity data for next entity pooling.
        /// </summary>
        internal void Destroy()
        {
            WorldRef.DestroyEntity(this);
        }

        #region Components
        /// <summary>
        /// Determines if the Entity has a IEntityComponent.
        /// </summary>
        /// <returns><c>true</c>, if the Entity has the IEntityComponent, <c>false</c> otherwise.</returns>
        /// <typeparam name="T">IEntityComponent to check.</typeparam>
        public bool HasComponent<T>() where T : IEntityComponent
        {
            return WorldRef.ComponentsStorage.HasComponent<T>(this);
        }

        /// <summary>
        /// Determines if the Entity has a IEntityComponent.
        /// </summary>
        /// <param name="componentType">IEntityComponent Type to check.</param>
        /// <returns><c>true</c>, if the Entity has the IEntityComponent, <c>false</c> otherwise.</returns>
        public bool HasComponent(Type componentType)
        {
            return WorldRef.ComponentsStorage.HasComponent(this, componentType);
        }

        /// <summary>
        /// Retrieves an IEntityComponent instance for this Entity.
        /// </summary>
        /// <returns>IEntityComponent instance if found, null otherwise.</returns>
        /// <typeparam name="T">IEntityComponent to retrieve.</typeparam>
        public T GetComponent<T>() where T : IEntityComponent
        {
            return WorldRef.ComponentsStorage.GetComponent<T>(this);
        }

        /// <summary>
        /// Add a component to this Entity.
        /// </summary>
        /// <typeparam name="T">Adding Component Type.</typeparam>
        /// <returns>Added component.</returns>
        public T AddComponent<T>() where T : IEntityComponent, new()
        {
            T component = new T();
            WorldRef.ComponentsStorage.AddComponent(this, component);
            return component;
        }

        /// <summary>
        /// Removes a component from the Entity.
        /// </summary>
        /// <typeparam name="T">IEntityComponent to remove.</typeparam>
        public void RemoveComponent<T>() where T : IEntityComponent
        {
            WorldRef.ComponentsStorage.RemoveComponent<T>(this);
        }
        #endregion
    }
}
