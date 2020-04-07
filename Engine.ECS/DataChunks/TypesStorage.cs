using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CopperCowEngine.ECS.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    internal sealed class TypesStorage
    {
        private readonly ChunkedArray<ComponentType> _typesCollection = new ChunkedArray<ComponentType>(256);

        public int TryRegisterType(Type type)
        {
            return TryRegisterType(type, out _);
        }

        public int TryRegisterType(Type type, out ComponentType componentType)
        {
            componentType = new ComponentType(type);

            if (_typesCollection.TryFind(componentType, out var index))
            {
                return index;
            }

            var newIndex = _typesCollection.Add(componentType);

            _typesCollection.GetAt(newIndex).Id = newIndex;

            return newIndex;
        }

        public Type GetTypeAtIndex(int index)
        {
            return _typesCollection.GetAt(index).BackedType;
        }
    }
}
