using SharpDX;

namespace CopperCowEngine.Rendering.Data
{
    public struct RendererData
    {
        public bool IsDynamic;

        public string MaterialName;

        public int MaterialQueue;

        public string MeshName;

        public Matrix PreviousTransformMatrix;

        public Matrix TransformMatrix;
    }
}
