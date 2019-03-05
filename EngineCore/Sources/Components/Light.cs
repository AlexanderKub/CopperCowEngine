using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace EngineCore
{
    public class Light : Component {
        public static int ShadowMapResolution = 4096;

        public enum LightType
        {
            Directional,
            Point,
            Spot,
        };
        public Vector4 LightColor = Vector4.One;
        public float LightIntensity = 1.0f;

        public float radius;
        public LightType Type;
        public bool EnableShadows;

        public Matrix ViewProjMatrix { protected set; get; }
        public Matrix ProjMatrix = Matrix.OrthoLH(30f, 30f, 0.01f, 500f);

        public Vector3 Direction {
            get {
                return Matrix.RotationQuaternion(gameObject.transform.WorldRotation).Forward;
            }
        }
        
        private Matrix m_ViewMatrix;
        public VertexShader VertexShader;
        public Buffer ContantBuffer;
        public DepthStencilView DepthStencilView;
        public InputLayout InputLayout;
        public ShaderResourceView ShaderResourceView;
        public SamplerState SamplerState;

        public override void Init() {
            Engine.Instance.RendererTechniqueRef.InitLight(this);
        }

        public override void Update() {
            base.Update();

            m_ViewMatrix = Matrix.LookAtLH(
                this.gameObject.transform.WorldPosition,
                Vector3.Zero,
                Vector3.Up
            );
            /*Vector3 Direction = new Vector3(
                2 * (gameObject.transform.WorldRotation.X * gameObject.transform.WorldRotation.Z +
                gameObject.transform.WorldRotation.W * gameObject.transform.WorldRotation.Y),
                2 * (gameObject.transform.WorldRotation.Y * gameObject.transform.WorldRotation.Z -
                gameObject.transform.WorldRotation.W * gameObject.transform.WorldRotation.X),
                1 - 2 * (gameObject.transform.WorldRotation.X * gameObject.transform.WorldRotation.X +
                gameObject.transform.WorldRotation.Y * gameObject.transform.WorldRotation.Y)
            );
            m_ViewMatrix =  Matrix.LookAtLH(
                this.gameObject.transform.WorldPosition,
                this.gameObject.transform.WorldPosition + Direction,
                Vector3.Up
            );*/

            ViewProjMatrix = m_ViewMatrix * ProjMatrix;
        }

        public override void Draw() {
            Engine.Instance.RendererTechniqueRef.RenderItemLight(this);
        }

        public override void Destroy()
        {
            VertexShader?.Dispose();
            ContantBuffer?.Dispose();
            DepthStencilView?.Dispose();
            InputLayout?.Dispose();
            ShaderResourceView?.Dispose();
            SamplerState?.Dispose();
        }

        public static implicit operator bool(Light foo)
        {
            return !object.ReferenceEquals(foo, null);
        }
    }
}
