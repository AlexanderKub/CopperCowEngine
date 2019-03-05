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

        public struct PositionsColorsStruct
        {
            public Vector4 Pos;
            public Vector4 Color;
        };

        private VertexStruct[] m_Points;
        public VertexStruct[] Points
        {
            get {
                VertexStruct[] tmp = new VertexStruct[m_Points.Length];
                for (int i = 0; i < tmp.Length; i++) {
                    tmp[i] = m_Points[i];
                    tmp[i].Position -= new Vector4(Pivot, 0);
                    tmp[i].Position *= FileScale;
                }
                return tmp;
            }
        }

        private PositionsColorsStruct[] m_SV_Points;
        public PositionsColorsStruct[] SVPoints
        {
            get {
                return m_SV_Points;
            }
        }

        public int Count
        {
            get {
                return Indexes.Length;
            }
        }

        public ModelGeometry(float fileScale, Vector3 pivot, VertexStruct[] verts, int[] indxs) {
            FileScale = fileScale;
            Pivot = pivot;
            m_Points = verts;
            Indexes = indxs;
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
            if (n > 10000)
            {
                n = vrtxs.Length;
            }
            m_Points = new VertexStruct[n];
            m_SV_Points = new PositionsColorsStruct[n];
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
                m_SV_Points[i] = new PositionsColorsStruct() {
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
