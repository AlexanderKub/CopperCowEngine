using System;
using System.Numerics;
using CopperCowEngine.Rendering.Geometry;

namespace CopperCowEngine.Rendering.Loaders
{
    public struct BoundsBox
    {
        public Vector3 Center => (Maximum + Minimum) * 0.5f;

        public Vector3 Extent => (Maximum - Minimum) * 0.5f;

        public Vector3 Minimum => BoundingBox.Min;

        public Vector3 Maximum => BoundingBox.Max;

        public Vector3 Size => Extent * 2.0f;

        internal BoundingBox BoundingBox;

        public BoundsBox(Vector3 minimum, Vector3 maximum)
        {
            BoundingBox = new BoundingBox(minimum, maximum);
        }

        /*internal static BoundsBox TransformAABBFast(BoundsBox bounds, Matrix4x4 matrix)
        {
            var newCenter = Vector3.Transform(bounds.Center, matrix);

            var newExtent = Vector3.TransformNormal(bounds.Extent, AbsMatrix(matrix));

            return new BoundsBox(newCenter - newExtent, newCenter + newExtent);
        }

        private static Matrix4x4 AbsMatrix(Matrix4x4 matrix)
        {
            var output = new Matrix4x4();
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    output[i, j] = Math.Abs(matrix[i, j]);
                }
            }
            return output;
        }*/
    }
}