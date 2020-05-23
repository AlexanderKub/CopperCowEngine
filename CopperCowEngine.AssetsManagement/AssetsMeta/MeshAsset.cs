using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using CopperCowEngine.AssetsManagement.FSWorkers;
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
        
        internal MeshAsset[] SubAssets;

        public MeshAsset()
        {
            Type = AssetTypes.Mesh;
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }
            int i;
            FileScale = reader.ReadSingle();
            Pivot = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(sizeof(float) * 3));
            var n = reader.ReadInt32();
            Vertices = new VertexStruct[n];
            for (i = 0; i < n; i++)
            {
                var arr = reader.ReadBytes(Marshal.SizeOf<VertexStruct>());
                Vertices[i] = SerializeBlock.FromBytes<VertexStruct>(arr);
            }
            n = reader.ReadInt32();
            Indexes = new int[n];
            for (i = 0; i < n; i++)
            {
                Indexes[i] = reader.ReadInt32();
            }
            BoundingMinimum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(sizeof(float) * 3));
            BoundingMaximum = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(sizeof(float) * 3));
            return true;
        }
    }
}
