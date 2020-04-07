using EngineCore.Utils;
using SharpDX;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ShaderGraph
{
    // Enums
    public sealed partial class MetaMaterial
    {
        public enum MaterialDomain
        {
            Surface,
        }

        public enum BlendMode
        {
            Opaque = 100000,
            Masked = 200000,
            Translucent = 300000,
            Additive = 400000,
            Modulate = 500000,
        }

        public enum ShadingMode
        {
            Unlit = 10000,
            Default = 20000,
        }

        public enum CullMode
        {
            Front = 1000,
            Back = 2000,
            None = 3000,
        }
    }

    public sealed partial class MetaMaterial
    {
        public MaterialDomain materialDomain = MaterialDomain.Surface;
        public BlendMode blendMode = BlendMode.Opaque; // States and shader template
        public ShadingMode shadingMode = ShadingMode.Default; // Layouts and shader template
        public CullMode cullMode = CullMode.Back; // States

        public bool Wireframe = false;
        public float OpacityMaskClipValue = 0.3333f;

        public int Queue {
            get {
                return (int)blendMode + (int)shadingMode + (int)cullMode + (Wireframe ? 100 : 200);
            }
        }
        public static MetaMaterial Standard = new MetaMaterial();
    }

    public class MaterialBase
    {

        public interface IMaterialAttributes
        {
            Vector4[] Pack();
        }

        public class MaterialAttributes : IMaterialAttributes
        {
            public Vector3 BaseColor;
            public float Metallic;
            public float Roughness;
            public Vector3 EmissiveColor;
            public float Opacity;
            public Vector3 Normal;
            public float AmbientOcclusion;

            public Vector4[] Pack()
            {
                return new Vector4[]{
                    new Vector4(BaseColor, Opacity),
                    new Vector4(EmissiveColor, 0),
                    new Vector4(Normal, 0),
                    new Vector4(Metallic, Roughness, AmbientOcclusion, 0),
                };
            }
        }

        public struct OpaqueUnlitMaterialAttributes
        {
            public Vector3 EmissiveColor;
            public Vector3 Normal;
            public Vector3 WorldPositionOffset;
            public float PixelDepthOffset;
        }
    }

    public class  MaterialInstance
    {
        public MaterialBase Shader;
        public string[] Parameters;
    }

    #region Header
    public partial class ShaderGraphMaterial
    {
        public enum Channel
        {
            R, G, B, A, RG, GB, RGB, RGBA
        }

        public enum VariableType
        {
            FLOAT,
            FLOAT2 = 4,
            FLOAT3 = 6,
            FLOAT4 = 7,
        }

        public enum TextureType
        {
            Texture2D,
            TextureCube,
        }

        public interface IBaseShaderInstruction
        {
            void Compile(ShaderGraphMaterial shader);
        }

        protected class TypeMismatch : Exception {
            public TypeMismatch(int instructionIndex, string a, string b) : 
                base($"Instruction #{instructionIndex}: Types mismatch. Expected {a.ToLower()}, catched {b.ToLower()}") { }
        }

        public struct VariableDefinition
        {
            public static VariableDefinition BaseColor = new VariableDefinition() { Name = "BaseColor", Type = VariableType.FLOAT3 };
            public static VariableDefinition Metallic = new VariableDefinition() { Name = "Metallic", Type = VariableType.FLOAT };
            public static VariableDefinition Roughness = new VariableDefinition() { Name = "Roughness", Type = VariableType.FLOAT };
            public static VariableDefinition EmissiveColor = new VariableDefinition() { Name = "EmissiveColor", Type = VariableType.FLOAT3 };
            public static VariableDefinition Opacity = new VariableDefinition() { Name = "Opacity", Type = VariableType.FLOAT };
            public static VariableDefinition Normal = new VariableDefinition() { Name = "Normal", Type = VariableType.FLOAT3 };
            public static VariableDefinition AmbientOcclusion = new VariableDefinition() { Name = "AmbientOcclusion", Type = VariableType.FLOAT };
            public static VariableDefinition OpacityMaskClipValue = new VariableDefinition() { Name = "OpacityMaskClipValue", Type = VariableType.FLOAT };

            public VariableType Type;
            public string Name;

            public override bool Equals(object obj)
            {
                return Equals((VariableDefinition)obj);
            }

            public bool Equals(VariableDefinition other)
            {
                return other.Type == Type && other.Name == Name;
            }

            public override int GetHashCode()
            {
                return Tuple.Create(Type, Name).GetHashCode();
            }
        }
    }
    #endregion

    #region Behavior
    public partial class ShaderGraphMaterial
    {
        private List<VariableDefinition> DefinedVariables = new List<VariableDefinition>();
        internal VariableDefinition[] GetDefinedVariables {
            get {
                VariableDefinition[] result = new VariableDefinition[DefinedVariables.Count];
                DefinedVariables.CopyTo(result);
                return result;
            }
        }

        private readonly MetaMaterial m_MetaMaterial;
        private List<IBaseShaderInstruction> Instructions;

        private List<TextureType> TexturesTypes;
        private List<string> TexturesSlots;
        private List<string> SamplersSlots;
        private List<string> CodeLines;
        private List<VariableDefinition> CustomVariables;
        private string m_ShaderTemplate;

        protected int CurrentInstructionIndex;
        private string m_Name;

        public ShaderGraphMaterial(string name, MetaMaterial meta) :this(name, meta, new List<IBaseShaderInstruction>()) { }
        public ShaderGraphMaterial(string name, MetaMaterial meta, List<IBaseShaderInstruction> instructions)
        {
            m_Name = name;
            m_MetaMaterial = meta;
            Instructions = instructions;
            SamplersSlots = new List<string>();
            TexturesSlots = new List<string>();
            TexturesTypes = new List<TextureType>();

            CodeLines = new List<string>();
            CustomVariables = new List<VariableDefinition>();

            // TODO: switch template by shader type
            m_ShaderTemplate = "";
            // SOME SPAGHETTI CARBANANA
            if (m_MetaMaterial.shadingMode == MetaMaterial.ShadingMode.Unlit)
            {
                m_ShaderTemplate = File.ReadAllText("Sources/Shaders/Templates/Unlit.hlsl");
                DefinedVariables.Add(VariableDefinition.EmissiveColor);
            }
            if (m_MetaMaterial.blendMode >= MetaMaterial.BlendMode.Masked)
            {
                DefinedVariables.Add(VariableDefinition.Opacity);
                if (m_MetaMaterial.blendMode == MetaMaterial.BlendMode.Masked)
                {
                    DefinedVariables.Add(VariableDefinition.OpacityMaskClipValue);
                }
            }
            /*if (m_MetaMaterial.blendMode == MetaMaterial.BlendMode.Opaque || m_MetaMaterial.blendMode == MetaMaterial.BlendMode.Masked)
            {
                if (m_MetaMaterial.shadingMode == MetaMaterial.ShadingMode.Default)
                {
                    m_ShaderTemplate = File.ReadAllText("Sources/Shaders/Templates/OpaqueAndMaskedDefault.hlsl");
                    DefinedVariables.Add(VariableDefinition.BaseColor);
                    DefinedVariables.Add(VariableDefinition.Metallic);
                    DefinedVariables.Add(VariableDefinition.Roughness);
                    DefinedVariables.Add(VariableDefinition.Normal);
                    DefinedVariables.Add(VariableDefinition.AmbientOcclusion);
                }
            }*/
        }

        protected void AddVariable(VariableDefinition definition)
        {
            if (CustomVariables.Contains(definition) || DefinedVariables.Contains(definition))
            {
                return;
            }
            CustomVariables.Add(definition);
        }

        protected void AddSetValue(VariableDefinition var, string value)
        {
            if (!CustomVariables.Contains(var) && !DefinedVariables.Contains(var))
            {
                CustomVariables.Add(var);
            }
            CodeLines.Add(var.Name + "=" + value + ";");
        }

        protected void AddTextureSlot(TextureType type, string slot)
        {
            if (TexturesSlots.Contains(slot))
            {
                return;
            }
            TexturesTypes.Add(type);
            TexturesSlots.Add(slot);
        }

        protected void AddSamplerSlot(string slot)
        {
            if (SamplersSlots.Contains(slot))
            {
                return;
            }
            SamplersSlots.Add(slot);
        }

        public void AddInstruction(IBaseShaderInstruction instruct)
        {
            Instructions.Add(instruct);
        }

        public void Compile()
        {
            CurrentInstructionIndex = 0;
            bool WithError = false;
            foreach (var item in Instructions)
            {
                try {
                    item.Compile(this);
                } catch(Exception e) {
                    WithError = true;
                    Console.WriteLine(e.Message);
                }
                CurrentInstructionIndex++;
            }

            if (WithError) {
                return;
            }
            
            string ShaderSource = GatherCompiledInstructions(m_ShaderTemplate);
            if (string.IsNullOrEmpty(ShaderSource))
            {
                return;
            }
            if (!CompileShader(ShaderSource, out ShaderBytecode bytecode)) {
                Console.WriteLine(ShaderSource);
                return;
            }
            SaveBytecode(bytecode);
            SaveAsset(m_Name, bytecode);
            SaveSourceCode(ShaderSource);
            Reset();
        }

        private void SaveSourceCode(string shaderSource)
        {
            //Debug
            Console.WriteLine(shaderSource);
        }

        private void SaveBytecode(ShaderBytecode bytecode)
        {
            //TODO: Save bytecode to File?
        }

        private void SaveAsset(string name, ShaderBytecode bytecode) {
            //AssetsManager.AssetsManagerInstance.GetManager().CreateShaderGraphAsset(name, bytecode);
        }

        private void Reset()
        {
            // Keep Instructions for Asset
            //Instructions.Clear();
            TexturesTypes.Clear();
            TexturesSlots.Clear();
            SamplersSlots.Clear();
            CustomVariables.Clear();
            CodeLines.Clear();
        }

        private string GatherCompiledInstructions(string template)
        {
            string shaderCodeOutput = template;
            #region Textures
            string tmp = "";
            for (int i = 0; i < TexturesSlots.Count; i++)
            {
                tmp += $"{TexturesTypes[i].ToString()} {TexturesSlots[i]}: register(t{i});\n";
            }

            shaderCodeOutput = shaderCodeOutput.Replace($"//<TEXTURES>", tmp);
            #endregion

            #region Samplers
            tmp = "";
            for (int i = 0; i < SamplersSlots.Count; i++)
            {
                tmp += $"SamplerState {SamplersSlots[i]}: register(s{i});\n";
            }
            shaderCodeOutput = shaderCodeOutput.Replace($"//<SAMPLERS>", tmp);
            #endregion

            #region Variables
            tmp = "";
            foreach (var item in CustomVariables)
            {
                tmp += "\n" + item.Type.ToString().ToLower() + " " + item.Name + ";";

            }
            shaderCodeOutput = shaderCodeOutput.Replace("//<VARIABLES>", tmp);
            #endregion

            #region Code
            tmp = "";
            for (int i = 0; i < CodeLines.Count; i++)
            {
                tmp += "\n" + CodeLines[i];
            }
            shaderCodeOutput = shaderCodeOutput.Replace("//<CODE>", tmp);
            #endregion
            return shaderCodeOutput;
        }

        private static SharpDX.Direct3D.ShaderMacro[] MaskedMacro = new SharpDX.Direct3D.ShaderMacro[] {
            new SharpDX.Direct3D.ShaderMacro("MASKED_BLEND", 1),
        };
        private static SharpDX.Direct3D.ShaderMacro[] AlphaMacro = new SharpDX.Direct3D.ShaderMacro[] {
            new SharpDX.Direct3D.ShaderMacro("ALPHA_BLEND", 1),
        }; 

        private bool CompileShader(string source, out ShaderBytecode bytecode)
        {
            SharpDX.Direct3D.ShaderMacro[] macro = null;
            if (m_MetaMaterial.blendMode > MetaMaterial.BlendMode.Masked)
            {
                macro = AlphaMacro;
            }
            else if (m_MetaMaterial.blendMode == MetaMaterial.BlendMode.Masked)
            {
                macro = MaskedMacro;
            }
            // TODO: Set shader macro by shader Type
            CompilationResult ShaderByteCode = ShaderBytecode.Compile(source, "PSMain", "ps_5_0",
            ShaderFlags.PackMatrixRowMajor, EffectFlags.None,
            macro,
            null
            );
            if (ShaderByteCode.Bytecode == null || ShaderByteCode.HasErrors)
            {
                Console.WriteLine(ShaderByteCode.Message);
                bytecode = null;
                return false;
            }
            bytecode = ShaderByteCode.Bytecode;
            ShaderGraphCompiler.LastCompiledBytecode = bytecode;
            return true;
        }
    }
    #endregion

    #region Instructions
    public partial class ShaderGraphMaterial
    {
        /// <summary>
        /// Attribute to mark shader graph instructions and make them visible for ShaderGraphCompiler.GatherShaderGraphInstructions.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        internal class ShaderGraphInstructionAttribute : Attribute
        {
            public readonly string NodeName;

            public ShaderGraphInstructionAttribute(string name)
            {
                this.NodeName = name;
            }
        }

        [ShaderGraphInstruction("SampleTexture")]
        public class SampleTextureInstruction : IBaseShaderInstruction
        {
            private string m_TextureSlot;
            private TextureType m_TextureType;
            private string m_SamplerSlot;
            private string m_UVSlot;
            private Channel m_ChannelMask;
            private VariableDefinition m_OutParam;

            public SampleTextureInstruction(string sampler, TextureType type, string slot, string uv, Channel channel, VariableDefinition outParam)
            {
                m_SamplerSlot = sampler;
                m_TextureType = type;
                m_TextureSlot = slot;
                m_UVSlot = uv;
                m_ChannelMask = channel;
                m_OutParam = outParam;
            }

            public void Compile(ShaderGraphMaterial shader)
            {
                if ((int)m_ChannelMask < (int)m_OutParam.Type || (int)m_ChannelMask >= (int)m_OutParam.Type + 1)
                {
                    throw new TypeMismatch(shader.CurrentInstructionIndex, 
                        m_OutParam.Type.ToString(), ((VariableType)((int)m_ChannelMask)).ToString());
                }
                shader.AddTextureSlot(m_TextureType, m_TextureSlot);
                shader.AddSamplerSlot(m_SamplerSlot);
                shader.AddSetValue(m_OutParam, $"{m_TextureSlot}.Sample({m_SamplerSlot}, {m_UVSlot}).{m_ChannelMask.ToString().ToLower()}");
            }
        }

        [ShaderGraphInstruction("Variable")]
        public class AddVariableInstruction : IBaseShaderInstruction
        {
            VariableDefinition m_Definition;

            public AddVariableInstruction(VariableDefinition definition)
            {
                m_Definition = definition;
            }

            public void Compile(ShaderGraphMaterial shader)
            {
                shader.AddVariable(m_Definition);
            }
        }

        [ShaderGraphInstruction("Set")]
        public class SetVariableValue : IBaseShaderInstruction
        {
            VariableDefinition m_Variable;
            object m_Value;

            public SetVariableValue(VariableDefinition Variable, object Value)
            {
                m_Variable = Variable;
                m_Value = Value;
            }

            public void Compile(ShaderGraphMaterial shader)
            {
                shader.AddSetValue(m_Variable, m_Value.ToString());
            }
        }

        [ShaderGraphInstruction("SetByMask")]
        public class SetVariableValueMask : IBaseShaderInstruction
        {
            VariableDefinition m_Variable;
            VariableDefinition m_Value;
            Channel m_Mask;

            public SetVariableValueMask(VariableDefinition Variable, VariableDefinition Value, Channel Mask)
            {
                m_Variable = Variable;
                m_Value = Value;
                m_Mask = Mask;
            }

            public void Compile(ShaderGraphMaterial shader)
            {
                shader.AddSetValue(m_Variable, m_Value.Name.ToString()+ "." + m_Mask.ToString().ToLower());
            }
        }
    }
    #endregion

    public class ShaderGraphCompiler
    {
        public static Dictionary<string,Type> GatherShaderGraphInstructions()
        {
            Dictionary<string, Type> nodes = new Dictionary<string, Type>();
            Type[] types = Miscellaneous.GetAllClassesWithAttribute<ShaderGraphMaterial.ShaderGraphInstructionAttribute>().ToArray();
            string nodeName;
            foreach (var t in types)
            {
                nodeName = (t.GetCustomAttributes(typeof(ShaderGraphMaterial.ShaderGraphInstructionAttribute), 
                    true).First() as ShaderGraphMaterial.ShaderGraphInstructionAttribute).NodeName;
                nodes.Add(nodeName, t);
            }
            return nodes;
        }

        public static byte[] LastCompiledBytecode;
        public void Compile(ShaderGraphMaterial material)
        {
            var t = GatherShaderGraphInstructions();
            material.Compile();
            // TODO: saving, caching? etc
        }
    }

    public class ShaderGraphNode
    {
        public string NodeName;
        public ShaderGraphMaterial.IBaseShaderInstruction Instruction;
        public Vector2 Position;
        public Vector2 HalfSize;
        

        private static ShaderGraphNode DEBUGShaderGraphNode1 = new ShaderGraphNode()
        {
            NodeName = "SampleTexture",
            Position = new Vector2(200, 200),
            HalfSize = new Vector2(50, 50),
        };

        private static ShaderGraphNode DEBUGShaderGraphNode2 = new ShaderGraphNode()
        {
            NodeName = "Set Value Mask",
            Position = new Vector2(400, 400),
            HalfSize = new Vector2(50, 50),
        };

        private static ShaderGraphNode DEBUGShaderGraphNode3 = new ShaderGraphNode()
        {
            NodeName = "Set Value",
            Position = new Vector2(400, 200),
            HalfSize = new Vector2(50, 50),
        };

        public static List<ShaderGraphNode> DEBUGShaderGraphNodes = new List<ShaderGraphNode>()
        {
            DEBUGShaderGraphNode1,
            DEBUGShaderGraphNode3,
            DEBUGShaderGraphNode2,
        };

        private static ShaderGraphLink DEBUGShaderGraphLink = new ShaderGraphLink()
        {
            From = DEBUGShaderGraphNode1,
            To = DEBUGShaderGraphNode2,
            FromPin = 1,
            ToPin = 0,
        };

        private static ShaderGraphLink DEBUGShaderGraphLink2 = new ShaderGraphLink()
        {
            From = DEBUGShaderGraphNode1,
            To = DEBUGShaderGraphNode3,
            FromPin = 0,
            ToPin = 0,
        };

        public static List<ShaderGraphLink> DEBUGShaderGraphLinks = new List<ShaderGraphLink>()
        {
            DEBUGShaderGraphLink, DEBUGShaderGraphLink2,
        };
    }

    public class ShaderGraphLink
    {
        public ShaderGraphNode From;
        public int FromPin;
        public ShaderGraphNode To;
        public int ToPin;
    }

    public class TestShadersCompiler
    {
        public void TestItAll()
        {
            var TextureSample = new ShaderGraphMaterial.VariableDefinition()
            {
                Name = "TextureSample",
                Type = ShaderGraphMaterial.VariableType.FLOAT4,
            };

            var instructions = new List<ShaderGraphMaterial.IBaseShaderInstruction>()
            {
                new ShaderGraphMaterial.AddVariableInstruction(TextureSample),
                /*new ShaderGraphMaterial.SampleTextureInstruction("LinearSampler",
                    ShaderGraphMaterial.TextureType.Texture2D,
                    "Texture0", "input.uv0",
                    ShaderGraphMaterial.Channel.RGBA,
                    TextureSample),
                new ShaderGraphMaterial.SetVariableValueMask(ShaderGraphMaterial.VariableDefinition.EmissiveColor, 
                    TextureSample, ShaderGraphMaterial.Channel.RGB),*/
                new ShaderGraphMaterial.SetVariableValue(ShaderGraphMaterial.VariableDefinition.EmissiveColor, 
                    "float3(0.0f, 0.533f, 1.0f)"),
                new ShaderGraphMaterial.SetVariableValueMask(ShaderGraphMaterial.VariableDefinition.Opacity,
                    TextureSample, ShaderGraphMaterial.Channel.A),
            };

            MetaMaterial mm = new MetaMaterial()
            {
                materialDomain = MetaMaterial.MaterialDomain.Surface,
                blendMode = MetaMaterial.BlendMode.Translucent,
                shadingMode = MetaMaterial.ShadingMode.Unlit,
                cullMode = MetaMaterial.CullMode.None,
                Wireframe = true,
            };

            ShaderGraphMaterial material = new ShaderGraphMaterial("TestShader", mm, instructions);
            material.AddInstruction(new ShaderGraphMaterial.SetVariableValue(
                ShaderGraphMaterial.VariableDefinition.Opacity, "0.7f"));

            ShaderGraphCompiler compiler = new ShaderGraphCompiler();
            compiler.Compile(material);
        }
    }
}
