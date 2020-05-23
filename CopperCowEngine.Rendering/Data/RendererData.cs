using System;
using System.Numerics;

namespace CopperCowEngine.Rendering.Data
{
    public struct RendererData : IComparable<RendererData>
    {
        public bool IsDynamic;

        public Guid MaterialGuid;

        public uint MaterialQueue;

        public Guid MeshGuid;

        public Matrix4x4 PreviousTransformMatrix;

        public Matrix4x4 TransformMatrix;

        public int CompareTo(RendererData other)
        {
            var a = MaterialQueue.CompareTo(other.MaterialQueue);
            if (a != 0) 
            {
                return a;
            }
            var b = MaterialGuid.CompareTo(other.MaterialGuid);
            if (b != 0) 
            {
                return b;
            }
            var c = MeshGuid.CompareTo(other.MeshGuid);
            return c != 0 ? c : 0;
        }
    }
}
