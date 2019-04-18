using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    public abstract class BaseSystem
    {
        /// <summary>
        /// Components filter for this System.
        /// </summary>
        public int FilterID { get; protected set; }

        /// <summary>
        /// Reference to World Instance.
        /// </summary>
        protected World WorldRef { get; private set; }

        /// <summary>
        /// Enabled state of this System.
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Initialize System.
        /// </summary>
        /// <param name="world">Reference to World Instance.</param>
        internal void Init(World world)
        {
            WorldRef = world;
            Enabled = true;
            SetupFilter();
            OnInit();
        }

        /// <summary>
        /// Destroy this System.
        /// </summary>
        public void Destroy()
        {
            InternalDestroy();
            WorldRef.RemoveSystem(this);
            WorldRef = null;
        }

        internal void InternalDestroy()
        {
            Enabled = false;
            OnDestroy();
            WorldRef.SystemFiltersStorage.RemoveSystemListeners(OnEntityAdded, OnEntityRemoved, FilterID);
        }

        /// <summary>
        /// Check if given Entity does pass System filter.
        /// </summary>
        /// <param name="entity">Given Entity.</param>
        /// <returns><c>true</c> if does, <c>false</c> if doesn't.</returns>
        internal bool DoesEntityPassFilter(Entity entity)
        {
            return WorldRef.SystemFiltersStorage.DoesEntityPassFilter(entity, FilterID);
        }

        /// <summary>
        /// System components filter. Override for every System.
        /// </summary>
        internal protected virtual void SetupFilter()
        {
            FilterID = -1;
        }

        /// <summary>
        /// Get entities what this System affect.
        /// </summary>
        /// <returns>List of Entity.</returns>
        protected Entity[] GetEntities()
        {
            return WorldRef.SystemFiltersStorage.GetEntities(FilterID);
        }

        /// <summary>
        /// Life-cycle hook for on System initialize actions.
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Life-cycle hook for on entity added actions.
        /// </summary>
        protected virtual void OnEntityAdded(Entity entity) { }

        /// <summary>
        /// Life-cycle hook for on entity removed actions.
        /// </summary>
        protected virtual void OnEntityRemoved(Entity entity) { }

        /// <summary>
        /// Life-cycle hook for on System update actions.
        /// </summary>
        /// <param name="deltaTime">Frame delta time.</param>
        public virtual void Update(Timer timer) { }

        /// <summary>
        /// Life-cycle hook for on System destroy actions.
        /// </summary>
        protected virtual void OnDestroy() { }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <typeparam name="R">Requires filter type.</typeparam>
    public abstract class BasicSystem<R> : BaseSystem where R : Requires
    {
        internal protected override void SetupFilter()
        {
            Type RType = typeof(R);
            FilterID = WorldRef.SystemFiltersStorage.RegisterEntityFilter(RType, OnEntityAdded, OnEntityRemoved);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <typeparam name="R">Requires filter type.</typeparam>
    /// <typeparam name="E">Excludes filter type.</typeparam>
    public abstract class BasicSystem<R, E> : BaseSystem where R : Requires where E : Excludes
    {
        internal protected override void SetupFilter()
        {
            Type RType = typeof(R);
            Type EType = typeof(E);
            FilterID = WorldRef.SystemFiltersStorage.RegisterEntityFilter(RType, EType, OnEntityAdded, OnEntityRemoved);
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <typeparam name="R">Requires filter type.</typeparam>
    /// <typeparam name="E">Excludes filter type.</typeparam>
    /// <typeparam name="C">RequiresOne filter type.</typeparam>
    public abstract class BasicSystem<R, E, C> : BaseSystem where R : Requires where E : Excludes where C : RequiresOne
    {
        internal protected override void SetupFilter()
        {
            Type RType = typeof(R);
            Type EType = typeof(E);
            Type CType = typeof(E);
            FilterID = WorldRef.SystemFiltersStorage.RegisterEntityFilter(RType, EType, CType, OnEntityAdded, OnEntityRemoved);
        }
    }
}
