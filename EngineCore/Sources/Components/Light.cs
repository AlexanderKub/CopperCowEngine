using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore
{
    public class Light : Component {
        public static int ShadowMapResolution = 2048;

        public enum LightType
        {
            Directional,
            Point,
            Spot,
        };
        public Vector4 ambientColor;
        public Vector4 diffuseColor;
        public Vector4 specularColor;
        public float radius;
        public LightType Type;
        public bool EnableShadows;

        public Matrix ViewProjMatrix { protected set; get; }
        public Matrix ProjMatrix;
        private Matrix m_ViewMatrix;

        public VertexShader VertexShader;
        public Buffer ContantBuffer;
        public DepthStencilView DepthStencilView;
        public InputLayout InputLayout;
        public ShaderResourceView ShaderResourceView;
        public SamplerState SamplerState;

        public override void Init() {
            Engine.Instance.RendererTechnique.InitLight(this);
        }

        public override void Update() {
            base.Update();

            m_ViewMatrix = Matrix.LookAtLH(
                this.gameObject.transform.Position,
                Vector3.Zero,
                Vector3.Up
            );
            ViewProjMatrix = m_ViewMatrix * ProjMatrix;
        }

        public override void Draw() {
            Engine.Instance.RendererTechnique.RenderItemLight(this);
        }

        public override void Destroy()
        {
            if (VertexShader != null) {
                VertexShader.Dispose();
            }
            if (ContantBuffer != null) {
                ContantBuffer.Dispose();
            }
        }
    }
}
