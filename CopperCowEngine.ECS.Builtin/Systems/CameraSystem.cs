using System;
using System.Numerics;
using CopperCowEngine.Core;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;
using CopperCowEngine.Rendering.Geometry;

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
                projection.Value = MatrixUtils.PerspectiveFovLeftHand((float)(Math.PI * 0.5), setup.AspectRatio, 
                    setup.FarClippingPlane, setup.NearClippingPlane);

                var position = locToWorld.Value.Translation;

                var cameraBackward = new Vector3(locToWorld.Value.M31, locToWorld.Value.M32, locToWorld.Value.M33);
                var forward = position - cameraBackward;

                var previousView = viewProjection.View;
                var previousViewProjection = viewProjection.ViewProjection;

                viewProjection.View = Matrix4x4.CreateLookAt(position, forward, Vector3.UnitY);
                viewProjection.ViewProjection = viewProjection.View * projection.Value; 

                frameData.AddCameraData(new CameraData
                {
                    Forward = forward,
                    Frustum = new BoundingFrustum(viewProjection.ViewProjection),
                    Index = 0,
                    FrameTime = Time.Delta,
                    Position = position,

                    PreviousView = previousView,
                    PreviousViewProjection = previousViewProjection,

                    Projection = projection.Value,
                    View = viewProjection.View,
                    ViewProjection = viewProjection.ViewProjection,
                });
            }
        }
    }
}
