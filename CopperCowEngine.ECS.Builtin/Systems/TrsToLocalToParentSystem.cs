using CopperCowEngine.ECS.Builtin.Components;
using SharpDX;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class TrsToLocalToParentSystem : ComponentSystem<Required<LocalToParent, Parent>, Optional<Translation, Rotation, Scale>>
    {
        protected override void Update()
        {
            foreach (var slice in Iterator)
            {
                ref var locToParent = ref slice.Sibling<LocalToParent>();

                locToParent.Value = Matrix.Identity;

                if (slice.HasSibling<Rotation>())
                {
                    locToParent.Value *= Matrix.RotationQuaternion(slice.Sibling<Rotation>().Value);
                }

                if (slice.HasSibling<Scale>())
                {
                    locToParent.Value *= Matrix.Scaling(slice.Sibling<Scale>().Value);
                }

                if (slice.HasSibling<Translation>())
                {
                    locToParent.Value *= Matrix.Translation(slice.Sibling<Translation>().Value);
                }
            }
        }
    }
}
