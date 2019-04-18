using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS
{
    /// <summary>
    /// Defines several utility extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines if a Type is an IEntityComponent.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an IEntityComponent, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsComponent(this Type type)
        {
            return typeof(IEntityComponent).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an ISingletonEntityComponent.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an ISingletonEntityComponent, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsSingletonComponent(this Type type)
        {
            return typeof(ISingletonEntityComponent).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an IEntityComponentWithEntityId.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an IEntityComponentWithEntityId, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsComponentWithEntityId(this Type type)
        {
            return typeof(IEntityComponentWithEntityId).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an BaseSystem.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an BaseSystem, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsSystem(this Type type)
        {
            return typeof(BaseSystem).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an Requires filter.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an Requires, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsRequires(this Type type)
        {
            return typeof(Requires).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an Excludes filter.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an Excludes, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsExcludes(this Type type)
        {
            return typeof(Excludes).IsAssignableFrom(type);
        }

        /// <summary>
        /// Determines if a Type is an RequiresOne filter.
        /// </summary>
        /// <returns><c>true</c>, if <paramref name="type"/> is an RequiresOne, <c>false</c> otherwise.</returns>
        /// <param name="type">Type to check.</param>
        public static bool IsRequiresOne(this Type type)
        {
            return typeof(RequiresOne).IsAssignableFrom(type);
        }
    }
}
