using AssetsManager.Loaders;
using System;
using System.IO;

namespace AssetsManager.AssetsMeta
{
    public enum ShaderTypeEnum
    {
        Vertex, Pixel, Geometry, Compute,
    }

    public class ShaderAsset: BaseAsset
    {
        public ShaderTypeEnum ShaderType;
        public byte[] Bytecode;

        public ShaderAsset() {
            this.Type = AssetTypes.Shader;
        }

        public override bool ImportAsset(string path, string ext) {
            if (ext != "hlsl") {
                Console.WriteLine("Unknown shader extension: {0}", ext);
                return false;
            }

            this.Bytecode = ShaderLoader.LoadAndCompileShader(path, this.ShaderType);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer) {
            base.SaveAsset(writer);
            writer.Write((int)this.ShaderType);
            writer.Write(this.Bytecode?.Length ?? 0);
            if (this.Bytecode != null) {
                writer.Write(this.Bytecode);
            }
        }

        public override bool LoadAsset(BinaryReader reader) {
            if (!base.LoadAsset(reader)) {
                return false;
            }
            this.ShaderType = (ShaderTypeEnum)reader.ReadInt32();
            int n = reader.ReadInt32();
            if (n > 0) {
                this.Bytecode = reader.ReadBytes(n);
            }
            return true;
        }
    }
}
