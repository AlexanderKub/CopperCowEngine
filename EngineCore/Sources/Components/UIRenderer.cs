using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using AssetsManager.Loaders;

namespace EngineCore
{
    class UIRenderer: Component
    {
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        private ShaderResourceView textureView;
        private SamplerState samplerState;
        private InputLayout layout;
        private Buffer contantBuffer;
        private Buffer vertexBuffer;
        private Buffer indexBuffer;
        public string TexturePath;
        public ShaderResourceView TargetTexture;
        public PrimitiveTopology Topology = PrimitiveTopology.TriangleList;

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
        
        private struct TransformMatrix
        {
            public Matrix transformViewProj;
        }

        private struct UIGeometryPoint
        {
            public Vector4 position;
            public Vector4 uv;
        }

        public override void Init() {
            ShaderBytecodePack shaderBytecodePack = AssetsLoader.GetShaderPack("UIShader");

            vertexShader = new VertexShader(Engine.Instance.Device, shaderBytecodePack.VertexShaderByteCode);

            pixelShader = new PixelShader(Engine.Instance.Device, shaderBytecodePack.PixelShaderByteCode);

            layout = new InputLayout(
                Engine.Instance.Device,
                ShaderSignature.GetInputSignature(shaderBytecodePack.VertexShaderByteCode),
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 16, 0),
                }
            );

            BufferDescription bufDesc = new BufferDescription() {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };

            UIGeometryPoint[] m_UIGeometryPoints = new UIGeometryPoint[m_Geometry.Points.Length];
            for (int i = 0; i < m_UIGeometryPoints.Length; i++) {
                m_UIGeometryPoints[i] = new UIGeometryPoint() {
                    position = m_Geometry.Points[i].Position,
                    uv = m_Geometry.Points[i].UV,
                };
            }
            vertexBuffer = Buffer.Create(Engine.Instance.Device, m_UIGeometryPoints, bufDesc);

            bufDesc = new BufferDescription() {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };

            indexBuffer = Buffer.Create(Engine.Instance.Device, m_Geometry.Indexes, bufDesc);
            
            contantBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<TransformMatrix>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );

            if (string.IsNullOrEmpty(TexturePath)) {
                return;
            }

            //Change path to name
            var texture2D = AssetsLoader.LoadTexture(TexturePath);

            textureView = new ShaderResourceView(Engine.Instance.Device, texture2D);

            samplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription() {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });

            Engine.Instance.Context.GenerateMips(textureView);
        }

        private TransformMatrix m_TransformMatrix;
        public override void Draw() {
            if (Engine.Instance.MainCamera != null) {
                m_TransformMatrix = new TransformMatrix {
                    transformViewProj = gameObject.transform.TransformMatrix,
                };
            }

            Engine.Instance.Context.InputAssembler.InputLayout = layout;

            Engine.Instance.Context.UpdateSubresource(ref m_TransformMatrix, contantBuffer);
            Engine.Instance.Context.VertexShader.SetConstantBuffer(0, contantBuffer);

            Engine.Instance.Context.VertexShader.Set(vertexShader);
            Engine.Instance.Context.PixelShader.Set(pixelShader);

            Engine.Instance.Context.PixelShader.SetShaderResource(0, TargetTexture ?? textureView);
            Engine.Instance.Context.PixelShader.SetSampler(0, samplerState);

            DrawGeometry();
        }

        public void DrawGeometry() {
            Engine.Instance.Context.InputAssembler.PrimitiveTopology = Topology;
            Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, 32, 0));
            Engine.Instance.Context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            Engine.Instance.Context.DrawIndexed(m_Geometry.Count *
                (Topology == PrimitiveTopology.TriangleList ? 3 : 2), 0, 0);
        }

        public override void Destroy() {
            vertexShader.Dispose();
            pixelShader.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            layout.Dispose();
        }
    }
}
