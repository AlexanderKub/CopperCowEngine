using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class CameraScreenAspectSystem : ComponentSystem<Required<CameraSetup, CameraScreenAspect>>
    {
        protected override void Update()
        {
            foreach (var slice in Iterator)
            {
                ref var setup = ref slice.Sibling<CameraSetup>();

                setup.AspectRatio = Screen.AspectRatio;
            }
        }
    }
}
