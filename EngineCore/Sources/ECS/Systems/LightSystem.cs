using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineCore;
using EngineCore.ECS.Components;
using EngineCore.Utils;

namespace EngineCore.ECS.Systems
{
    public class LightSystem : BasicSystem<Requires<Transform, Light>> {

        public override void Update(Timer timer)
        {
            Entity[] entities = GetEntities();
            SingletonFrameScene frameScene = WorldRef.GetSingletonComponent<SingletonFrameScene>();

            Transform transform;
            Light light;

            entities = entities.OrderBy(a => a.GetComponent<Light>().Type).ToArray();

            int index = 0;
            bool culled;
            BoundingSphere boundingSphere;

            foreach (var entity in entities)
            {
                culled = true;
                transform = entity.GetComponent<Transform>();
                light = entity.GetComponent<Light>();

                foreach (var camera in frameScene.FrameData.CamerasList)
                {
                    boundingSphere = new BoundingSphere(transform.Position, light.Radius * 2);
                    if (light.Type != Light.LightType.Directional && !camera.Frustrum.Intersects(ref boundingSphere))
                    {
                        continue;
                    }
                    culled = false;
                    frameScene.FrameData.AddLightDataToCamera(camera.index, index);
                }

                if (culled) { continue; }

                if (light.IsCastShadows)
                {
                    light.ViewProjection = Matrix.LookAtLH(
                        transform.Position,
                        transform.Position + transform.Direction,
                        Vector3.Up
                    );
                    //light.ViewProjection *= Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1.0f, 0.01f, 10000f);
                    light.ViewProjection *= Matrix.OrthoLH(110f, 110f, 0.5f, 300f);
                }

                frameScene.FrameData.AddLightData(new StandardFrameData.LightData() {
                    Type = light.Type,
                    Radius = light.Radius,
                    IsCastShadows = light.IsCastShadows,
                    ViewProjection = light.ViewProjection,
                    Frustrum = new BoundingFrustum(light.ViewProjection),
                    Position = transform.Position,
                    Direction = transform.Direction,
                    Color = light.Color,
                    Intensity = light.Intensity,
                });
                index++;
            }
        }

    }
}
