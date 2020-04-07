using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;
using SharpDX;

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
