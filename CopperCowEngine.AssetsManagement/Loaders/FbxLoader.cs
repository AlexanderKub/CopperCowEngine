using CopperCowEngine.Rendering.Loaders;
using SharpDX;

namespace CopperCowEngine.AssetsManagement.Loaders
{
    internal class FbxLoader
    {
        private static FbxNative.FbxLoader _nativeLoader;

        public static ModelGeometry[] Load(string path)
        {
            if (_nativeLoader == null)
            {
                _nativeLoader = new FbxNative.FbxLoader();
            }

            var scene = _nativeLoader.LoadScene(path);
            var result = new ModelGeometry[scene.Meshes.Count];

            for (var k = 0; k < result.Length; k++)
            {
                var mesh = scene.Meshes[k];

                //TODO: scene children, mats and so on
                var vertexCount = mesh.VertexCount;
                var vertices = new Vector3[vertexCount];
                var normals = new Vector3[vertexCount];
                var uvs = new Vector2[vertexCount];
                var colors = new Vector4[vertexCount];

                int i;
                for (i = 0; i < vertexCount; i++)
                {
                    var meshVertex = mesh.Vertices[i];
                    vertices[i] = meshVertex.Position;
                    normals[i] = meshVertex.Normal;
                    uvs[i] = meshVertex.TexCoord0;
                    colors[i] = meshVertex.Color0.ToVector4();
                }

                var indices = new int[mesh.IndexCount];
                var j = 0;
                for (i = 0; i < mesh.TriangleCount; i++)
                {
                    indices[j] = mesh.Triangles[i].Index0;
                    indices[j + 1] = mesh.Triangles[i].Index1;
                    indices[j + 2] = mesh.Triangles[i].Index2;
                    j += 3;
                }
                result[k] = new ModelGeometry(vertices, colors, uvs, indices, null, normals);
            }

            return result;
        }
    }
}