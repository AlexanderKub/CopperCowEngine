using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineCore;
using EngineCore.ECS.Components;
using EngineCore.Utils;
using System.Collections;

namespace EngineCore.ECS.Systems
{
    public class RenderSystem : BasicSystem<Requires<Transform, Renderer>> {

        private class OrderByRender : IComparer<Entity>
        {
            public int Compare(Entity x, Entity y)
            {
                Renderer a, b;
                a = x.GetComponent<Renderer>();
                b = y.GetComponent<Renderer>();

                if (a == null && b == null) {
                    return 0;
                }

                if (a.materialInfo.Queue != b.materialInfo.Queue)
                {
                    return a.materialInfo.Queue.CompareTo(b.materialInfo.Queue);
                }
                if (a.materialInfo.Name != b.materialInfo.Name)
                {
                    return a.materialInfo.Name.CompareTo(b.materialInfo.Name);
                }
                if (a.meshInfo.Name != b.meshInfo.Name)
                {
                    return a.meshInfo.Name.CompareTo(b.meshInfo.Name);
                }
                return x.ID.CompareTo(y.ID);
            }
        }

        private SortedSet<Entity> SortedByRendererEntities;
        public RenderSystem()
        {
            SortedByRendererEntities = new SortedSet<Entity>(new OrderByRender());
        }

        protected override void OnEntityAdded(Entity entity)
        {
            SortedByRendererEntities.Add(entity);
        }

        protected override void OnEntityRemoved(Entity entity)
        {
            SortedByRendererEntities.Remove(entity);
        }

        public override void Update(Timer timer)
        {
            SingletonFrameScene frameScene = WorldRef.GetSingletonComponent<SingletonFrameScene>();

            Transform transform;
            Renderer renderer;

            //TODO: faster ordering optimization
            /*Entity[] entities = GetEntities();
            entities = entities.OrderBy(a => a.GetComponent<Renderer>().materialInfo.Queue)
                    .ThenBy(a => a.GetComponent<Renderer>().materialInfo.Name)
                    .ThenBy(a => a.GetComponent<Renderer>().meshInfo.Name).ToArray();*/
                    
            int index = 0;
            bool culled;
            foreach (Entity entity in SortedByRendererEntities)
            {
                culled = true;

                transform = entity.GetComponent<Transform>();
                renderer = entity.GetComponent<Renderer>();

                renderer.Bounds = BoundsBox.TransformAABBFast(renderer.meshInfo.Bounds, transform.TransformMatrix);

                foreach (var camera in frameScene.FrameData.CamerasList)
                {
                    if (camera.Frustrum.Intersects(ref renderer.Bounds.boundingBox))
                    {
                        culled = false;
                        frameScene.FrameData.AddRendererDataToCamera(camera.index, index);
                    }
                }

                foreach (var light in frameScene.FrameData.LightsList)
                {
                    if (!light.IsCastShadows) {
                        continue;
                    }
                    if (light.Frustrum.Intersects(ref renderer.Bounds.boundingBox))
                    {
                        culled = false;
                        frameScene.FrameData.AddRendererDataToLight(light.index, index);
                    }
                }

                if (culled) { continue; }

                frameScene.FrameData.AddRendererData(new StandardFrameData.RendererData() {
                    EntityId = entity.ID,
                    PreviousTransformMatrix = transform.PreviousTransformMatrix,
                    TransformMatrix = transform.TransformMatrix,
                    MeshName = renderer.meshInfo.Name,
                    MaterialName = renderer.materialInfo.Name,
                    MaterialQueue = renderer.materialInfo.Queue,
                    IsDynamic = renderer.IsDynamic,
                });
                index++;
            }
        }

    }
}
