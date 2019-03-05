using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EngineCore
{
    public class Camera : Component
    {
        //TODO: Add posible to change for each camera
        private const float NearClippingPlane = 0.001f;
        private const float FarClippingPlane = 1000f;
        
        // Note, we are using inverted 32-bit float depth for better precision, 
        // so reverse near and far below
        static public bool IsInvertedDepthBuffer = false;

        public bool IsMain;

        public float FOV
        {
            get {
                return m_FOV;
            }
            set {
                if (value != m_FOV) {
                    m_FOV = value;
                }
            }
        }
        private float m_FOV;

        public float AspectRatio
        {
            get {
                return m_AspectRatio;
            }
            set {
                if (value != m_AspectRatio) {
                    m_AspectRatio = value;
                }
            }
        }
        private float m_AspectRatio;
        
        public Matrix ViewProjectionMatrix
        {
            get {
                m_ViewProjectionMatrix = View * Projection;
                return m_ViewProjectionMatrix;
            }
        }
        private Matrix m_ViewProjectionMatrix;

        public Matrix View
        {
            get {
                m_View = Matrix.LookAtLH(
                    gameObject.transform.WorldPosition, 
                    gameObject.transform.WorldPosition + Direction,
                    Vector3.Up
                );
                return m_View;
            }
        }
        private Matrix m_View;

        public Matrix Projection
        {
            get {
                if (IsInvertedDepthBuffer) {
                    m_Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, FarClippingPlane, NearClippingPlane);
                } else{
                    m_Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, NearClippingPlane, FarClippingPlane);
                }
                return m_Projection;
            }
            set {
                if(value != m_Projection) {
                    m_Projection = value;
                }
            }
        }
        private Matrix m_Projection;

        public Camera()
        {
            FOV = MathUtil.PiOverTwo;
            AspectRatio = Engine.Instance.DisplayRef.AspectRatio;
        }

        public Vector3 Direction
        {
            get {
                m_Direction = new Vector3(
                    2 * (gameObject.transform.WorldRotation.X * gameObject.transform.WorldRotation.Z + 
                    gameObject.transform.WorldRotation.W * gameObject.transform.WorldRotation.Y),
                    2 * (gameObject.transform.WorldRotation.Y * gameObject.transform.WorldRotation.Z - 
                    gameObject.transform.WorldRotation.W * gameObject.transform.WorldRotation.X),
                    1 - 2 * (gameObject.transform.WorldRotation.X * gameObject.transform.WorldRotation.X +
                    gameObject.transform.WorldRotation.Y * gameObject.transform.WorldRotation.Y)
                );
                //TODO: Solve problem this identity View Matrix
                return m_Direction;
            }
        }
        private Vector3 m_Direction;

        public static implicit operator bool(Camera foo)
        {
            return !object.ReferenceEquals(foo, null);
        }
    }
}
