using System;

namespace CopperCowEngine.ECS
{
    public struct Entity : IEquatable<Entity>
    {
        public static Entity Null { get; } = new Entity(-1, -1);

        public int Id { get; }

        public int Version { get; }

        private Entity(int id, int version)
        {
            Id = id;
            Version = version;
        }

        public override bool Equals(object other) => other != null && Equals((Entity) other);

        public bool Equals(Entity other)
        {
            return Id == other.Id && Version == other.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        public override string ToString()
        {
            return $"{Id} : {Version}";
        }

        public static bool operator ==(Entity lhs, Entity rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Entity lhs, Entity rhs)
        {
            return !(lhs == rhs);
        }

        internal static Entity Create(int id)
        {
            return new Entity(id, 0);
        }

        internal static Entity Recycle(int id, int version)
        {
            return new Entity(id, version);
        }
    }
}
