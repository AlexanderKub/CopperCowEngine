using System.Numerics;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class LightsSystem : ComponentSystem<Required<Translation, DirectionalLight>, Optional<LightColor>>
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;

            var frameData = (StandardFrameData)engine.RenderingFrameData;

            foreach (var slice in Iterator)
            {
                var translation = slice.Sibling<Translation>();

                var directionalLight = slice.Sibling<DirectionalLight>();

                var color = slice.HasSibling<LightColor>() ? slice.Sibling<LightColor>().Value : Vector3.One;
                
                frameData.AddLightDataToCamera(0, frameData.AddLightData(new LightData
                {
                    Position = translation.Value,
                    Color = color,
                    Direction = directionalLight.Direction,
                    Intensity = directionalLight.Intensity,
                    Type = LightType.Directional,
                    Radius = 1000,
                }));
            }
        }
    }
}