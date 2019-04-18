using SharpDX;
using EngineCore;

namespace PongGame
{
    class ScorePoint : BehaviourComponent
    {
        public Vector3 TargetScale;
        private float LerpSpeed = 8f;
       
        public override void OnUpdate() {
            float deltaTime = Engine.Instance.Time.DeltaTime;
            float m_Time = Engine.Instance.Time.Time;
            transform.Scale = 
                Vector3.Lerp(transform.Scale, TargetScale, deltaTime * LerpSpeed);
        }

        public void Hide() {
            TargetScale = Vector3.Zero;
            transform.Scale = TargetScale;
        }

        public void Show() {
            TargetScale = Vector3.One * 2f - Vector3.Up;
        }
    }
}
