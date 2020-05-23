using System.Numerics;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.JobSystem;
using CopperCowEngine.Unsafe.Collections;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class TrsToLocalToWorldSystem : ComponentSystem<Required<LocalToWorld>, Optional<Translation, Rotation, Scale, NonUniformScale>, Excepted<Parent>>
    {
        protected override void Update()
        {
            foreach (var slice in Iterator)
            {
                ref var locToWorld = ref slice.Sibling<LocalToWorld>();

                locToWorld.PreviousValue = locToWorld.Value;
                locToWorld.Value = Matrix4x4.Identity;

                if (slice.HasSibling<Scale>())
                {
                    locToWorld.Value *= Matrix4x4.CreateScale(slice.Sibling<Scale>().Value);
                } 
                else if (slice.HasSibling<NonUniformScale>())
                {
                    locToWorld.Value *= Matrix4x4.CreateScale(slice.Sibling<NonUniformScale>().Value);
                }

                if (slice.HasSibling<Rotation>())
                {
                    locToWorld.Value *= Matrix4x4.CreateFromQuaternion(slice.Sibling<Rotation>().Value);
                }

                if (slice.HasSibling<Translation>())
                {
                    locToWorld.Value *= Matrix4x4.CreateTranslation(slice.Sibling<Translation>().Value);
                }
            }
        }
    }
}
