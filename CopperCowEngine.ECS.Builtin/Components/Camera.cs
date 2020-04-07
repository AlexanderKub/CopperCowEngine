using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CopperCowEngine.ECS.Builtin.Components
{
    public struct CameraSetup : IComponentData
    {
        public float NearClippingPlane;

        public float FarClippingPlane;

        public float Fov;

        public float AspectRatio;

        public static CameraSetup Default = new CameraSetup
        {
            NearClippingPlane = 0.001f,
            FarClippingPlane = 10000f,
            Fov = MathUtil.PiOverTwo,
            AspectRatio = 1,
        };
    }

    public struct CameraProjection : IComponentData
    {
        public Matrix Value;
    }

    public struct CameraViewProjection : IComponentData
    {
        public Matrix View;

        public Matrix ViewProjection;
    }
}
