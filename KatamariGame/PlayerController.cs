using SharpDX;
using EngineCore;

namespace KatamariGame
{
    public class PlayerController : Component
    {
        private Vector3 Direction = Vector3.Zero;
        private Transform m_transform;
      
        public float Speed = 10f;
        public float RotSpeed = 0.03f;

        public override void Init() {
            m_transform = gameObject.transform;
        }

        private Transform m_visTransform;
        public void SetVisualTransform(Transform inTransform) {
            m_visTransform = inTransform;
        }
        public Transform GetVisualTransform() {
            return m_visTransform;
        }

        private float m_RotationAngle;
        public override void Update() {
            float deltaTime = Engine.Instance.Time.DeltaTime;

            m_RotationAngle = 0;
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.A)) {
                m_RotationAngle -= 1f;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.D)) {
                m_RotationAngle = 1f;
            }

            Vector3 axis = m_transform.TransformMatrix.Left;
            m_transform.WorldRotation *= Quaternion.RotationAxis(
                m_transform.TransformMatrix.Up, 
                MathUtil.RadiansToDegrees(m_RotationAngle) * RotSpeed * deltaTime
            );
            m_transform.WorldPosition += m_transform.TransformMatrix.Forward * Speed * deltaTime;
           
            m_visTransform.WorldRotation *= Quaternion.RotationAxis(axis, Speed * deltaTime);
        }
    }
}
