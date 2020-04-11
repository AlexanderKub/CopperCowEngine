using System;
using System.Runtime.InteropServices;

namespace CopperCowEngine.ECS.DataChunks
{
    internal struct ComponentType : IEquatable<ComponentType>
    {
        private int _id;

        public Type BackedType { get; }

        public int Size { get; }

        public int Id
        {
            get => _id;
            set { if (_id == -1) _id = value; }
        }

        public ComponentType(Type type)
        {
            BackedType = type;

            Size = Marshal.SizeOf(type);

            _id = -1;
        }

        public override bool Equals(object other) => other != null && Equals((ComponentType) other);

        public bool Equals(ComponentType other)
        {
            return BackedType == other.BackedType;
        }

        public override int GetHashCode()
        {
            return BackedType.GetHashCode();
        }
    }
}
