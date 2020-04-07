using System;
using SharpDX;
using System.IO;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class MaterialAsset : BaseAsset
    {
        public Vector3 AlbedoColor = Vector3.One;
        public float AlphaValue = 1.0f;
        public float RoughnessValue = 0.8f;
        public float MetallicValue = 0.05f;
        public Vector2 Tile = Vector2.One;
        public Vector2 Shift = Vector2.Zero;

        public string AlbedoMapAsset = string.Empty;
        public string NormalMapAsset = string.Empty;
        public string RoughnessMapAsset = string.Empty;
        public string MetallicMapAsset = string.Empty;
        public string OcclusionMapAsset = string.Empty;

        public MaterialAsset()
        {
            Type = AssetTypes.Material;
        }

        public override void CopyValues(BaseAsset source)
        {
            if (!(source is MaterialAsset materialAsset))
            {
                return;
            }

            AlbedoColor = materialAsset.AlbedoColor;
            AlphaValue = materialAsset.AlphaValue;
            RoughnessValue = materialAsset.RoughnessValue;
            MetallicValue = materialAsset.MetallicValue;
            Tile = materialAsset.Tile;
            Shift = materialAsset.Shift;
            AlbedoMapAsset = materialAsset.AlbedoMapAsset;
            AlbedoMapAsset = materialAsset.AlbedoMapAsset;
            NormalMapAsset = materialAsset.NormalMapAsset;
            RoughnessMapAsset = materialAsset.RoughnessMapAsset;
            MetallicMapAsset = materialAsset.MetallicMapAsset;
            OcclusionMapAsset = materialAsset.OcclusionMapAsset;
        }

        public override bool ImportAsset(string path, string ext)
        {
            //Create not import
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            base.SaveAsset(writer);
            writer.Write(SerializeBlock.GetBytes(AlbedoColor));
            writer.Write(AlphaValue);
            writer.Write(RoughnessValue);
            writer.Write(MetallicValue);
            writer.Write(SerializeBlock.GetBytes(Tile));
            writer.Write(SerializeBlock.GetBytes(Shift));
            writer.Write(AlbedoMapAsset);
            writer.Write(NormalMapAsset);
            writer.Write(RoughnessMapAsset);
            writer.Write(MetallicMapAsset);
            writer.Write(OcclusionMapAsset);
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }
            AlbedoColor = SerializeBlock.FromBytes<Vector3>(reader.ReadBytes(12));
            AlphaValue = reader.ReadSingle();
            RoughnessValue = reader.ReadSingle();
            MetallicValue = reader.ReadSingle();
            Tile = SerializeBlock.FromBytes<Vector2>(reader.ReadBytes(8));
            Shift = SerializeBlock.FromBytes<Vector2>(reader.ReadBytes(8));
            AlbedoMapAsset = reader.ReadString();
            NormalMapAsset = reader.ReadString();
            RoughnessMapAsset = reader.ReadString();
            MetallicMapAsset = reader.ReadString();
            OcclusionMapAsset = reader.ReadString();
            return true;
        }

        public override bool IsSame(BaseAsset other)
        {
            if (!(other is MaterialAsset otherAsset))
            {
                return false;
            }

            var same = AlbedoColor == otherAsset.AlbedoColor;
            same &= Math.Abs(this.AlphaValue - otherAsset.AlphaValue) < float.Epsilon;
            same &= Math.Abs(this.RoughnessValue - otherAsset.RoughnessValue) < float.Epsilon;
            same &= Math.Abs(this.MetallicValue - otherAsset.MetallicValue) < float.Epsilon;
            same &= Tile == otherAsset.Tile;
            same &= Shift == otherAsset.Shift;
            same &= AlbedoMapAsset == otherAsset.AlbedoMapAsset;
            same &= AlbedoMapAsset == otherAsset.AlbedoMapAsset;
            same &= NormalMapAsset == otherAsset.NormalMapAsset;
            same &= RoughnessMapAsset == otherAsset.RoughnessMapAsset;
            same &= MetallicMapAsset == otherAsset.MetallicMapAsset;
            same &= OcclusionMapAsset == otherAsset.OcclusionMapAsset;

            return base.IsSame(other) && same;
        }
    }
}
