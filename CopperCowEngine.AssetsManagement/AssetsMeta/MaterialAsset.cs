using System;
using System.IO;
using System.Numerics;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class MaterialAsset : BaseAsset
    {
        public Vector3 AlbedoColor = Vector3.One;
        public float AlphaValue = 1.0f;
        public Vector3 EmissiveColor = Vector3.Zero;
        public float RoughnessValue = 0.8f;
        public float MetallicValue = 0.05f;
        public Vector2 Tile = Vector2.One;
        public Vector2 Shift = Vector2.Zero;

        public string AlbedoMapAsset = string.Empty;
        public string EmissiveMapAsset = string.Empty;
        public string NormalMapAsset = string.Empty;
        public string RoughnessMapAsset = string.Empty;
        public string MetallicMapAsset = string.Empty;
        public string OcclusionMapAsset = string.Empty;

        public MaterialAsset()
        {
            Type = AssetTypes.Material;
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }
            AlbedoColor = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            AlphaValue = reader.ReadSingle();
            EmissiveColor = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            RoughnessValue = reader.ReadSingle();
            MetallicValue = reader.ReadSingle();
            Tile = SerializeBlock.FromBytes<Vector2>(reader.ReadBytes(8));
            Shift = SerializeBlock.FromBytes<Vector2>(reader.ReadBytes(8));
            AlbedoMapAsset = reader.ReadString();
            EmissiveMapAsset = reader.ReadString();
            NormalMapAsset = reader.ReadString();
            RoughnessMapAsset = reader.ReadString();
            MetallicMapAsset = reader.ReadString();
            OcclusionMapAsset = reader.ReadString();
            return true;
        }
    }
}
