using System.Numerics;
using CopperCowEngine.Rendering.Geometry;

namespace CopperCowEngine.Rendering.Data
{
    public struct CameraData
    {
        public Vector3 Forward;

        public BoundingFrustum Frustum;

        public int Index;

        public Vector3 Position;

        public Matrix4x4 PreviousView;

        public Matrix4x4 PreviousViewProjection;

        public Matrix4x4 Projection;

        public Matrix4x4 View;

        public Matrix4x4 ViewProjection;
    }
}
