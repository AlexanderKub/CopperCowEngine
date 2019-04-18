using AssetsManager.AssetsMeta;
using SharpDX;

namespace AssetsManager.Loaders
{
    public class ModelGeometry
    {
        public int[] Indexes;
        public int[] IndexesWithAdj;
        public Vector3[] Verticies;
        public Vector4[] Colors;
        public Vector2[] UVs;
        public Vector3[] Normals;
        public Vector3 Pivot;
        public float FileScale = 1.0f;

        private Vector3 m_BoundingMinimum;
        private Vector3 m_BoundingMaximum;
        public Vector3 BoundingMinimum {
            get {
                return (m_BoundingMinimum - Pivot) * FileScale;
            } 
        }
        public Vector3 BoundingMaximum {
            get {
                return (m_BoundingMaximum - Pivot) * FileScale;
            }
        }

        public struct PositionsColorsStruct
        {
            public Vector4 Pos;
            public Vector4 Color;
        };

        private VertexStruct[] m_Points;
        public VertexStruct[] Points
        {
            get {
                if (m_Points == null) {
                    return null;
                }
                VertexStruct[] tmp = new VertexStruct[m_Points.Length];
                for (int i = 0; i < tmp.Length; i++) {
                    tmp[i] = m_Points[i];
                    tmp[i].Position -= new Vector4(Pivot, 0);
                    tmp[i].Position *= FileScale;
                }
                return tmp;
            }
        }

        public PositionsColorsStruct[] SVPoints { get; private set; }

        public int Count
        {
            get {
                return Indexes.Length;
            }
        }

        public ModelGeometry(float fileScale, Vector3 pivot, VertexStruct[] verts, int[] indxs, Vector3 boundMin, Vector3 boundMax) {
            FileScale = fileScale;
            Pivot = pivot;
            m_Points = verts;
            Indexes = indxs;
            m_BoundingMinimum = boundMin;
            m_BoundingMaximum = boundMax;
        }

        public ModelGeometry(Vector3[] vrtxs, Vector4[] colors, Vector2[] uvs, int[] indxs, 
            int[] indxsWithAdj, Vector3[] normals) {

            InternalInit(vrtxs, colors, uvs, indxs, IndexesWithAdj, normals);
        }

        private void InternalInit(Vector3[] vrtxs, Vector4[] colors, Vector2[] uvs, int[] indxs,
            int[] indxsWithAdj, Vector3[] normals) {

            Vector3 normal;
            Verticies = vrtxs;
            Indexes = indxs;
            IndexesWithAdj = indxsWithAdj;
            Colors = colors;
            UVs = uvs;
            Normals = normals;

            int n = vrtxs.Length;
            m_Points = new VertexStruct[n];
            SVPoints = new PositionsColorsStruct[n];
            
            m_BoundingMinimum = Vector3.One * float.MaxValue;
            m_BoundingMaximum = Vector3.One * float.MinValue;

            for (int i = 0; i < n; i++) {
                normal = (normals == null ? Vector3.Zero : normals[i]);
                CalculateTangentBinormal(normal, out Vector3 tangent, out Vector3 binormal);
                m_Points[i] = new VertexStruct() {
                    Position = new Vector4(vrtxs[i], 1),
                    Color = (colors == null ? Vector4.One : colors[i]),
                    UV0 = new Vector4(uvs == null ? Vector2.Zero : uvs[i], 0, 0),
                    UV1 = Vector4.Zero,
                    Normal = new Vector4(normal, 0),
                    Tangent = new Vector4(tangent, 0),
                };

                m_BoundingMinimum = Vector3.Min(m_BoundingMinimum, vrtxs[i]);
                m_BoundingMaximum = Vector3.Max(m_BoundingMaximum, vrtxs[i]);

                SVPoints[i] = new PositionsColorsStruct() {
                    Pos = m_Points[i].Position,
                    Color = m_Points[i].Color,
                };
            }
        }

        private void CalculateTangentBinormal(Vector3 Normal, out Vector3 Tangent, out Vector3 Binormal) {
            Vector3 crossOne = Vector3.Cross(Normal, Vector3.ForwardLH);
            Vector3 crossTwo = Vector3.Cross(Normal, Vector3.Up);

            if (crossOne.Length() > crossTwo.Length()) {
                Tangent = crossOne;
            } else {
                Tangent = crossTwo;
            }
            Tangent.Normalize();

            Binormal = Vector3.Cross(Normal, Tangent);
            Binormal.Normalize();
        }

        private static Vector3 ToVector3(Vector4 inVector) {
            return new Vector3(inVector.X, inVector.Y, inVector.Z);
        }
    }
}
