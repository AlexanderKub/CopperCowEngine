using CopperCowEngine.ECS.Builtin.Components;
using SharpDX;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class TrsToLocalToWorldSystem : ComponentSystem<Required<LocalToWorld>, Optional<Translation, Rotation, Scale>, Excepted<Parent>>
    {
        protected override void Update()
        {
            foreach (var slice in Iterator)
            {
                ref var locToWorld = ref slice.Sibling<LocalToWorld>();

                locToWorld.Value = Matrix.Identity;

                if (slice.HasSibling<Rotation>())
                {
                    locToWorld.Value *= Matrix.RotationQuaternion(slice.Sibling<Rotation>().Value);
                }

                if (slice.HasSibling<Scale>())
                {
                    locToWorld.Value *= Matrix.Scaling(slice.Sibling<Scale>().Value);
                }

                if (slice.HasSibling<Translation>())
                {
                    locToWorld.Value *= Matrix.Translation(slice.Sibling<Translation>().Value);
                }
            }
        }
    }
}
