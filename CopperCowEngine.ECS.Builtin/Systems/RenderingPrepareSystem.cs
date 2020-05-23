using CopperCowEngine.ECS.Builtin.Singletons;
using CopperCowEngine.Rendering.Data;

namespace CopperCowEngine.ECS.Builtin.Systems
{
    public class RenderingPrepareSystem : ComponentlessSystem
    {
        protected override void Update()
        {
            var engine = Context.GetSingletonComponent<EngineHolder>().Engine;
            var frameData = (StandardFrameData)engine.RenderingFrameData;
            var frame2DData = (Standard2DFrameData)engine.Rendering2DFrameData;
            frameData.Reset();
            frame2DData.Reset();
        }
    }
}