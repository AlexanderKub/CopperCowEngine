using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore
{
    public struct BoundsBox
    {
        public Vector3 Center {
            get {
                return (Maximum + Minimum) * 0.5f;
            }
        }

        public Vector3 Extent {
            get {
                return (Maximum - Minimum) * 0.5f;
            }
        }

        public Vector3 Minimum {
            get {
                return boundingBox.Minimum;
            }
        }

        public Vector3 Maximum {
            get {
                return boundingBox.Maximum;
            }
        }

        public Vector3 Size {
            get {
                return Extent * 2.0f;
            }
        }

        internal BoundingBox boundingBox;

        public BoundsBox(Vector3 Minimum, Vector3 Maximum) {
            boundingBox = new BoundingBox(Minimum, Maximum);
        }

        internal static BoundsBox TransformAABBFast(BoundsBox bounds,Matrix matrix)
        {
            Vector3 newCenter = Vector3.TransformCoordinate(bounds.Center, matrix);
            Vector3 newExtent = Vector3.TransformNormal(bounds.Extent, AbsMatrix(matrix));
            return new BoundsBox(newCenter - newExtent, newCenter + newExtent);
        }

        private static Matrix AbsMatrix(Matrix matrix)
        {
            Matrix output = new Matrix();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    output[i, j] = Math.Abs(matrix[i, j]);
                }
            }
            return output;
        }
    }
}
