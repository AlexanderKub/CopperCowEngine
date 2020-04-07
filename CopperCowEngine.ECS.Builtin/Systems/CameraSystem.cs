using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;
using SharpDX;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class CameraSystem : ComponentSystem<Required<LocalToWorld, CameraSetup, CameraProjection, CameraViewProjection>>
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;

            var frameData = (StandardFrameData)engine.RenderingFrameData;

            foreach (var slice in Iterator)
            {
                var setup = slice.Sibling<CameraSetup>();

                var locToWorld = slice.Sibling<LocalToWorld>();

                ref var projection = ref slice.Sibling<CameraProjection>();

                ref var viewProjection = ref slice.Sibling<CameraViewProjection>();

                // TODO: check inverse depth buffer
                projection.Value = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, setup.AspectRatio, 
                    setup.FarClippingPlane, setup.NearClippingPlane);

                var position = locToWorld.Value.TranslationVector;

                var forward = position + locToWorld.Value.Backward;

                viewProjection.View = Matrix.LookAtLH(position, forward, locToWorld.Value.Up);

                viewProjection.ViewProjection = viewProjection.View * projection.Value;

                frameData.AddCameraData(new CameraData
                {
                    Forward = forward,
                    Frustum = new BoundingFrustum(viewProjection.ViewProjection),
                    Index = 0,
                    Position = position,

                    PreviousView = viewProjection.View,
                    PreviousViewProjection = viewProjection.ViewProjection,

                    Projection = projection.Value,
                    View = viewProjection.View,
                    ViewProjection = viewProjection.ViewProjection,
                });
            }
        }
    }
}
