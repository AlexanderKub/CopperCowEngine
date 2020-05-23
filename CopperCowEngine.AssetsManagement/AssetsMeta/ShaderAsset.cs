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

        public string EntryPoint;

        public Dictionary<string, object> Macro;

        public ShaderAsset()
        {
            Type = AssetTypes.Shader;
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