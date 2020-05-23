using System;
using System.Collections.Generic;
using System.Text;
using CopperCowEngine.ECS.Builtin.Components;
using CopperCowEngine.Rendering.Loaders;

namespace CopperCowEngine.ECS.Builtin.Extensions
{
    public static class ComponentsExtension
    {
        public static Material CreateMaterial(this MaterialInfo materialInfo )
        {
            return new Material
            {
                AssetGuid = materialInfo.AssetGuid,
                Queue = materialInfo.Queue,
            };
        }

        public static Mesh CreateMesh(this MeshInfo meshInfo)
        {
            return new Mesh
            {
                AssetGuid = meshInfo.AssetGuid,
                Bounds = meshInfo.Bounds,
            };
        }
    }
}
