using System.Diagnostics;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using AssetsManager.Loaders;

namespace EngineCore {
    
    public class Renderer : Component
    {
        public enum SpecificTypeEnum {
            None,
            SkySphere,
            ReflectionSphere,
            Unlit,
        }

        public SpecificTypeEnum SpecificType;
        public bool UseGeometryShader;
        public bool UseShadowVolume;

        public VertexShader VertexShader;
        public PixelShader PixelShader;

        private VertexShader vertexForGeometryShader;
        private VertexShader triangleVertexShader;
        private GeometryShader geometryShader;
        private PixelShader pixelForGeometryShader;

        public InputLayout layout;
        private InputLayout geometryLayout;

        private Buffer geometryOutputBuffer;
        private Buffer geometryConstantBuffer;
        private Buffer geometryVertexBuffer;

        public Buffer VertexBuffer;
        public Buffer IndexBuffer;
        private Buffer geometryIndexBuffer;
        public PrimitiveTopology Topology = PrimitiveTopology.TriangleList;
        public Material RendererMaterial;

        public MaterialPropetyBlock CustomPropertyBlock;
        public MaterialPropetyBlock GetPropetyBlock {
            get {
                if (CustomPropertyBlock == null)
                {
                    return RendererMaterial.PropetyBlock;
                }
                return CustomPropertyBlock;
            }
        }

        public ModelGeometry Geometry
        {
            get {
                return m_Geometry;
            }
            set {
                m_Geometry = value;
            }
        }
        private ModelGeometry m_Geometry;

        private struct ShadowVolumeMatrix
        {
            public Matrix ViewProj;
            public Matrix ModelTransform;
            public Vector4 LightPosition;
        }

        public override void Init() {
            if (RendererMaterial == null) {
                RendererMaterial = Material.DefaultMaterial;
            }
            Engine.Instance.RendererTechnique.InitRenderer(this);

            ShaderBytecodePack geometryShaderBytecodePack = AssetsLoader.GetShaderPack("TriangleShader");
            triangleVertexShader = new VertexShader(
                Engine.Instance.Device,
                geometryShaderBytecodePack.VertexShaderByteCode
            );
            
            geometryShaderBytecodePack = AssetsLoader.GetShaderPack("ExampleGeometryShader");

            if (geometryShaderBytecodePack.HasVS()) {
                vertexForGeometryShader = new VertexShader(
                    Engine.Instance.Device, 
                    geometryShaderBytecodePack.VertexShaderByteCode
                );

                geometryLayout = new InputLayout(
                    Engine.Instance.Device,
                    ShaderSignature.GetInputSignature(geometryShaderBytecodePack.VertexShaderByteCode),
                    new[] {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0),
                    }
                );
            }

            if (geometryShaderBytecodePack.HasPS()) {
                pixelForGeometryShader = new PixelShader(Engine.Instance.Device, geometryShaderBytecodePack.PixelShaderByteCode);
            }

            if (geometryShaderBytecodePack.HasGS()) {
                geometryShader = new GeometryShader(
                    Engine.Instance.Device,
                    geometryShaderBytecodePack.GeometryShaderByteCode,
                    new StreamOutputElement[] {
                        new StreamOutputElement(0, "SV_POSITION", 0, 0, 4, 0),
                        new StreamOutputElement(0, "COLOR", 0, 0, 4, 0),
                    },
                    new int[] {
                        Utilities.SizeOf<ModelGeometry.PositionsColorsStruct>(),
                    },
                    -1,
                    null
                );
                
                geometryConstantBuffer = new Buffer(
                    Engine.Instance.Device,
                    Utilities.SizeOf<ShadowVolumeMatrix>(),
                    ResourceUsage.Default,
                    BindFlags.ConstantBuffer,
                    CpuAccessFlags.None,
                    ResourceOptionFlags.None, 0
                );
                
                geometryOutputBuffer = new Buffer(
                    Engine.Instance.Device,
                    new BufferDescription {
                        BindFlags = BindFlags.VertexBuffer | BindFlags.StreamOutput,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                        Usage = ResourceUsage.Default,
                        StructureByteStride = Utilities.SizeOf<ModelGeometry.PositionsColorsStruct>(),
                        SizeInBytes = Utilities.SizeOf<ModelGeometry.PositionsColorsStruct>() * 1000,
                    }
                );
            }

