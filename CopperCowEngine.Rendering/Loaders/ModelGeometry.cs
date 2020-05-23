using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

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
            public Vector3 Pos;
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
                    tmp[i].Position -= Pivot;
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
            ComputeTangents(vertices, normals, uvs, indices, out var tangents, out _);
            for (var i = 0; i < n; i++)
            {
                var normal = normals?[i] ?? Vector3.Zero;
                //CalculateTangentBinormal(normal, out var tangent, out _);
                _points[i] = new VertexStruct()
                {
                    Position = vertices[i],
                    Color = colors?[i] ?? Vector4.One,
                    Uv0 = uvs?[i] ?? Vector2.Zero,
                    Uv1 = Vector4.Zero,
                    Normal = normal,
                    Tangent = tangents[i],
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

        /*private static void CalculateTangentBinormal(Vector3 normal, out Vector3 tangent, out Vector3 binormal)
        {
            var crossOne = Vector3.Cross(normal, Vector3.UnitZ);
            var crossTwo = Vector3.Cross(normal, Vector3.UnitY);

            tangent = crossOne.Length() > crossTwo.Length() ? crossOne : crossTwo;
            tangent = Vector3.Normalize(tangent);

            binormal = Vector3.Cross(normal, tangent);
            binormal = Vector3.Normalize(binormal);
        }*/

        private static void ComputeTangents(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector3> normals, 
            IReadOnlyList<Vector2> textureCoordinates, IReadOnlyList<int> triangleIndices,
            out Vector3[] tangents, out Vector3[] biTangents)
        {
            var length = positions.Count;
            var tan1 = new Vector3[length];
            for (var t = 0; t < triangleIndices.Count; t += 3)
            {
                var i1 = triangleIndices[t];
                var i2 = triangleIndices[t + 1];
                var i3 = triangleIndices[t + 2];
                var v1 = positions[i1];
                var v2 = positions[i2];
                var v3 = positions[i3];
                var w1 = textureCoordinates[i1];
                var w2 = textureCoordinates[i2];
                var w3 = textureCoordinates[i3];
                var x1 = v2.X - v1.X;
                var x2 = v3.X - v1.X;
                var y1 = v2.Y - v1.Y;
                var y2 = v3.Y - v1.Y;
                var z1 = v2.Z - v1.Z;
                var z2 = v3.Z - v1.Z;
                var s1 = w2.X - w1.X;
                var s2 = w3.X - w1.X;
                var t1 = w2.Y - w1.Y;
                var t2 = w3.Y - w1.Y;
                var r = 1.0f / (s1 * t2 - s2 * t1);
                var uDir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                tan1[i1] += uDir;
                tan1[i2] += uDir;
                tan1[i3] += uDir;
            }

            tangents = new Vector3[length];
            biTangents = new Vector3[length];
            for (var i = 0; i < length; i++)
            {
                var n = normals[i];
                var t = tan1[i];
                t = (t - n * Vector3.Dot(n, t));
                t = Vector3.Normalize(t);
                var b = Vector3.Cross(n, t);
                tangents[i] = t;
                biTangents[i] = b;
            }
        }
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexStruct
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector2 Uv0;
        public Vector4 Uv1;
        public Vector3 Normal;
        public Vector3 Tangent;
    };
}
