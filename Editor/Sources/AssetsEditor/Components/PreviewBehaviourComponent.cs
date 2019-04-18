using EngineCore.ECS;

namespace Editor.AssetsEditor.Components
{
    internal class PreviewBehaviourComponent : IEntityComponent
    {
        public float ScaleOffset = 1.0f;
        public float Yaw;
        public float Pitch;
    }
}