            BufferDescription bufDesc = new BufferDescription() {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };
            
            if (UseGeometryShader) {
                ModelGeometry.PositionsColorsStruct[] MG = Primitives.Cube(Primitives.White).SVPoints;
                geometryVertexBuffer = Buffer.Create(Engine.Instance.Device, MG, bufDesc);
            }

            bufDesc = new BufferDescription() {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            };

            if (m_Geometry.IndexesWithAdj != null) {
                geometryIndexBuffer = Buffer.Create(
                    Engine.Instance.Device,
                    m_Geometry.IndexesWithAdj,
                    bufDesc
                );
            }
        }


        public void UpdateMesh(ModelGeometry Mesh) {
            Geometry = Mesh;
            UpdateMesh();
        }

        public void UpdateMesh() {
            Engine.Instance.RendererTechnique.InitRenderer(this);
        }

        public override void Draw() {
            Engine.Instance.RendererTechnique.RenderItem(this);
        }

        private ShadowVolumeMatrix m_ShadowVolumeMatrix;
        public void DrawToStreamOutput() {
            if (!UseGeometryShader || !UseShadowVolume) {
                return;
            }

            m_ShadowVolumeMatrix = new ShadowVolumeMatrix() {
                ViewProj = Engine.Instance.MainCamera.ViewProjectionMatrix,
                ModelTransform = gameObject.transform.TransformMatrix,
                LightPosition = Engine.Instance.MainLight != null ?
                    new Vector4(gameObject.transform.Position - Engine.Instance.MainLight.gameObject.transform.Position, 1) 
                    : Vector4.Zero,
            };

            // First Pass
            Engine.Instance.SetWireframeRender();
            Engine.Instance.Context.InputAssembler.InputLayout = geometryLayout;
            Engine.Instance.Context.StreamOutput.SetTarget(geometryOutputBuffer, 0);

            Engine.Instance.Context.UpdateSubresource(ref m_ShadowVolumeMatrix, geometryConstantBuffer);
            Engine.Instance.Context.VertexShader.SetConstantBuffer(0, geometryConstantBuffer);
            Engine.Instance.Context.GeometryShader.SetConstantBuffer(0, geometryConstantBuffer);

            Engine.Instance.Context.VertexShader.Set(vertexForGeometryShader);
            Engine.Instance.Context.GeometryShader.Set(geometryShader);
            Engine.Instance.Context.PixelShader.Set(null);

            Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(geometryVertexBuffer, 32, 0));
            Engine.Instance.Context.InputAssembler.SetIndexBuffer(geometryIndexBuffer, Format.R32_UInt, 0);
            Engine.Instance.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleListWithAdjacency;
            Engine.Instance.Context.DrawIndexed(m_Geometry.IndexesWithAdj.Length, 0, 0);

            // Second Pass
            Engine.Instance.Context.StreamOutput.SetTargets(null);

            Engine.Instance.Context.VertexShader.Set(triangleVertexShader);
            Engine.Instance.Context.GeometryShader.Set(null);
            Engine.Instance.Context.PixelShader.Set(pixelForGeometryShader);

            Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(geometryOutputBuffer, 32, 0));
            Engine.Instance.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Engine.Instance.Context.DrawAuto();
            Engine.Instance.SetSolidRender();
        }

        private int getTopologyVertsCount() {
            return Topology == PrimitiveTopology.TriangleList ? 3 : 2;
        }

        public override void Destroy() {
            VertexShader?.Dispose();
            geometryShader?.Dispose();
            PixelShader?.Dispose();
            vertexForGeometryShader?.Dispose();
            pixelForGeometryShader?.Dispose();

            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
            geometryVertexBuffer?.Dispose();
            geometryOutputBuffer?.Dispose();
            geometryConstantBuffer?.Dispose();

            layout?.Dispose();
        }
    }
}
