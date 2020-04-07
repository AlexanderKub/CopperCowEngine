using System;
using System.Collections.Generic;
using System.IO;
using CopperCowEngine.Rendering;

namespace CopperCowEngine.AssetsManagement.AssetsMeta
{
    public class ShaderAsset : BaseAsset
    {
        public ShaderType ShaderType;

        public byte[] Bytecode;

        internal string EntryPoint;

        internal Dictionary<string, object> Macro;

        public ShaderAsset()
        {
            Type = AssetTypes.Shader;
        }

        public override void CopyValues(BaseAsset source)
        {
        }

        public override bool ImportAsset(string path, string ext)
        {
            if (ext != "hlsl")
            {
                Console.WriteLine("Unknown shader extension: {0}", ext);
                return false;
            }

            Bytecode = AssetsManager.RenderBackend.CompileAndImportShader(path, ShaderType, EntryPoint, Macro);
            return true;
        }

        public override void SaveAsset(BinaryWriter writer)
        {
            base.SaveAsset(writer);
            writer.Write((int)ShaderType);
            writer.Write(Bytecode?.Length ?? 0);
            if (Bytecode != null)
            {
                writer.Write(Bytecode);
            }
        }

        public override bool LoadAsset(BinaryReader reader)
        {
            if (!base.LoadAsset(reader))
            {
                return false;
            }
            ShaderType = (ShaderType)reader.ReadInt32();

            var n = reader.ReadInt32();
            if (n > 0)
            {
                Bytecode = reader.ReadBytes(n);
            }
            return true;
        }
    }
}