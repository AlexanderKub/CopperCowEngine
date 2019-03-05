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

        #region Position
        public Vector3 LocalPosition {
            get {
                return m_LocalPosition;
            }
            set {
                if (value != m_LocalPosition) {
                    m_LocalPosition = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Vector3 m_LocalPosition = Vector3.Zero;

        public Vector3 WorldPosition {
            get {
                return TransformMatrix.TranslationVector;
            }
            set {
                Vector3 locPos = value;
                if (Parent) {
                    Matrix transformMatrix = Matrix.Translation(locPos) * 
                        Matrix.Invert(Parent.TransformMatrix);
                    locPos = transformMatrix.TranslationVector;
                }
                if (locPos != m_LocalPosition) {
                    m_LocalPosition = locPos;
                    m_NeedUpdate = true;
                }
            }
        }
        #endregion

        #region Scale
        public Vector3 LocalScale {
            get {
                return m_LocalScale;
            }
            set {
                if (value != m_LocalScale) {
                    m_LocalScale = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Vector3 m_LocalScale = Vector3.One;

        public Vector3 WorldScale {
            get {
                Vector3 locScale = m_LocalScale;
                if (Parent) {
                    locScale *= Parent.WorldScale;
                }
                return locScale;
            }
            set {
                Vector3 locScale = value;
                if (Parent) {
                    locScale = locScale / Parent.WorldScale;
                }

                if (locScale != m_LocalScale) {
                    m_LocalScale = locScale;
                    m_NeedUpdate = true;
                }
            }
        }
        #endregion

        #region Rotation
        public Quaternion LocalRotation
        {
            get {
                return m_LocalRotation;
            }
            set {
                if (value != m_LocalRotation) {
                    m_LocalRotation = value;
                    m_NeedUpdate = true;
                }
            }
        }
        private Quaternion m_LocalRotation = Quaternion.Identity;

        public Quaternion WorldRotation {
            get {
                TransformMatrix.Decompose(out Vector3 translate, 
                    out Quaternion rot, out Vector3 scale);
                return rot;
            }
            set {
                Quaternion locRot = value;
                if (Parent) {
                    Matrix WorldToLocal = Matrix.Invert(Parent.TransformMatrix);
                    WorldToLocal.Decompose(out Vector3 translate, 
                        out Quaternion rot, out Vector3 scale);
                    locRot = rot * locRot;
                }

                if (locRot != m_LocalRotation) {
                    m_LocalRotation = locRot;
                    m_NeedUpdate = true;
                }
            }
        }
        #endregion

        private bool m_NeedUpdate = true;
        public Matrix TransformMatrix { get; private set; }

        internal Transform() { }

        public override void Init() {
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
            LocalPosition = pos;
            LocalScale = scale;
            LocalRotation = rot;
        }

        public override void Update() {
            if (!IsNeedUpdate() && (Parent == null || !Parent.IsNeedUpdate())) {
                RequestChildsUpdate();
                return;
            }

            TransformMatrix = Matrix.Identity;

            if (Parent) {
                TransformMatrix *= Matrix.Scaling(m_LocalScale);
                TransformMatrix *= Matrix.RotationQuaternion(m_LocalRotation);
                TransformMatrix *= Matrix.Translation(m_LocalPosition);

                TransformMatrix *= Parent.TransformMatrix;
            } else {
                TransformMatrix *= Matrix.Scaling(m_LocalScale);
                TransformMatrix *= Matrix.RotationQuaternion(m_LocalRotation);
                TransformMatrix *= Matrix.Translation(m_LocalPosition);
            }

            RequestChildsUpdate();
            m_NeedUpdate = false;
        }

        private void RequestChildsUpdate() {
            if (Childs != null) {
                foreach (Transform eachChild in Childs) {
                    eachChild.gameObject.Update();
                }
            }
        }

        public override void Draw() { }
        public override void Destroy() { }

        public static implicit operator bool(Transform foo)
        {
            return !object.ReferenceEquals(foo, null);
        }
    }
}