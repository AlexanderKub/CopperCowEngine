using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class RenderingSystem : ComponentSystem<Required<LocalToWorld, Mesh, Material>>
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;

            var frameData = (StandardFrameData)engine.RenderingFrameData;

            foreach (var slice in Iterator)
            {
                var locToWorld = slice.Sibling<LocalToWorld>();

                var mesh = slice.Sibling<Mesh>();

                var material = slice.Sibling<Material>();

                // TODO: Sorting or layer add
                frameData.AddRendererDataToCamera(0, frameData.AddRendererData(new RendererData
                {
                    IsDynamic = true,
                    MaterialName = material.Name,
                    MaterialQueue = material.Queue,
                    MeshName = mesh.Name,
                    PreviousTransformMatrix = locToWorld.Value,
                    TransformMatrix = locToWorld.Value,
                }));
            }
        }
    }
}
