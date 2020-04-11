using System.Numerics;
using CopperCowEngine.ECS.Builtin.Components;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class TrsToLocalToParentSystem : ComponentSystem<Required<LocalToParent, Parent>, Optional<Translation, Rotation, Scale>>
    {
        protected override void Update()
        {
            foreach (var slice in Iterator)
            {
                ref var locToParent = ref slice.Sibling<LocalToParent>();

                locToParent.Value = Matrix4x4.Identity;

                if (slice.HasSibling<Rotation>())
                {
                    locToParent.Value *= Matrix4x4.CreateFromQuaternion(slice.Sibling<Rotation>().Value);
                }

                if (slice.HasSibling<Scale>())
                {
                    locToParent.Value *= Matrix4x4.CreateScale(slice.Sibling<Scale>().Value);
                }

                if (slice.HasSibling<Translation>())
                {
                    locToParent.Value *= Matrix4x4.CreateTranslation(slice.Sibling<Translation>().Value);
                }
            }
        }
    }
}
