using SharpDX;
using EngineCore;

namespace PongGame
{
    class ScorePoint : Component
    {
        public Vector3 TargetScale;
        private float LerpSpeed = 8f;
       
        public override void Update() {
            float deltaTime = Engine.Instance.Time.DeltaTime;
            float m_Time = Engine.Instance.Time.Time;
            gameObject.transform.Scale = 
                Vector3.Lerp(gameObject.transform.Scale, TargetScale, deltaTime * LerpSpeed);
        }

        public void Hide() {
            TargetScale = Vector3.Zero;
            gameObject.transform.Scale = TargetScale;
        }

        public void Show() {
            TargetScale = Vector3.One * 2f - Vector3.Up;
        }
    }
}
