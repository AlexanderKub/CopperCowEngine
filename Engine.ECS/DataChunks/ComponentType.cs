using System;
using System.Runtime.InteropServices;

namespace CopperCowEngine.ECS.DataChunks
{
    internal struct ComponentType
    {
        public Type BackedType { get; }

        public int Size { get; }

        public int Id
        {
            get => _id;
            set { if (_id == -1) _id = value; }
        }

        private int _id;

        public ComponentType(Type type)
        {
            BackedType = type;

            Size = Marshal.SizeOf(type);

            _id = -1;
        }
    }
}
