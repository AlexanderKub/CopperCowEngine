using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CopperCowEngine.ECS.DataChunks
{
    internal struct DataChunkArchetype : IEquatable<DataChunkArchetype>
    {

        public ImmutableArray<int> ComponentTypes { get; }

        public int ChunkCapacity { get; }

        public DataChunkArchetype(int dataSize, params int[] componentTypesIds) : this(dataSize, ImmutableArray.Create(componentTypesIds)) { }

        public DataChunkArchetype(int dataSize, ImmutableArray<int> componentTypes)
        {
            ComponentTypes = componentTypes;

            var n = componentTypes.Length;

            var size = sizeof(int) + dataSize;
            
            // Backed fields + Array service memory (https://stackoverflow.com/questions/1589669/overhead-of-a-net-array) + Types array
            // Possible wrong calculation
            var serviceMemory = sizeof(int) * (2 + n) + 24 * (n + 3);
            ChunkCapacity = (DataChunk.ChunkSize - serviceMemory) / size;
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

        public DataChunkArchetype Reduce<T>() where T : struct, IComponentData
        {
            if (ComponentTypes.Length == 1)
            {
                return Null;
            }

            var type = typeof(T);
            return !Types.Contains(type) ? this : new DataChunkArchetype(Types.Remove(type));
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

        //public ImmutableArray<Type> Types { get; }

        /*public DataChunkArchetype(params Type[] types) : this(ImmutableArray.Create(types)) { }

        public DataChunkArchetype(ImmutableArray<Type> types)
        {
            Types = types;

            var n = types.Length;
            var size = sizeof(int);

            for (var i = 0; i < n; i++)
            {
                size += Marshal.SizeOf(types[i]);
            }
            
            // Backed fields + Array service memory (https://stackoverflow.com/questions/1589669/overhead-of-a-net-array) + Types array
            // Possible wrong calculation
            var serviceMemory = sizeof(int) * 2 + 24 * (n + 3) + 24 * n;
            ChunkCapacity = (DataChunk.ChunkSize - serviceMemory) / size;
        }*/

        /*public DataChunkArchetype Extend<T>() where T : struct, IComponentData
        {
            if (Types == null)
            {
                return new DataChunkArchetype(typeof(T));
            }

            var type = typeof(T);
            return Types.Contains(type) ? this : new DataChunkArchetype(Types.Insert(0, type));
        }

        public DataChunkArchetype Reduce<T>() where T : struct, IComponentData
        {
            if (Types.Length == 1)
            {
                return Null;
            }

            var type = typeof(T);
            return !Types.Contains(type) ? this : new DataChunkArchetype(Types.Remove(type));
        }

        public bool Compatibility(in DataChunkArchetype other)
        {
            if (Types == null || other.Types == null)
            {
                return Types == other.Types;
            }

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var type in other.Types)
            {
                if (!Types.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Filter(Type[] required, Type[] optional, Type[] excepted)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in required)
            {
                if (!Types.Contains(type))
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
                if (Types.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(DataChunkArchetype other)
        {
            if (Types == null || other.Types == null)
            {
                return Types == other.Types;
            }

            if (Types.Length != other.Types.Length)
            {
                return false;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < Types.Length; i++)
            {
                if (Types[i] != other.Types[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static DataChunkArchetype Null { get; } = new DataChunkArchetype();

        public static bool Compatibility(in DataChunkArchetype archetype, params Type[] other)
        {
            if (archetype.Types == null)
            {
                return false;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in other)
            {
                if (!archetype.Types.Contains(type))
                {
                    return false;
                }
            }

            return true;
        }*/
    }
}
