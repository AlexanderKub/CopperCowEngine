using System;
using SharpDX;

namespace EngineCore
{
    public class CBoundingSphere : Component
    {
        public Vector3 Center;
        public float Radius;
        public Action<CBoundingSphere, GameObject> OnContact;

        public BoundingSphere BS;

        private Transform m_transfrorm;
        public override void Init() {
            m_transfrorm = gameObject.transform;
            BS = new BoundingSphere(Center, Radius);
            Engine.Instance.m_BoundingScene.BS_List.Add(this);
        }

        public bool HasContact(CBoundingSphere other) {
            float distance = Vector3.Distance(
                m_transfrorm.Position + Center,
                other.gameObject.transform.Position + other.Center
            );
            return distance <= Radius + other.Radius;
        }

        public void OnContactEvent(CBoundingSphere other) {
            if(OnContact == null) {
                return;
            }
            OnContact(other, gameObject);
        }

        public override void Destroy() {
            Engine.Instance.m_BoundingScene.BS_List.Remove(this);
        }
    }
}
