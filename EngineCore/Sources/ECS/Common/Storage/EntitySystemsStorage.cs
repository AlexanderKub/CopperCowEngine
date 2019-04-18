using EngineCore.ECS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    internal class EntitySystemsStorage
    {
        /// <summary>
        /// Systems dictionary.
        /// </summary>
        private Dictionary<Type, BaseSystem> m_SystemsList;

        /// <summary>
        /// Reference to World instance.
        /// </summary>
        private readonly World WorldRef;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="world">Reference to World instance</param>
        internal EntitySystemsStorage(World world)
        {
            WorldRef = world;
            m_SystemsList = new Dictionary<Type, BaseSystem>();
        }

        /// <summary>
        /// Add new System to World.
        /// </summary>
        /// <param name="system">New system.</param>
        public void AddSystem(BaseSystem system)
        {
            AddSystem(system, out bool created);
        }

        /// <summary>
        /// Add new System to World.
        /// </summary>
        /// <param name="system">New system.</param>
        /// <param name="created">Created flag.</param>
        public void AddSystem(BaseSystem system, out bool created)
        {
#if DEBUG
            if (HasSystem(system.GetType()))
            {
                created = false;
                throw new SystemAlreadyExistsException();
            }
#endif
            system.Init(WorldRef);
            m_SystemsList.Add(system.GetType(), system);
            created = true;
        }

        /// <summary>
        /// Determines if the World has a given type System.
        /// </summary>
        /// <returns><c>true</c>, if the World has a given type System, <c>false</c> otherwise.</returns>
        /// <typeparam name="T">BaseSystem to check.</typeparam>
        public bool HasSystem<T>() where T : BaseSystem
        {
            return HasSystem(typeof(T));
        }

        /// <summary>
        /// Determines if the World has a given type System.
        /// </summary>
        /// <returns><c>true</c>, if the World has a given type System, <c>false</c> otherwise.</returns>
        /// <param name="systemType">BaseSystem type to check.</param>
        public bool HasSystem(Type systemType)
        {
#if DEBUG
            if (!systemType.IsSystem())
            {
                throw new TypeNotSystemException();
            }
#endif
            return m_SystemsList.ContainsKey(systemType);
        }

        /// <summary>
        /// Removes a system from World.
        /// </summary>
        /// <param name="systemType">BaseSystem type to remove.</param>
        public void RemoveSystem(Type systemType)
        {
#if DEBUG
            if (!systemType.IsSystem())
            {
                throw new TypeNotSystemException();
            }
            if (!HasSystem(systemType))
            {
                throw new SystemNotFoundException();
            }
#endif
            m_SystemsList.Remove(systemType);
        }

        /// <summary>
        /// Removes a system from World.
        /// </summary>
        /// <param name="system">System instance to remove.</param>
        public void RemoveSystem(BaseSystem system)
        {
            Type systemType = system.GetType();
#if DEBUG
            if (!HasSystem(systemType) || GetSystem(systemType) != system)
            {
                throw new SystemNotFoundException();
            }
#endif
            m_SystemsList.Remove(systemType);
        }

        /// <summary>
        /// Retrieves a system in World.
        /// </summary>
        /// <typeparam name="T">BaseSystem type to retrieve.</typeparam>
        /// <returns>BaseSystem instance if found, null otherwise.</returns>
        public T GetSystem<T>() where T : BaseSystem
        {
            return (T)GetSystem(typeof(T));
        }

        /// <summary>
        /// Retrieves a system in World.
        /// </summary>
        /// <param name="systemType">BaseSystem type to retrieve.</param>
        /// <returns>BaseSystem instance.</returns>
        public BaseSystem GetSystem(Type systemType)
        {
#if DEBUG
            if (!systemType.IsSystem())
            {
                throw new TypeNotSystemException();
            }
#endif
            m_SystemsList.TryGetValue(systemType, out BaseSystem foundSystem);
            return foundSystem;
        }

        /// <summary>
        /// Retrieves an all BaseSystem instance in World.
        /// </summary>
        /// <returns>BaseSystem instances list.</returns>
        public List<BaseSystem> GetSystems()
        {
            return m_SystemsList.Values.ToList();
        }
        
        /// <summary>
        /// Cleanup method.
        /// </summary>
        internal void Destroy()
        {
            foreach (var item in m_SystemsList)
            {
                item.Value.InternalDestroy();
            }
            m_SystemsList.Clear();
        }
    }
}
