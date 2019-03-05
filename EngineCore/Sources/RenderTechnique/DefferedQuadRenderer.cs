using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore.RenderTechnique
{
    class DefferedQuadRenderer
    {
        private VertexShader m_VertexShader;
        private PixelShader m_PixelShader;
      
        public struct VerticiesPoint
        {
            public Vector4 Position;
            public Vector4 UV;
        }

        private VerticiesPoint[] m_VerticiesPoints;
        private Buffer m_VertexBuffer;

        private int[] m_Indicies = new int[] {
            0, 2, 1,
            3, 2, 0,
        };
        private Buffer m_IndexBuffer;

        private SamplerState m_SamplerState;
        private InputLayout m_InputLayout;

        private Buffer LightBuffer;
        private Buffer ConstBuffer;

        private Vector3 m_Position;
        private Quaternion m_Rotation;
        private Vector3 m_Scale;
        private int m_Type;

        public Vector3 Scale {
            get {
                return m_Scale;
            }
            set {
                m_Scale = value;
                UpdateTransformMatrix();
            }
        }
        public Vector3 Position {
            get {
                return m_Position;
            }
            set {
                m_Position = value;
                UpdateTransformMatrix();
            }
        }

        private bool m_FullScreen;
        public bool FullScreen {
            get {
                return m_FullScreen;
            }
            set {
                m_FullScreen = value;
                UpdateTransformMatrix();
            }
        }

        public DefferedQuadRenderer() : this(Vector2.Zero, Vector2.One, Quaternion.Identity, 0) { }

        public DefferedQuadRenderer(Vector2 pos) : this(pos, Vector2.One, Quaternion.Identity, 0) { }

        public DefferedQuadRenderer(Vector2 pos, Vector2 scale, int Type) : this(pos, scale, Quaternion.Identity, Type) { }

        public DefferedQuadRenderer(Vector2 pos, Vector2 scale, Quaternion rot, int Type) {
            m_Position = new Vector3(pos, 0f);
            m_Scale = new Vector3(scale, 1f);
            m_Rotation = rot;
            m_Type = Type;

            m_VerticiesPoints = new VerticiesPoint[] {
                new VerticiesPoint() {
                   Position = new Vector4(-1f, 1f, 0, 1f),
                   UV = new Vector4(0, 0, 0, 0),
                },
                new VerticiesPoint() {
                   Position = new Vector4(-1f, -1f, 0, 1f),
                   UV = new Vector4(0, 1f, 0, 0),
                },
                new VerticiesPoint() {
                   Position = new Vector4(1f,-1f, 0, 1f),
                    UV = new Vector4(1f, 1f, 0, 0),
                },
                new VerticiesPoint() {
                   Position = new Vector4(1f, 1f, 0, 1f),
                   UV = new Vector4(1f, 0, 0, 0),
                },
            };
            
            m_VertexBuffer = Buffer.Create(Engine.Instance.Device, m_VerticiesPoints, new BufferDescription() {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            });

            m_IndexBuffer = Buffer.Create(Engine.Instance.Device, m_Indicies, new BufferDescription() {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
            });

            m_VertexShader = AssetsLoader.GetShader<VertexShader>("PBRDefferedQuadVS", out ShaderSignature shaderSignature);
            m_InputLayout = new InputLayout(
                Engine.Instance.Device,
                shaderSignature,
                new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, 16, 0),
                }
            );

            m_PixelShader = AssetsLoader.GetShader <PixelShader>("PBRDefferedQuadPS");

            m_SamplerState = new SamplerState(Engine.Instance.Device, new SamplerStateDescription() {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
            });

            UpdateTransformMatrix();
            m_LightBuffer = new LightBufferStruct[]{ new LightBufferStruct(), new LightBufferStruct(), new LightBufferStruct() };
            LightBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<LightBufferStruct>() * m_LightBuffer.Length,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
        }

        internal void UpdateTransformMatrix() {
            Matrix m_TransformMatrix = Matrix.Identity;
            m_TransformMatrix *= Matrix.Scaling(m_FullScreen ? Vector3.One : m_Scale);
            m_TransformMatrix *= Matrix.RotationQuaternion(m_Rotation);
            m_TransformMatrix *= Matrix.Translation(m_FullScreen ? Vector3.Zero : m_Position);

            m_ConstantBuffer = new ConstantBufferStruct() {
                TransformMatrix = m_TransformMatrix,
                Type = new Vector4((float)m_Type, 0, 0, 0),
            };

            ConstBuffer = new Buffer(
                Engine.Instance.Device,
                Utilities.SizeOf<ConstantBufferStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
        }

        internal struct ConstantBufferStruct
        {
            public Matrix TransformMatrix;
            public Vector4 CameraPosition;
            public Vector4 Type;
        }
        private ConstantBufferStruct m_ConstantBuffer;
        
        internal struct LightBufferStruct
        {
            public Matrix viewProjMatrix;
            public Vector4 lightTint;
            public float type;
            public Vector3 position;
            public Vector3 direction;
            public float distanceSqr;
        }
        private LightBufferStruct[] m_LightBuffer;
        
        public void Draw(GBuffer gBuffer) {
            Engine.Instance.Context.InputAssembler.InputLayout = m_InputLayout;

            Engine.Instance.Context.VertexShader.Set(m_VertexShader);
            Engine.Instance.Context.PixelShader.Set(m_PixelShader);

            m_ConstantBuffer.CameraPosition = new Vector4(Engine.Instance.MainCamera.gameObject.transform.WorldPosition, 0);

            if (Engine.Instance.MainLight)
            {
                m_LightBuffer[0].viewProjMatrix = Engine.Instance.MainLight.ViewProjMatrix;
                m_LightBuffer[0].lightTint = Engine.Instance.MainLight.LightColor;
                m_LightBuffer[0].type = (float)Engine.Instance.MainLight.Type;
                m_LightBuffer[0].position = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.gameObject.transform.WorldPosition : Vector3.Zero;
                m_LightBuffer[0].direction = Engine.Instance.MainLight != null
                    ? Matrix.RotationQuaternion(Engine.Instance.MainLight.gameObject.transform.WorldRotation).Forward
                    : Vector3.Zero;
                m_LightBuffer[0].distanceSqr = Engine.Instance.MainLight != null
                    ? Engine.Instance.MainLight.radius * Engine.Instance.MainLight.radius : 0;
            }

            Engine.Instance.Context.UpdateSubresource(ref m_ConstantBuffer, ConstBuffer);
            Engine.Instance.Context.UpdateSubresource(m_LightBuffer, LightBuffer);

            Engine.Instance.Context.VertexShader.SetConstantBuffer(0, ConstBuffer);
            Engine.Instance.Context.PixelShader.SetConstantBuffer(0, ConstBuffer);
            Engine.Instance.Context.PixelShader.SetConstantBuffer(1, LightBuffer);

            Engine.Instance.Context.PixelShader.SetSampler(0, m_SamplerState);
            for (int i = 0; i < gBuffer.TargetsCount; i++) {
                Engine.Instance.Context.PixelShader.SetShaderResource(i, gBuffer.shaderResourceViews[i]);
            }
            //Radiance
            Engine.Instance.Context.PixelShader.SetShaderResource(5, Material.GetSkySphereMaterial().albedoMapView);
            //Irradiance
            Engine.Instance.Context.PixelShader.SetShaderResource(6, Material.IrradianceMap);

            //Shadow map
            Engine.Instance.Context.PixelShader.SetSampler(1, Engine.Instance.MainLight.SamplerState);
            Engine.Instance.Context.PixelShader.SetShaderResource(7, Engine.Instance.MainLight.ShaderResourceView);

            Engine.Instance.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Engine.Instance.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(m_VertexBuffer, 32, 0));
            Engine.Instance.Context.InputAssembler.SetIndexBuffer(m_IndexBuffer, Format.R32_UInt, 0);

            bool isWired = Engine.Instance.IsWireframe;
            Engine.Instance.SetSolidRender();

            Engine.Instance.Context.DrawIndexed(6, 0, 0);
            if (isWired) {
                Engine.Instance.SetWireframeRender();
            }
        }
    }
}
