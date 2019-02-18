using EngineCore;
using SharpDX;

namespace Editor.AssetsEditor.Components
{
    class PreviewBehaviour : Component
    {
        //public bool DisableRotation;
        public float ScaleOffset = 1.0f;
        private float ScaleLimit = 0.00001f;
        public float Yaw = 0.0f;
        public float Pitch = 0.0f;

        public void Reset() {
            ScaleOffset = 1.0f;
            Yaw = 0.0f;
            Pitch = 0.0f;
        }

        public override void Update() {
            float deltaT = Engine.Instance.Time.DeltaTime;

            //if (!DisableRotation) {
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.A)) {
                Yaw -= MathUtil.Pi * deltaT;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.D)) {
                Yaw += MathUtil.Pi * deltaT;
            }

            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.W)) {
                Pitch += MathUtil.Pi * 0.5f * deltaT;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.S)) {
                Pitch -= MathUtil.Pi * 0.5f * deltaT;
            }
            //}

            ScaleOffset = ScaleOffset > ScaleLimit ? ScaleOffset : ScaleLimit;
            gameObject.transform.Scale = Vector3.One * ScaleOffset;
            gameObject.transform.Rotation = Quaternion.RotationYawPitchRoll(Yaw, Pitch, 0.0f);
        }
    }
}
