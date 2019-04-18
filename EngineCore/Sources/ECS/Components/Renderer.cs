using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.ECS.Components
{
    public class Renderer : IEntityComponent
    {

        public AssetsLoader.MeshInfo meshInfo { get; private set; }
        public AssetsLoader.MaterialInfo materialInfo { get; private set; }

        public void SetMesh(AssetsLoader.MeshInfo info)
        {
            meshInfo = info;
        }

        public void SetMaterial(AssetsLoader.MaterialInfo info)
        {
            materialInfo = info;
        }

        public void SetMeshAndMaterial(AssetsLoader.MeshInfo mesh, AssetsLoader.MaterialInfo material)
        {
            meshInfo = mesh;
            materialInfo = material;
        }

        public BoundsBox Bounds;
        public bool IsDynamic;
    }
}
