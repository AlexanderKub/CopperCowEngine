using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

            componentSize = componentType.Size;
            return newIndex;
        }

        public Type GetTypeAtIndex(int index)
        {
            return TypesCollection.GetAt(index).BackedType;
        }

        public ComponentType GetComponentTypeAtIndex(int index)
        {
            return TypesCollection.GetAt(index);
        }
    }
}
