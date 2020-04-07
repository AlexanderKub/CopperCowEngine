using System;

namespace CopperCowEngine.ECS
{
    public readonly struct Entity : IEquatable<Entity>
    {
        public static Entity Null { get; } = new Entity(-1);

        public int Id { get; }

        private Entity(int id)
        {
            Id = id;
        }

        public bool Equals(Entity other)
        {
            return Id == other.Id;
        }

        internal static Entity Create(int id)
        {
            return new Entity(id);
        }
    }
}
