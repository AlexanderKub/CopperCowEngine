using SharpDX;

namespace CopperCowEngine.Rendering.Data
{
    public enum LightType
    {
        Directional, Point, Spot, Capsule,
    }

    public struct LightData
    {
        public Vector3 Color;

        public Vector3 Direction;

        public BoundingFrustum Frustum;

        public int Index;

        public float Intensity;

        public bool IsCastShadows;

        public Vector3 Position;

        public float Radius;

        public LightType Type;

        public Matrix ViewProjection;
    }
}
