using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Components
{
    public class Camera : IEntityComponent
    {
        public float NearClippingPlane = 0.001f;
        public float FarClippingPlane = 10000f;
        public float FOV = MathUtil.PiOverTwo;
        public float AspectRatio = 1;

        public Matrix View;
        public Matrix Projection;
        public Matrix ViewProjection;
        public Matrix PreviousView;
        public Matrix PreviousViewProjection;

        public float Yaw = 0f;
        public float Pitch = 0f;

        public bool IsScreenView = true;
    }
}
