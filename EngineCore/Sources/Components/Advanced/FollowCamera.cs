using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EngineCore
{
    public class FollowCamera : Camera
    {
        private Transform m_Target;
        public Transform Target
        {
            get {
                return m_Target;
            }
            set {
                if (m_Target != value) {
                    m_Target = value;
                }
            }
        }
        public float Distance;

        //private Vector3 targetPos;

        public override void Update() {
            if(m_Target == null) {
                return;
            }

            Vector3 dir = m_Target.TransformMatrix.Backward + m_Target.TransformMatrix.Up * 1.25f;
            dir.Normalize();
            gameObject.transform.WorldPosition = m_Target.WorldPosition + dir * Distance;
            /*targetPos = m_Target.Position + dir * Distance;
            gameObject.transform.Position = Vector3.SmoothStep(
                gameObject.transform.Position, targetPos, 
                Engine.Instance.Time.DeltaTime * 10f
            );*/

            gameObject.transform.WorldRotation = Quaternion.LookAtLH(
                gameObject.transform.WorldPosition,
                m_Target.WorldPosition,
                gameObject.transform.TransformMatrix.Up
            );
        }
    }
}
