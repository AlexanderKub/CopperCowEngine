using System;
using System.Collections.Generic;

namespace CopperCowEngine.ECS.Base
{
    internal sealed class SingletonComponentsDataStorage
    {
        private readonly List<SingletonComponentData> _singletonComponents;

        public SingletonComponentsDataStorage()
        {
            _singletonComponents = new List<SingletonComponentData>();
        }
        
        public ref T GetSingletonComponent<T>() where T : struct, ISingletonComponentData
        {
            var type = typeof(T);
            var index = _singletonComponents.FindIndex(c => c.Type == type);

            if (index < 0)
            {
                _singletonComponents.Add(new SingletonComponentDataArray<T>());
                index = _singletonComponents.Count - 1;
            }

            // ReSharper disable once UseNegatedPatternMatching
            var array = _singletonComponents[index] as SingletonComponentDataArray<T>;
#if DEBUG
            if (array == null)
            {
                throw new NullReferenceException();
            }
#endif
            return ref array.Data[0];
        }

        private class SingletonComponentData : IEquatable<SingletonComponentData>
        {
            public readonly Type Type;

            protected SingletonComponentData(Type type)
            {
                Type = type;
            }

            public override bool Equals(object other) => Equals(other as SingletonComponentData);

            public bool Equals(SingletonComponentData other)
            {
                return Type == other?.Type;
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode();
            }
        }

        private class SingletonComponentDataArray<T> : SingletonComponentData where T : struct, ISingletonComponentData
        {
            public readonly T[] Data = new T[1];

            public SingletonComponentDataArray() : base(typeof(T))
            {
                Data[0] = new T();
            }
        }
    }
}
