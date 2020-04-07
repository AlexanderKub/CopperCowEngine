using SharpDX;
using System;
using System.IO;
using CopperCowEngine.AssetsManagement.Loaders;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class MeshAsset : BaseAsset
    {
        public VertexStruct[] Vertices;
        public int[] Indexes;
        public float FileScale = 1.0f;
        public Vector3 Pivot;
        public Vector3 BoundingMinimum;
        public Vector3 BoundingMaximum;

        private MeshAsset[] _subAssets;

        public MeshAsset()
        {
            Type = AssetTypes.Mesh;
        }

        public bool ImportAsset(string path, string ext, float fileScale)
        {
            FileScale = fileScale;
            return ImportAsset(path, ext);
        }

        public override void CopyValues(BaseAsset source)
        {
        }

        public override bool ImportAsset(string path, string ext)
        {
            var modelGeometries = new ModelGeometry[1];

            switch (ext)
            {
                case "obj":
                    modelGeometries[0] = ObjLoader.Load(path);
                    break;
                case "fbx":
                {
                    modelGeometries = FbxLoader.Load(path);
                    if (modelGeometries.Length > 1)
                    {
                        _subAssets = new MeshAsset[modelGeometries.Length - 1];
                        for (int i = 1; i < modelGeometries.Length; i++)
                        {
                            _subAssets[i - 1] = new MeshAsset()
                            {
                                Name = this.Name + "_" + i,
                                FileScale = FileScale,
                                Pivot = Vector3.Zero,
                                Vertices = modelGeometries[i].Points,
                                Indexes = modelGeometries[i].Indexes,
                                BoundingMinimum = modelGeometries[i].BoundingMinimum,
                                BoundingMaximum = modelGeometries[i].BoundingMaximum,
                            };
                        }
                    }

                    break;
                }
                default:
                    Console.WriteLine("Unknown mesh extension: {0}", ext);
                    return false;
            }

            Pivot = Vector3.Zero;
            Vertices = modelGeometries[0].Points;
            Indexes = modelGeometries[0].Indexes;
            BoundingMinimum = modelGeometries[0].BoundingMinimum;
            BoundingMaximum = modelGeometries[0].BoundingMaximum;
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            base.SaveAsset(writer);
            int i;
            writer.Write(FileScale);
            writer.Write(SerializeBlock.GetBytes(Pivot));
            writer.Write(Vertices.Length);
            for (i = 0; i < Vertices.Length; i++)
            {
                var bytes = SerializeBlock.GetBytes(Vertices[i]);
                writer.Write(bytes);
            }
            writer.Write(Indexes.Length);
            for (i = 0; i < Indexes.Length; i++)
            {
                writer.Write(Indexes[i]);
            }
            writer.Write(SerializeBlock.GetBytes(BoundingMinimum));
            writer.Write(SerializeBlock.GetBytes(BoundingMaximum));
            if (_subAssets == null)
            {
                return;
            }
            foreach (var subAsset in _subAssets)
            {
                AssetsManager.GetManager().FileSystemWorker.CreateAssetFile(subAsset, true);
            }
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }
            int i;
            FileScale = reader.ReadSingle();
            Pivot = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            var n = reader.ReadInt32();
            Vertices = new VertexStruct[n];
            for (i = 0; i < n; i++)
            {
                var arr = reader.ReadBytes(96);
                Vertices[i] = SerializeBlock.FromBytes<VertexStruct>(arr);
            }
            n = reader.ReadInt32();
            Indexes = new int[n];
            for (i = 0; i < n; i++)
            {
                Indexes[i] = reader.ReadInt32();
            }
            BoundingMinimum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            BoundingMaximum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            return true;
        }
    }
}
