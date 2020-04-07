using SharpDX;

namespace CopperCowEngine.Rendering.Data
{
    public struct CameraData
    {
        public Vector3 Forward;

        public BoundingFrustum Frustum;

        public int Index;

        public Vector3 Position;

        public Matrix PreviousView;

        public Matrix PreviousViewProjection;

        public Matrix Projection;

        public Matrix View;

        public Matrix ViewProjection;
    }
}
