using System;
using System.Collections.Generic;
using CopperCowEngine.ECS.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class ComponentTypesStorage
    {
        private ChunkedArray<ComponentType> TypesCollection { get; }

        public ComponentTypesStorage()
        {
            TypesCollection  = new ChunkedArray<ComponentType>(256);
        }

        public int TryRegisterType(Type type)
        {
            return TryRegisterType(type, out _);
        }

        public int TryRegisterType(Type type, out int componentSize)
        {
            var componentType = new ComponentType(type);

            if (TypesCollection.TryFind(componentType, out var index))
            {
                componentSize = componentType.Size;
                return index;
            }

            var newIndex = TypesCollection.Add(componentType);
            TypesCollection.GetAt(newIndex).Id = newIndex;

#if DEBUG
            ComponentTypesHashSet.Add(type);
#endif

            componentSize = componentType.Size;
            return newIndex;
        }

        public ComponentType GetComponentTypeAtIndex(int index)
        {
            return TypesCollection.GetAt(index);
        }
    }

#if DEBUG
    internal static class ComponentTypesHashSet
    {
        private static readonly Dictionary<int, Type> Types = new Dictionary<int, Type>();

        public static void Add(Type item)
        {
            Types.TryAdd(item.GetHashCode(), item);
        }

        public static Type Get(ComponentType type)
        {
            Types.TryGetValue(type.GetHashCode(), out var resultType);
            return resultType;
        }
    }
#endif
}
