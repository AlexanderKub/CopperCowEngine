using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineCore.ECS.Components;


namespace EngineCore.ECS.Systems
{
    public class CamerasSystem : BasicSystem<Requires<Camera, Transform>> {

        public override void Update(Timer timer)
        {
            Entity[] entities = GetEntities();

            SingletonConfigVar configVar = WorldRef.GetSingletonComponent<SingletonConfigVar>();
            SingletonInput input = WorldRef.GetSingletonComponent<SingletonInput>();
            SingletonFrameScene frameScene = WorldRef.GetSingletonComponent<SingletonFrameScene>();
            frameScene.FrameData.Reset();

            Transform transform;
            Camera camera;

            foreach (var entity in entities)
            {
                camera = entity.GetComponent<Camera>();
                transform = entity.GetComponent<Transform>();
                
                UpdateCamera(timer.DeltaTime, camera, transform, input, configVar);
                frameScene.FrameData.AddCameraData(new StandardFrameData.CameraData() {
                    View = camera.View,
                    Projection = camera.Projection,
                    ViewProjection = camera.ViewProjection,
                    PreviousView = camera.PreviousView,
                    PreviousViewProjection = camera.PreviousViewProjection,
                    Position = transform.Position,
                    Forward = transform.Direction,
                    Frustrum = new BoundingFrustum(camera.ViewProjection),
                });
            }
        }

        private void UpdateCamera(float DeltaTime, Camera camera, Transform transform, SingletonInput input, SingletonConfigVar configVar)
        {
            #region Test free camera
            camera.Yaw += input.MouseXOffset * 10f * DeltaTime;
            camera.Pitch += input.MouseYOffset * 10f * DeltaTime;
            if (camera.Pitch >= 90f) {
                camera.Pitch = 89.0001f;
            }

            if (camera.Pitch <= -90f) {
                camera.Pitch = -89.0001f;
            }

            transform.Rotation = Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(camera.Yaw), MathUtil.DegreesToRadians(camera.Pitch), 0f);

            Vector3 left = Vector3.Cross(Vector3.Normalize(transform.Direction), Vector3.Up);
            left = Vector3.Normalize(left);
            float speed = 2f;
            if (input.IsButtonDown(SingletonInput.Buttons.LSHIFT))
            {
                speed *= 5f;
            }
            if (input.IsButtonDown(SingletonInput.Buttons.LEFT))
            {
                transform.Position += left * DeltaTime * speed;
            }
            if (input.IsButtonDown(SingletonInput.Buttons.RIGHT))
            {
                transform.Position += -left * DeltaTime * speed;
            }
            if (input.IsButtonDown(SingletonInput.Buttons.DOWN))
            {
                transform.Position += -Vector3.Normalize(transform.Direction) * DeltaTime * speed;
            }
            if (input.IsButtonDown(SingletonInput.Buttons.UP))
            {
                transform.Position += Vector3.Normalize(transform.Direction) * DeltaTime * speed;
            }
            #endregion
            camera.PreviousView = camera.View;
            camera.PreviousViewProjection = camera.ViewProjection;
            camera.View = Matrix.LookAtLH(
                transform.Position,
                transform.Position + transform.Direction,
                Vector3.Up
            );

            if (camera.IsScreenView) {
                camera.AspectRatio = configVar.ScreenAspectRatio;
            }

            if (configVar.IsInvertedDepthBuffer) {
                camera.Projection = Matrix.PerspectiveFovLH(camera.FOV, camera.AspectRatio, camera.FarClippingPlane, camera.NearClippingPlane);
            } else {
                camera.Projection = Matrix.PerspectiveFovLH(camera.FOV, camera.AspectRatio, camera.NearClippingPlane, camera.FarClippingPlane);
            }
            camera.ViewProjection = camera.View * camera.Projection;
        }
    }
}
