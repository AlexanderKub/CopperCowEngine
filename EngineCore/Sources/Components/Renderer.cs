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

        public VertexShader VertexShader;
        public PixelShader PixelShader;

        public InputLayout layout;

        public Buffer VertexBuffer;
        public Buffer IndexBuffer;

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

        public ModelGeometry Geometry { get; set; }

        internal Renderer() { }

        public override void Init() {
            if (RendererMaterial == null) {
                RendererMaterial = Material.DefaultMaterial;
            }
            if (Geometry == null) {
                Geometry = Primitives.Cube();
            }
            Engine.Instance.RendererTechniqueRef.InitRenderer(this);
        }

        public void SetMeshAndMaterial(ModelGeometry Mesh, Material material)
        {
            RendererMaterial = material;
            UpdateMesh(Mesh);
        }

        public void UpdateMesh(ModelGeometry Mesh) {
            Geometry = Mesh;
            UpdateMesh();
        }

        public void UpdateMesh() {
            Engine.Instance.RendererTechniqueRef.InitRenderer(this);
        }

        public override void Draw() {
            Engine.Instance.RendererTechniqueRef.RenderItem(this);
        }
        
        public override void Destroy() {
            VertexShader?.Dispose();
            VertexShader = null;
            PixelShader?.Dispose();
            PixelShader = null;

            VertexBuffer?.Dispose();
            VertexBuffer = null;
            IndexBuffer?.Dispose();
            IndexBuffer = null;

            layout?.Dispose();
            layout = null;
        }
    }
}
