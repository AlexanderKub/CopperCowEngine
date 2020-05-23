using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.DataChunks
{
    [DebuggerTypeProxy(typeof(ComponentTypeDebugView))]
    internal struct ComponentType : IEquatable<ComponentType>
    {
        private int _id;

        private int BackedTypeHashCode { get; }

        public int Size { get; }

        public int Id
        {
            get => _id;
            set { if (_id == -1) _id = value; }
        }

        public ComponentType(Type type)
        {
            BackedTypeHashCode = type.GetHashCode();

            Size = Marshal.SizeOf(type);

            _id = -1;
        }

        public override bool Equals(object other) => other != null && Equals((ComponentType) other);

        public bool Equals(ComponentType other)
        {
            return BackedTypeHashCode == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return BackedTypeHashCode;
        }
    }

    internal sealed class ComponentTypeDebugView
    {
        private readonly ComponentType _componentType;

        public ComponentTypeDebugView(ComponentType componentType)
        {
            _componentType = componentType;
        }

        #if DEBUG
        public string ComponentType => ComponentTypesHashSet.Get(_componentType).Name;
        #endif
    }
}
