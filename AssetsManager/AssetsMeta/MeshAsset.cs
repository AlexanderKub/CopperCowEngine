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
        public Vector4 UV0;
        public Vector4 UV1;
        public Vector4 Normal;
        public Vector4 Tangent;
    };

    public class MeshAsset: BaseAsset
    {
        public VertexStruct[] Vertices;
        public int[] Indexes;
        public float FileScale = 1.0f;
        public Vector3 Pivot;
        public Vector3 BoundingMinimum;
        public Vector3 BoundingMaximum;

        public MeshAsset() {
            this.Type = AssetTypes.Mesh;
        }

        public bool ImportAsset(string path, string ext, float fileScale)
        {
            FileScale = fileScale;
            return ImportAsset(path, ext);
        }

        public override bool ImportAsset(string path, string ext) {
            ModelGeometry[] MG = new ModelGeometry[1];

            if (ext == "obj") {
                MG[0] = ObjLoader.Load(path);
            } else if (ext == "fbx") {
                MG = FbxLoader.Load(path);
                if (MG.Length > 1)
                {
                    subAssets = new MeshAsset[MG.Length - 1];
                    for (int i = 1; i < MG.Length; i++)
                    {
                        subAssets[i - 1] = new MeshAsset()
                        {
                            Name = this.Name + "_" + i,
                            FileScale = FileScale,
                            Pivot = Vector3.Zero,
                            Vertices = MG[i].Points,
                            Indexes = MG[i].Indexes,
                            BoundingMinimum = MG[i].BoundingMinimum,
                            BoundingMaximum = MG[i].BoundingMaximum,
                        };
                    }
                }
            } else {
                Console.WriteLine("Unknown mesh extension: {0}", ext);
                return false;
            }

            Pivot = Vector3.Zero;
            Vertices = MG[0].Points;
            Indexes = MG[0].Indexes;
            BoundingMinimum = MG[0].BoundingMinimum;
            BoundingMaximum = MG[0].BoundingMaximum;
            return true;
        }

        private MeshAsset[] subAssets;
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
            writer.Write(SerializeBlock.GetBytes(BoundingMinimum));
            writer.Write(SerializeBlock.GetBytes(BoundingMaximum));
            if (subAssets == null)
            {
                return;
            }
            foreach (MeshAsset subAsset in subAssets)
            {
                AssetsManagerInstance.GetManager().FSWorker.CreateAssetFile(subAsset, true);
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
            BoundingMinimum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            BoundingMaximum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            return true;
        }
    }
}
