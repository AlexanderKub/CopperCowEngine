using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// Class for store cache of entities, which stores all
    /// types of entities (Destroyed, Created, Activated, Deactivated)
    /// within the World.
    /// </summary>
    internal class EntityCache
    {
        /// <summary>
        /// Contains all the created entities
        /// </summary>
        public List<Entity> Created;

        /// <summary>
        /// A temporary storage for the destroyed entities
        /// for the world. This list gets cleared every call
        /// to refresh.
        /// </summary>
        public List<Entity> Destroyed;

        /// <summary>
        /// A temporary storage for the activated entities
        /// for the world. This array gets cleared every call
        /// to refresh.
        /// </summary>
        public List<Entity> Activated;

        /// <summary>
        /// A temporary storage for the deactivated entities
        /// for the world. This array gets cleared every call
        /// to refresh.
        /// </summary>
        public List<Entity> Deactivated;

        /// <summary>
        /// Get active entity by ID.
        /// </summary>
        /// <param name="id">Entity ID.</param>
        /// <returns>Entity or null</returns>
        public Entity GetEntity(int id)
        {
            if (Created.Count > id)
            {
                return Created[id].IsActive ? Created[id] : null;
            }
            return null;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EntityCache()
        {
            Created = new List<Entity>();
            Destroyed = new List<Entity>();
            Activated = new List<Entity>();
            Deactivated = new List<Entity>();
        }

        /// <summary>
        /// Clears the temporary cache
        /// </summary>
        public void ClearTemp()
        {
            Destroyed.Clear();
            Activated.Clear();
            Deactivated.Clear();
        }

        /// <summary>
        /// Clears everything in the cache
        /// </summary>
        public void Clear()
        {
            ClearTemp();
            Created.Clear();
        }
    }
}
