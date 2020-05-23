using System.Numerics;
using System.Runtime.InteropServices;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths.BrandNewCommonStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameConstBufferStruct
    {
        public Matrix4x4 View;
        public Matrix4x4 Projection;
        public Vector3 CameraPosition;
        public float FrameTime;
        public Vector4 PerspectiveValues;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameInverseConstBufferStruct
    {
        public Matrix4x4 InverseView;
        public Matrix4x4 InverseProjection;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PerFramePreviousConstBufferStruct
    {
        public Matrix4x4 PreviousViewProjection;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerMaterialConstBufferStruct
    {
        public Vector4 AlbedoColor;
        public Vector4 EmissiveColor;

        public float RoughnessValue;
        public float MetallicValue;
        public float ReflectanceValue;
        public float Unlit;

        public Vector2 TextureTiling;
        public Vector2 TextureShift;
        
        //r hasAlbedoMap;
        //g hasNormalMap;
        //b hasMetallicMap;
        //a hasRoughnessMap;
        public Vector4 OptionsMask0;
        
        //r hasOcclusionMap
        //g hasEmissiveMap
        //b unlit
        //a nonReceiveShadows
        public Vector4 OptionsMask1;

        public float AlphaClip;
        private Vector3 _filler;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectConstBufferStruct
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldViewProjection;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectPreviousConstBufferStruct
    {
        public Matrix4x4 PreviousWorldViewProjection;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectInverseConstBufferStruct
    {
        public Matrix4x4 InverseWorldViewProjection;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct DownScaleConstStruct
    {
        public uint ResX;
        public uint ResY;
        public uint Domain;
        public uint GroupSize;
        public float AdaptationGreater;
        public float AdaptationLower;
        public float BloomThreshold;
        private float _filler;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct PerLightBatchConstBufferStruct
    {
        public uint LightIndex0;
        public uint LightIndex1;
        public uint LightIndex2;
        public uint LightIndex3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PostProcessBufferStruct
    {
        public float MiddleGrey;
        public float MinLuminance;
        public float MaxLuminance;
        public float ExposurePow;
    
        public float NumeratorMultiplier;
        public float ToneMapA;
        public float ToneMapB;
        public float ToneMapC;
    
        public float ToneMapD;
        public float ToneMapE;
        public float ToneMapF;
        public float BloomScale;

        public Vector4 DOFFarValues;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct LightParamsBufferStruct
    {
        public float Type;
        public Vector3 Params;
        public Vector3 Center;
        public float InverseRange;
        public Vector3 Color;
        public float Intensity;
    }
}
