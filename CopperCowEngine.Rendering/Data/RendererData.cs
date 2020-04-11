using System.Numerics;

namespace CopperCowEngine.Rendering.Data
{
    public struct RendererData
    {
        public bool IsDynamic;

        public string MaterialName;

        public int MaterialQueue;

        public string MeshName;

        public Matrix4x4 PreviousTransformMatrix;

        public Matrix4x4 TransformMatrix;
    }
}
