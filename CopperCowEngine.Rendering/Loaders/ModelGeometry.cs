using System.Numerics;

namespace CopperCowEngine.Rendering.Loaders
{
    public class ModelGeometry
    {
        public int[] Indexes;
        public int[] IndexesWithAdj;
        public Vector3[] Vertices;
        public Vector4[] Colors;
        public Vector2[] UVs;
        public Vector3[] Normals;
        public Vector3 Pivot;
        public float FileScale = 1.0f;

        private Vector3 _boundingMinimum;

        private Vector3 _boundingMaximum;

        public Vector3 BoundingMinimum => (_boundingMinimum - Pivot) * FileScale;

        public Vector3 BoundingMaximum => (_boundingMaximum - Pivot) * FileScale;

        public struct PositionsColorsStruct
        {
            public Vector4 Pos;
            public Vector4 Color;
        };

        private VertexStruct[] _points;

        public VertexStruct[] Points
        {
            get
            {
                if (_points == null)
                {
                    return null;
                }
                var tmp = new VertexStruct[_points.Length];
                for (var i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = _points[i];
                    tmp[i].Position -= new Vector4(Pivot, 0);
                    tmp[i].Position *= FileScale;
                }
                return tmp;
            }
        }

        public PositionsColorsStruct[] SvPoints { get; private set; }

        public int Count => Indexes.Length;

        public ModelGeometry(float fileScale, Vector3 pivot, VertexStruct[] vertices, int[] indices, Vector3 boundMin, Vector3 boundMax)
        {
            FileScale = fileScale;
            Pivot = pivot;
            _points = vertices;
            Indexes = indices;
            _boundingMinimum = boundMin;
            _boundingMaximum = boundMax;
        }

        public ModelGeometry(Vector3[] vertices, Vector4[] colors, Vector2[] uvs, int[] indices,
            int[] indicesWithAdj, Vector3[] normals)
        {

            InternalInit(vertices, colors, uvs, indices, IndexesWithAdj, normals);
        }

        private void InternalInit(Vector3[] vertices, Vector4[] colors, Vector2[] uvs, int[] indices,
            int[] indicesWithAdj, Vector3[] normals)
        {
            Vertices = vertices;
            Indexes = indices;
            IndexesWithAdj = indicesWithAdj;
            Colors = colors;
            UVs = uvs;
            Normals = normals;

            var n = vertices.Length;
            _points = new VertexStruct[n];
            SvPoints = new PositionsColorsStruct[n];

            _boundingMinimum = Vector3.One * float.MaxValue;
            _boundingMaximum = Vector3.One * float.MinValue;

            for (var i = 0; i < n; i++)
            {
                var normal = normals?[i] ?? Vector3.Zero;
                CalculateTangentBinormal(normal, out var tangent, out _);
                _points[i] = new VertexStruct()
                {
                    Position = new Vector4(vertices[i], 1),
                    Color = colors?[i] ?? Vector4.One,
                    Uv0 = new Vector4(uvs?[i] ?? Vector2.Zero, 0, 0),
                    Uv1 = Vector4.Zero,
                    Normal = new Vector4(normal, 0),
                    Tangent = new Vector4(tangent, 0),
                };

                _boundingMinimum = Vector3.Min(_boundingMinimum, vertices[i]);
                _boundingMaximum = Vector3.Max(_boundingMaximum, vertices[i]);

                SvPoints[i] = new PositionsColorsStruct()
                {
                    Pos = _points[i].Position,
                    Color = _points[i].Color,
                };
            }
        }

        private static void CalculateTangentBinormal(Vector3 normal, out Vector3 tangent, out Vector3 binormal)
        {
            var crossOne = Vector3.Cross(normal, Vector3.UnitZ);
            var crossTwo = Vector3.Cross(normal, Vector3.UnitY);

            tangent = crossOne.Length() > crossTwo.Length() ? crossOne : crossTwo;
            tangent = Vector3.Normalize(tangent);

            binormal = Vector3.Cross(normal, tangent);
            binormal = Vector3.Normalize(binormal);
        }

        private static Vector3 ToVector3(Vector4 inVector)
        {
            return new Vector3(inVector.X, inVector.Y, inVector.Z);
        }
    }

    public struct VertexStruct
    {
        public Vector4 Position;
        public Vector4 Color;
        public Vector4 Uv0;
        public Vector4 Uv1;
        public Vector4 Normal;
        public Vector4 Tangent;
    };
}
