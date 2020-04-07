using CopperCowEngine.ECS.Builtin.Components;

namespace CopperCowEngine.ECS.Builtin.Singletons
{
    public struct FrameDataHolder : ISingletonComponentData
    {
        public CameraSetup MainCamera;
    }
}
