using SharpDX;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class MaterialAsset: BaseAsset
    {
        public Vector3 AlbedoColor = Vector3.One;
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

        public MaterialAsset(MaterialAsset source) : base(source)  {
            AlbedoColor = source.AlbedoColor;
            RoughnessValue = source.RoughnessValue;
            MetallicValue = source.MetallicValue;
            Tile = source.Tile;
            Shift = source.Shift;
            AlbedoMapAsset = source.AlbedoMapAsset;
            AlbedoMapAsset = source.AlbedoMapAsset;
            NormalMapAsset = source.NormalMapAsset;
            RoughnessMapAsset = source.RoughnessMapAsset;
            MetallicMapAsset = source.MetallicMapAsset;
            OcclusionMapAsset = source.OcclusionMapAsset;
        }

        public override bool ImportAsset(string path, string ext) {
            //Create not import
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write(SerializeBlock.GetBytes(AlbedoColor));
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
    }
}
