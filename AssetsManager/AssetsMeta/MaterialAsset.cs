using SharpDX;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class MaterialAsset: BaseAsset
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

        public MaterialAsset() {
            this.Type = AssetTypes.Material;
        }

        public MaterialAsset(MaterialAsset source) {
            CopyValues(source);
        }

        public override void CopyValues(BaseAsset source) {
            base.CopyValues(source);
            MaterialAsset ma = source as MaterialAsset;
            AlbedoColor = ma.AlbedoColor;
            AlphaValue = ma.AlphaValue;
            RoughnessValue = ma.RoughnessValue;
            MetallicValue = ma.MetallicValue;
            Tile = ma.Tile;
            Shift = ma.Shift;
            AlbedoMapAsset = ma.AlbedoMapAsset;
            AlbedoMapAsset = ma.AlbedoMapAsset;
            NormalMapAsset = ma.NormalMapAsset;
            RoughnessMapAsset = ma.RoughnessMapAsset;
            MetallicMapAsset = ma.MetallicMapAsset;
            OcclusionMapAsset = ma.OcclusionMapAsset;
        }

        public override bool ImportAsset(string path, string ext) {
            //Create not import
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
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

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
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

        public override bool IsSame(BaseAsset other) {
            bool same = true;
            MaterialAsset otherMA = other as MaterialAsset;
            same &= this.AlbedoColor == otherMA.AlbedoColor;
            same &= this.AlphaValue == otherMA.AlphaValue;
            same &= this.RoughnessValue == otherMA.RoughnessValue;
            same &= this.MetallicValue == otherMA.MetallicValue;
            same &= this.Tile == otherMA.Tile;
            same &= this.Shift == otherMA.Shift;
            same &= this.AlbedoMapAsset == otherMA.AlbedoMapAsset;
            same &= this.AlbedoMapAsset == otherMA.AlbedoMapAsset;
            same &= this.NormalMapAsset == otherMA.NormalMapAsset;
            same &= this.RoughnessMapAsset == otherMA.RoughnessMapAsset;
            same &= this.MetallicMapAsset == otherMA.MetallicMapAsset;
            same &= this.OcclusionMapAsset == otherMA.OcclusionMapAsset;
            return base.IsSame(other) && same;
        }
    }
}
