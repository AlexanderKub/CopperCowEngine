using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// Class for perform Entity pooling.
    /// </summary>
    internal class EntityPool
    {
        /// <summary>
        /// Reference to World instance.
        /// </summary>
        private readonly World WorldRef;

        /// <summary>
        /// A pool for Entities.
        /// </summary>
        private HashSet<Entity> FreeEntitiesPool;

        /// <summary>
        /// Counter for generate unique ID.
        /// </summary>
        private int EntityCounter = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="world">Reference to World instance</param>
        public EntityPool(World world)
        {
            WorldRef = world;
            FreeEntitiesPool = new HashSet<Entity>();
        }

        /// <summary>
        /// Get entity from pool
        /// </summary>
        internal Entity GetNewEntity()
        {
            Entity entity;

            if (FreeEntitiesPool.Count > 0)
            {
                entity = FreeEntitiesPool.First();
            }
            else
            {
                FreeEntitiesPool.Add(new Entity(WorldRef, EntityCounter++));
                WorldRef.ComponentsStorage.AddEntityData();
                entity = FreeEntitiesPool.First();
            }

            FreeEntitiesPool.Remove(entity);
            entity.Reinit();
            return entity;
        }

        /// <summary>
        /// Return entity to pool
        /// </summary>
        internal void RemoveEntity(Entity entity)
        {
            entity.IsValid = false;
            FreeEntitiesPool.Add(entity);
        }

        /// <summary>
        /// Destroy pool
        /// </summary>
        internal void Destroy()
        {
            FreeEntitiesPool.Clear();
        }
    }
}
