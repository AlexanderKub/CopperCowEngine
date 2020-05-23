using System;
using System.Collections.Immutable;

namespace CopperCowEngine.ECS.DataChunks
{
    internal readonly struct DataChunkArchetype : IEquatable<DataChunkArchetype>
    {
        public ImmutableArray<int> ComponentTypes { get; }


        public DataChunkArchetype(int dataSize, params int[] componentTypesIds) 
            : this(dataSize, ImmutableArray.Create(componentTypesIds)) { }

        private DataChunkArchetype(int dataSize, ImmutableArray<int> componentTypes)
        {
            ComponentTypes = componentTypes;
        }

        public DataChunkArchetype Extend(int componentTypeSize, int componentTypeId)
        {
            if (ComponentTypes == null)
            {
                return new DataChunkArchetype(componentTypeSize, componentTypeId);
            }

            return ComponentTypes.Contains(componentTypeId) ? this : 
                new DataChunkArchetype(componentTypeSize, ComponentTypes.Insert(0, componentTypeId).Sort());
        }

        public DataChunkArchetype Reduce(int componentTypeSize, int componentTypeId)
        {
            if (ComponentTypes.Length == 1)
            {
                return Null;
            }

            return !ComponentTypes.Contains(componentTypeId) ? this :
                new DataChunkArchetype(componentTypeSize, ComponentTypes.Remove(componentTypeId).Sort());
        }

        public bool Compatibility(in DataChunkArchetype other)
        {
            if (ComponentTypes == null || other.ComponentTypes == null)
            {
                return ComponentTypes == other.ComponentTypes;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var type in other.ComponentTypes)
            {
                if (!ComponentTypes.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Filter(int[] required, int[] optional, int[] excepted)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in required)
            {
                if (!ComponentTypes.Contains(type))
                {
                    return false;
                }
            }

            if (excepted == null)
            {
                return true;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in excepted)
            {
                if (ComponentTypes.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj != null && Equals((DataChunkArchetype) obj);

        public bool Equals(DataChunkArchetype other)
        {
            if (ComponentTypes == null || other.ComponentTypes == null)
            {
                return ComponentTypes == other.ComponentTypes;
            }

            if (ComponentTypes.Length != other.ComponentTypes.Length)
            {
                return false;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < ComponentTypes.Length; i++)
            {
                if (ComponentTypes[i] != other.ComponentTypes[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return ComponentTypes.GetHashCode();
        }

        public static DataChunkArchetype Null { get; } = new DataChunkArchetype();

        public static bool Compatibility(in DataChunkArchetype archetype, params int[] other)
        {
            if (archetype.ComponentTypes == null)
            {
                return false;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in other)
            {
                if (!archetype.ComponentTypes.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
