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
                var index = frameData.AddRendererData(new RendererData
                {
                    IsDynamic = true,
                    MaterialGuid = material.AssetGuid,
                    MaterialQueue = material.Queue,
                    MeshGuid = mesh.AssetGuid,
                    PreviousTransformMatrix = locToWorld.PreviousValue,
                    TransformMatrix = locToWorld.Value,
                });
                
                frameData.AddRendererDataToCamera(0, index);
                if (frameData.LightsList.Count > 0)
                {
                    frameData.AddRendererDataToLight(0, index);
                }
            }

            frameData.Finish();
        }
    }
}
