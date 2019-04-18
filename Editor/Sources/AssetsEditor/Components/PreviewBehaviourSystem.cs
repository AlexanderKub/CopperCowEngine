using EngineCore;
using EngineCore.ECS;
using EngineCore.ECS.Components;
using SharpDX;
using System;

namespace Editor.AssetsEditor.Components
{
    internal class PreviewBehaviourSystem : BasicSystem<Requires<Transform, PreviewBehaviourComponent>>
    {
        private float ScaleLimit = 0.00001f;

        public override void Update(Timer timer)
        {
            Entity[] entities = GetEntities();
            Transform transform;
            PreviewBehaviourComponent behavior;
            foreach (var entity in entities) {
                transform = entity.GetComponent<Transform>();
                behavior = entity.GetComponent<PreviewBehaviourComponent>();

                behavior.ScaleOffset = Math.Max(behavior.ScaleOffset, ScaleLimit);

                transform.Scale = Vector3.One * behavior.ScaleOffset;
                transform.Rotation = Quaternion.RotationYawPitchRoll(behavior.Yaw, behavior.Pitch, 0);
            }
            base.Update(timer);
        }
    }
}
