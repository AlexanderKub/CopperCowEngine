using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    public struct ComponentsFilter : IEquatable<ComponentsFilter>
    {
        public static readonly ComponentsFilter Empty = new ComponentsFilter();
        public static readonly Type[] EmptyTypes = new Type[0];

        public Type[] RequiresAll;
        public Type[] RequiresOne;
        public Type[] Excludes;

        public override bool Equals(object obj) =>
            (obj is ComponentsFilter filtter) && Equals(filtter);

        public bool Equals(ComponentsFilter other) {
            if (RequiresAll.Length != other.RequiresAll.Length || RequiresAll.Except(other.RequiresAll).Any())
            {
                return false;
            }
            if (RequiresOne.Length != other.RequiresOne.Length || RequiresOne.Except(other.RequiresOne).Any())
            {
                return false;
            }
            if (Excludes.Length != other.Excludes.Length || Excludes.Except(other.Excludes).Any())
            {
                return false;
            }
            return true;
        }

        public static bool operator ==(ComponentsFilter left, ComponentsFilter right) => Equals(left, right);

        public static bool operator !=(ComponentsFilter left, ComponentsFilter right) => !Equals(left, right);

        // Maybe wrong
        public override int GetHashCode() => Tuple.Create(RequiresAll, RequiresOne, Excludes).GetHashCode();
    }

    internal class SystemComponentsFilters
    {
        private class Callbacks {
            public Action<Entity> OnAdded;
            public Action<Entity> OnRemoved;
        }

        private List<ComponentsFilter> m_RegisteredFiltersList;
        private List<Callbacks> m_RegisteredFiltersCallbacks;

        /// <summary>
        /// Filtered entities cache.
        /// </summary>
        private List<List<Entity>> m_FilteredEntities;

        private EntityCache EntityCacheRef;

        public SystemComponentsFilters(EntityCache entityCache)
        {
            EntityCacheRef = entityCache;
            m_RegisteredFiltersList = new List<ComponentsFilter>();
            m_FilteredEntities = new List<List<Entity>>();
            m_RegisteredFiltersCallbacks = new List<Callbacks>();
        }

        internal void RemoveSystemListeners(Action<Entity> OnAdded, Action<Entity> OnRemoved, int FilterId)
        {
            m_RegisteredFiltersCallbacks[FilterId].OnAdded -= OnAdded;
            m_RegisteredFiltersCallbacks[FilterId].OnRemoved -= OnRemoved;
        }

        internal int RegisterEntityFilter(Type requires, Action<Entity> OnAdded, Action<Entity> OnRemoved)
        {
            return RegisterEntityFilter(requires, null, null, OnAdded, OnRemoved);
        }

        internal int RegisterEntityFilter(Type requires, Type excludes, Action<Entity> OnAdded, Action<Entity> OnRemoved)
        {
            return RegisterEntityFilter(requires, excludes, null, OnAdded, OnRemoved);
        }

        internal int RegisterEntityFilter(Type requires, Type excludes, Type requiresOne, Action<Entity> OnAdded, Action<Entity> OnRemoved)
        {
            ComponentsFilter newOne = new ComponentsFilter()
            {
                RequiresAll = requires.GetGenericArguments(),
                RequiresOne = excludes?.GetGenericArguments() ?? ComponentsFilter.EmptyTypes,
                Excludes = requiresOne?.GetGenericArguments() ?? ComponentsFilter.EmptyTypes,
            };

            int filterID;

            if (m_RegisteredFiltersList.IndexOf(newOne) > -1) {
                filterID = m_RegisteredFiltersList.IndexOf(newOne);
                m_RegisteredFiltersCallbacks[filterID].OnAdded += OnAdded;
                m_RegisteredFiltersCallbacks[filterID].OnRemoved += OnRemoved;
                return filterID;
            }

            m_RegisteredFiltersList.Add(newOne);
            m_FilteredEntities.Add(new List<Entity>());
            m_RegisteredFiltersCallbacks.Add(new Callbacks());
            filterID = m_RegisteredFiltersList.Count - 1;
            m_RegisteredFiltersCallbacks[filterID].OnAdded += OnAdded;
            m_RegisteredFiltersCallbacks[filterID].OnRemoved += OnRemoved;
            FilterCreatedEntities(filterID);

            return filterID;
        }

        /// <summary>
        /// Check given Entity is in filtered list.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <returns><c>true</c> if entity in filtered list, <c>false</c> otherwise.</returns>
        internal bool IsEntityInFiltered(Entity entity, int FilterId)
        {
            return m_FilteredEntities[FilterId].Contains(entity);
        }

        /// <summary>
        /// Add given Entity to filtered cache.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <param name="FilterId">Filter id.</param>
        private void AddEntityToFiltered(Entity entity, int FilterId)
        {
            AddEntityToFiltered(entity, FilterId, true);
        }

        /// <summary>
        /// Add given Entity to filtered cache.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <param name="FilterId">Filter id.</param>
        /// <param name="checkExist">Check contains or not.</param>
        private void AddEntityToFiltered(Entity entity, int FilterId, bool checkExist)
        {
            if (checkExist)
            {
                if (IsEntityInFiltered(entity, FilterId))
                {
                    return;
                }
            }
            
            m_FilteredEntities[FilterId].Add(entity);
            m_RegisteredFiltersCallbacks[FilterId].OnAdded(entity);
        }

        /// <summary>
        /// Remove given Entity from filtered cache.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        private void RemoveEntityFromFiltered(Entity entity, int FilterId)
        {
            if (!IsEntityInFiltered(entity, FilterId))
            {
                return;
            }

            m_RegisteredFiltersCallbacks[FilterId].OnRemoved(entity);
            m_FilteredEntities[FilterId].Remove(entity);
        }

        internal void RemoveEntityFromAllFiltered(Entity entity)
        {
            for (int i = 0; i < m_FilteredEntities.Count; i++)
            {
                RemoveEntityFromFiltered(entity, i);
            }
        }

        internal void FilterActivatedEntities(Entity entity)
        {
            for (int i = 0; i < m_FilteredEntities.Count; i++)
            {
                bool passFilter = DoesEntityPassFilter(entity, i);
                if (passFilter) {
                    AddEntityToFiltered(entity, i);
                } else {
                    RemoveEntityFromFiltered(entity, i);
                }
            }
        }

        private void FilterCreatedEntities(int FilterId)
        {
            foreach (var entity in EntityCacheRef.Created)
            {
                if (!entity.IsActive)
                {
                    continue;
                }
                if (DoesEntityPassFilter(entity, FilterId))
                {
                    AddEntityToFiltered(entity, FilterId, false);
                }
            }
        }

        internal bool DoesEntityPassFilter(Entity entity, int FilterId)
        {
            bool result = true;

            ComponentsFilter Filter = m_RegisteredFiltersList[FilterId];

            // Entity must have all of these components
            foreach (Type componentType in Filter.RequiresAll)
            {
                if (!entity.HasComponent(componentType))
                {
                    result = false;
                    break;
                }
            }

            // Entity must have at least one of these components
            if (Filter.RequiresOne.Length > 0)
            {
                bool hasOne = false;
                foreach (Type componentType in Filter.RequiresOne)
                {
                    if (entity.HasComponent(componentType))
                    {
                        hasOne = true;
                        break;
                    }

                }
                result &= hasOne;
            }

            // Entity must have none of these components
            if (Filter.Excludes.Length > 0)
            {
                bool hasOne = false;
                foreach (Type componentType in Filter.Excludes)
                {
                    if (entity.HasComponent(componentType))
                    {
                        hasOne = true;
                        break;
                    }
                }
                result &= !hasOne;
            }

            return result;
        }

        /// Get entities what pass given Filter.
        /// </summary>
        /// <param name="FilterID">Given Filter id.</param>
        /// <returns>List of Entity.</returns>
        internal Entity[] GetEntities(int FilterID)
        {
            if (FilterID < 0 || FilterID > m_FilteredEntities.Count() - 1)
            {
                return null;
            }
            return m_FilteredEntities[FilterID].ToArray();
        }

        internal void Destroy()
        {
            m_RegisteredFiltersList.Clear();
            m_RegisteredFiltersCallbacks.Clear();
            m_FilteredEntities.Clear();
        }
    }
}
