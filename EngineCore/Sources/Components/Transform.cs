using System.Drawing;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore
{
    public class Transform : Component {
        public Transform Parent
        {
            get {
                return m_Parent;
            }
            set {
                if (value != m_Parent) {
                    m_Parent = value;
                    if (m_Parent.Childs == null) {
                        m_Parent.Childs = new List<Transform>();
                    }
                    m_Parent.Childs.Add(this);
                    m_NeedUpdate = true;
                }
            }
        }
        private Transform m_Parent;
        public List<Transform> Childs;

        public Vector3 Position
        {
            get {
                return m_Position;
            }
            set {
                if (value != m_Position) {
                    m_Position = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Vector3 m_Position;

        public Vector3 Scale
        {
            get {
                return m_Scale;
            }
            set {
                if (value != m_Scale) {
                    m_Scale = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Vector3 m_Scale;

        public Quaternion Rotation
        {
            get {
                return m_Rotation;
            }
            set {
                if (value != m_Rotation) {
                    m_Rotation = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Quaternion m_Rotation;

        private Matrix m_TransformMatrix;
        public Matrix TransformMatrix
        {
            get {
                return m_TransformMatrix;
            }
        }

        private bool m_NeedUpdate;

        public override void Init() {
            m_NeedUpdate = true;
            if (Scale == Vector3.Zero) {
                Scale = Vector3.One;
            }
            Update();
        }

        public bool IsNeedUpdate() {
            return m_NeedUpdate;
        }

        public void SetParent(Transform newParent) {
            SetParent(newParent, false);
        }

        public void SetParent(Transform newParent, bool stayWorld) {
            Parent = newParent;

            if (stayWorld) {
                return;
            }

            Matrix WorldToLocal = Matrix.Invert(Parent.TransformMatrix);
            WorldToLocal = TransformMatrix * WorldToLocal;

            Vector3 scale;
            Vector3 pos;
            Quaternion rot;

            WorldToLocal.Decompose(out scale, out rot, out pos);
            Position = pos;
            Scale = scale;
            Rotation = rot;
        }

        public override void Update() {
            if (!IsNeedUpdate() && (Parent == null || !Parent.IsNeedUpdate())) {
                return;
            }

            m_TransformMatrix = Matrix.Identity;

            if (Parent != null) {
                m_TransformMatrix *= Matrix.Scaling(m_Scale);
                m_TransformMatrix *= Matrix.RotationQuaternion(m_Rotation);
                m_TransformMatrix *= Matrix.Translation(m_Position);

                m_TransformMatrix *= Parent.TransformMatrix;
            } else {
                m_TransformMatrix *= Matrix.Scaling(m_Scale);
                m_TransformMatrix *= Matrix.RotationQuaternion(m_Rotation);
                m_TransformMatrix *= Matrix.Translation(m_Position);
            }


            if (Childs != null) {
                foreach (Transform eachChild in Childs) {
                    eachChild.gameObject.Update();
                }
            }

            m_NeedUpdate = false;
        }

        public override void Draw() {

        }

        public override void Destroy() {
            
        }
    }
}
