using AssetsManager.Loaders;
using SharpDX;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public struct VertexStruct
    {
        public Vector4 Position;
        public Vector4 Color;
        public Vector4 UV;
        public Vector4 Normal;
        public Vector4 Tangent;
        public Vector4 Binormal;
    };

    public class MeshAsset: BaseAsset
    {
        public VertexStruct[] Vertices;
        public int[] Indexes;
        public float FileScale;
        public Vector3 Pivot;

        public MeshAsset() {
            this.Type = AssetTypes.Mesh;
        }

        public override bool ImportAsset(string path, string ext) {
            ModelGeometry MG;
            if (ext == "obj") {
                MG = ObjLoader.Load(path);
            } else if (ext == "fbx") {
                MG = FbxLoader.Load(path);
            } else {
                Console.WriteLine("Unknown mesh extension: {0}", ext);
                return false;
            }

            FileScale = 1.0f;
            Pivot = Vector3.Zero;
            Vertices = MG.Points;
            Indexes = MG.Indexes;
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            int i;
            writer.Write(FileScale);
            writer.Write(SerializeBlock.GetBytes(Pivot));
            writer.Write(Vertices.Length);
            for (i = 0; i < Vertices.Length; i++) {
                byte[] bytes = SerializeBlock.GetBytes(Vertices[i]);
                writer.Write(bytes);
            }
            writer.Write(Indexes.Length);
            for (i = 0; i < Indexes.Length; i++) {
                writer.Write(Indexes[i]);
            }
        }

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
                return false;
            }
            int i, n;
            FileScale = reader.ReadSingle();
            Pivot = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            n = reader.ReadInt32();
            this.Vertices = new VertexStruct[n];
            for (i = 0; i < n; i++) {
                byte[] arr = reader.ReadBytes(96);
                this.Vertices[i] = SerializeBlock.FromBytes<VertexStruct>(arr);
            }
            n = reader.ReadInt32();
            this.Indexes = new int[n];
            for (i = 0; i < n; i++) {
                this.Indexes[i] = reader.ReadInt32();
            }
            return true;
        }
    }
}
