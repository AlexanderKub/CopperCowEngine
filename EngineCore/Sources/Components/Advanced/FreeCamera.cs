using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EngineCore
{
    public class FreeCamera : Camera
    {
        public float Speed = 1f;

        public override void Init() {
            Engine.Instance.Input.MouseMove += OnMouseMove;
        }

        private float Pitch = 0f;
        private float Yaw = 0;
        private void OnMouseMove(InputDevice.MouseMoveEventArgs args) {
            Yaw += Engine.Instance.Input.MouseOffset.X * 10f * Engine.Instance.Time.DeltaTime;
            Pitch += Engine.Instance.Input.MouseOffset.Y * 10f * Engine.Instance.Time.DeltaTime;

            //70
            if (Pitch >= 90f) {
                Pitch = 89.0001f;
            }

            //-60
            if (Pitch <= -90f) {
                Pitch = -89.0001f;
            }
        }

        public override void Update() {
            gameObject.transform.Rotation = 
                Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(Yaw), MathUtil.DegreesToRadians(Pitch), 0f);

            Vector3 posOffset = Vector3.Zero;
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.W)) {
                posOffset += Vector3.ForwardLH;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.S)) {
                posOffset -= Vector3.ForwardLH;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.D)) {
                posOffset += Vector3.Right;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.A)) {
                posOffset -= Vector3.Right;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.Space))
            {
                posOffset += Vector3.Up;
            }
            if (Engine.Instance.Input.IsKeyDown(System.Windows.Forms.Keys.C))
            {
                posOffset -= Vector3.Up;
            }

            //Backward cause LeftHanded space
            gameObject.transform.Position += (Matrix.RotationQuaternion(gameObject.transform.Rotation).Right * posOffset.X +
                Matrix.RotationQuaternion(gameObject.transform.Rotation).Backward * posOffset.Z +
                Matrix.RotationQuaternion(gameObject.transform.Rotation).Up * posOffset.Y) *
                10f * Speed * Engine.Instance.Time.DeltaTime;
        }
    }
}
