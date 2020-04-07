using System;
using SharpDX;

namespace CopperCowEngine.Rendering.Loaders
{
    public struct BoundsBox
    {
        public Vector3 Center => (Maximum + Minimum) * 0.5f;

        public Vector3 Extent => (Maximum - Minimum) * 0.5f;

        public Vector3 Minimum => BoundingBox.Minimum;

        public Vector3 Maximum => BoundingBox.Maximum;

        public Vector3 Size => Extent * 2.0f;

        internal BoundingBox BoundingBox;

        public BoundsBox(Vector3 minimum, Vector3 maximum)
        {
            BoundingBox = new BoundingBox(minimum, maximum);
        }

        internal static BoundsBox TransformAABBFast(BoundsBox bounds, Matrix matrix)
        {
            var newCenter = Vector3.TransformCoordinate(bounds.Center, matrix);
            var newExtent = Vector3.TransformNormal(bounds.Extent, AbsMatrix(matrix));
            return new BoundsBox(newCenter - newExtent, newCenter + newExtent);
        }

        private static Matrix AbsMatrix(Matrix matrix)
        {
            var output = new Matrix();
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    output[i, j] = Math.Abs(matrix[i, j]);
                }
            }
            return output;
        }
    }
}