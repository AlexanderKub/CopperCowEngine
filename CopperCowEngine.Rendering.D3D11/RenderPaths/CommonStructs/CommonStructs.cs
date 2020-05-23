using System.Numerics;

namespace CopperCowEngine.Rendering.D3D11.RenderPaths.CommonStructs
{
    // DEPRECATED
    public struct ConstBufferPerObjectStruct
    {
        public Matrix4x4 PreviousWorldViewProjMatrix;
        public Matrix4x4 WorldViewProjMatrix;
        public Matrix4x4 WorldViewMatrix;
        public Matrix4x4 WorldMatrix;

        public Vector2 TextureTiling;
        public Vector2 TextureShift;

        public Vector4 AlbedoColor;
        public float RoughnessValue;
        public float MetallicValue;
        public Vector2 Filler;

        //r hasAlbedoMap;
        //g hasNormalMap;
        //b hasRoughnessMap;
        //a hasMetallicMap;
        public Vector4 OptionsMask0;

        //r hasOcclusionMap;
        //g unlit;
        //b nonRecieveShadows;
        public Vector4 OptionsMask1;
    }

    public struct ConstBufferPerFrameStruct
    {
        public Matrix4x4 Projection;
        public Matrix4x4 ProjectionInv;
        public Matrix4x4 ViewInv;
        public Matrix4x4 PreviousView;
        public Vector4 CameraForward;
        public Vector3 CameraPos;
        public float AlphaTest;
        public uint NumLights;
        public uint WindowWidth;
        public uint WindowHeight;
        public uint MaxNumLightsPerTile;
        public uint DirLightsNum;
        public uint CurrentFps;
        public Vector2 Filler;
    }

    public struct ConstBufferDirLightStruct
    {
        public Vector3 DirLightDirection;
        public float DirLightIntensity;
        public Vector4 DirLightColor;
    }

    public struct ConstBufferShadowDepthStruct
    {
        public Matrix4x4 WorldMatrix;
        public Matrix4x4 ViewProjectionMatrix;
    }

    public struct ConstBufferShadowMapLightStruct
    {
        public Matrix4x4 LightViewProjectionMatrix;
        public Vector2 LeftTop;
        public Vector2 RightBottom;
    }

    // NEW
    public struct ConstBufferPerFrameDeferredStruct
    {
        public Matrix4x4 View;
        public Matrix4x4 InverseView;
        public Matrix4x4 Projection;
        public Matrix4x4 InverseProjection;
        public Vector3 CameraPosition;
        public float Fps;
        public Vector4 PerspectiveValues;
    }

    public struct ConstBufferPerObjectDeferredStruct
    {
        public Matrix4x4 World;
        public Matrix4x4 WorldInverse;
    }

    public struct ConstBufferPerMaterialDeferredStruct
    {
        public Vector4 AlbedoColor;
        public Vector4 EmissiveColor;

        public float RoughnessValue;
        public float MetallicValue;
        public float SpecularValue;
        public float Unlit;

        public Vector2 TextureTiling;
        public Vector2 TextureShift;

        //r hasAlbedoMap
        //g hasMetallicMap
        //b hasEmissiveMap
        //a hasRoughnessMap
        public Vector4 OptionsMask0;

        //r hasNormalMap
        //g hasSpecularMap
        //b hasOcclusionMap
        //a nonRecieveShadows
        public Vector4 OptionsMask1;

        public float AlphaClip;
        private Vector3 _filler;
    }

    public struct ConstBufferPerLightStruct
    {
        public Vector3 Direction;
        public uint Type; // 0=Direction, 1=Point, 2=spot
        public Vector3 Position;
        public float Radius;
        public Vector3 Color;
        public float Intensity;
    }

    public struct ConstBufferLightVolumeDomainShader
    {
        public Matrix4x4 LightMatrix;
        public float LightParam1;
        public float LightParam2;
        private Vector2 _filler;
    }

    public struct ConstBufferPostProcessStruct
    {
        public float MiddleGrey;
        public float LumWhiteSqr;
        private Vector2 _filler;
    }

    public static class CommonStructsHelper
    {
        public static Vector4 FloatMaskValue(bool v0, bool v1, bool v2, bool v3)
        {
            return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
        }
    }
}
