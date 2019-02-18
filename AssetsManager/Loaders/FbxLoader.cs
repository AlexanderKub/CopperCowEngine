using SharpDX;
using FbxNative;
using FbxNativeLoader = FbxNative.FbxLoader;

namespace AssetsManager.Loaders
{
    class FbxLoader {
        private static FbxNativeLoader NativeLoader;
        public static ModelGeometry Load(string path) {
            if (NativeLoader == null) {
                NativeLoader = new FbxNativeLoader();
            }

            Scene scene = NativeLoader.LoadScene(path);
            Mesh mesh = scene.Meshes[scene.Meshes.Count - 1];

            int vertCount = mesh.VertexCount;
            Vector3[] verts = new Vector3[vertCount]; 
            Vector3[] norms = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            Vector4[] colrs = new Vector4[vertCount];

            int i;
            MeshVertex mv;
            for (i = 0; i < vertCount; i++) {
                mv = mesh.Vertices[i];
                verts[i] = mv.Position;
                norms[i] = mv.Normal;
                uvs[i] = mv.TexCoord0;
                colrs[i] = mv.Color0.ToVector4();
            }

            int[] indxs = new int[mesh.IndexCount];
            int j = 0;
            for (i = 0; i < mesh.TriangleCount; i++) {
                indxs[j] = mesh.Triangles[i].Index0;
                indxs[j + 1] = mesh.Triangles[i].Index1;
                indxs[j + 2] = mesh.Triangles[i].Index2;
                j += 3;
            }

            return new ModelGeometry(verts, colrs, uvs, indxs, null, norms);
        }
    }
}
