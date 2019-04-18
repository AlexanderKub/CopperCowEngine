using System.Diagnostics;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using AssetsManager.Loaders;

namespace EngineCore
{

    public enum SpecificTypeEnum
    {
        None,
        SkySphere,
        ReflectionSphere,
        Unlit,
        Wireframe,
    }
    //public SpecificTypeEnum SpecificType;
    /*public MaterialPropetyBlock GetPropetyBlock {
        get {
            if (CustomPropertyBlock == null)
            {
                return RendererMaterial.PropetyBlock;
            }
            return CustomPropertyBlock;
        }
    }*/

    /*
    if (RotateAroundPivot != Vector3.Zero) {
        TransformMatrix *= Matrix.Translation(-RotateAroundPivot);
        TransformMatrix *= Matrix.RotationQuaternion(m_RelativeRotation);
        TransformMatrix *= Matrix.Translation(RotateAroundPivot);
    }
    */
}