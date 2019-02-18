using AssetsManager.Loaders;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public class ShaderAsset: BaseAsset
    {
        public byte[] VertexBytecode;
        public byte[] PixelBytecode;
        public byte[] GeometryBytecode;

        public ShaderAsset() {
            this.Type = AssetTypes.Shader;
        }

        public override bool ImportAsset(string path, string ext) {
            if (ext != "hlsl") {
                Console.WriteLine("Unknown shader extension: {0}", ext);
                return false;
            }

            byte[][] pack = ShaderLoader.LoadAndCompileShader(path);
            this.VertexBytecode = pack[0];
            this.PixelBytecode = pack[1];
            this.GeometryBytecode = pack[2];
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write(VertexBytecode?.Length ?? 0);
            if (VertexBytecode != null) {
                writer.Write(VertexBytecode);
            }
            writer.Write(PixelBytecode?.Length ?? 0);
            if (PixelBytecode != null) {
                writer.Write(PixelBytecode);
            }
            writer.Write(GeometryBytecode?.Length ?? 0);
            if (GeometryBytecode != null) {
                writer.Write(GeometryBytecode);
            }
        }

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
                return false;
            }
            int n;
            n = reader.ReadInt32();
            if (n > 0) {
                this.VertexBytecode = reader.ReadBytes(n);
            }
            n = reader.ReadInt32();
            if (n > 0) {
                this.PixelBytecode = reader.ReadBytes(n);
            }
            n = reader.ReadInt32();
            if (n > 0) {
                this.GeometryBytecode = reader.ReadBytes(n);
            }
            return true;
        }
    }
}
