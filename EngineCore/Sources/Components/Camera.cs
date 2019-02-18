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
                    gameObject.transform.Position, 
                    gameObject.transform.Position + Direction,
                    Vector3.Up
                );
                return m_View;
            }
        }
        private Matrix m_View;

        public Matrix Projection
        {
            get {
                m_Projection = Matrix.PerspectiveFovLH(FOV, AspectRatio, NearClippingPlane, FarClippingPlane);
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
                    2 * (gameObject.transform.Rotation.X * gameObject.transform.Rotation.Z + 
                    gameObject.transform.Rotation.W * gameObject.transform.Rotation.Y),
                    2 * (gameObject.transform.Rotation.Y * gameObject.transform.Rotation.Z - 
                    gameObject.transform.Rotation.W * gameObject.transform.Rotation.X),
                    1 - 2 * (gameObject.transform.Rotation.X * gameObject.transform.Rotation.X +
                    gameObject.transform.Rotation.Y * gameObject.transform.Rotation.Y)
                );
                //TODO: Solve problem this identity View Matrix
                return m_Direction;
            }
        }
        private Vector3 m_Direction;
        
    }
}
