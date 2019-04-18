using SharpDX;
using EngineCore;

namespace KatamariGame
{
    public class PlayerController : BehaviourComponent
    {
        private Vector3 Direction = Vector3.Zero;
        private DeprecatedTransform m_transform;
      
        public float Speed = 10f;
        public float RotSpeed = 0.03f;

        public override void OnInit() {
            m_transform = transform;
        }

        private DeprecatedTransform m_visTransform;
        public void SetVisualTransform(DeprecatedTransform inTransform) {
            m_visTransform = inTransform;
        }
        public DeprecatedTransform GetVisualTransform() {
            return m_visTransform;
        }

        private float m_RotationAngle;
        public override void OnUpdate() {
            float deltaTime = Engine.Instance.Time.DeltaTime;

            m_RotationAngle = 0;
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.A)) {
                m_RotationAngle -= 1f;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.D)) {
                m_RotationAngle = 1f;
            }

            Vector3 axis = m_transform.TransformMatrix.Left;
            m_transform.Rotation *= Quaternion.RotationAxis(
                m_transform.TransformMatrix.Up, 
                MathUtil.RadiansToDegrees(m_RotationAngle) * RotSpeed * deltaTime
            );
            m_transform.Position += m_transform.TransformMatrix.Forward * Speed * deltaTime;
           
            m_visTransform.Rotation *= Quaternion.RotationAxis(axis, Speed * deltaTime);
        }
    }
}
