using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.D3D11
{
    public class CommonStructs
    {
        // DEPRECATED
        public struct ConstBufferPerObjectStruct
        {
            public Matrix PreviousWorldViewProjMatrix;
            public Matrix WorldViewProjMatrix;
            public Matrix WorldViewMatrix;
            public Matrix WorldMatrix;

            public Vector2 textureTiling;
            public Vector2 textureShift;

            public Vector4 AlbedoColor;
            public float RoughnessValue;
            public float MetallicValue;
            public Vector2 filler;

            //r hasAlbedoMap;
            //g hasNormalMap;
            //b hasRoughnessMap;
            //a hasMetallicMap;
            public Vector4 optionsMask0;

            //r hasOcclusionMap;
            //g unlit;
            //b nonRecieveShadows;
            public Vector4 optionsMask1;
        }

        public struct ConstBufferPerFrameStruct
        {
            public Matrix Projection;
            public Matrix ProjectionInv;
            public Matrix ViewInv;
            public Matrix PreviousView;
            public Vector4 CameraForward;
            public Vector3 CameraPos;
            public float AlphaTest;
            public uint NumLights;
            public uint WindowWidth;
            public uint WindowHeight;
            public uint MaxNumLightsPerTile;
            public uint DirLightsNum;
            public uint CurrentFPS;
            public Vector2 filler;
        }

        public struct ConstBufferDirLightStruct
        {
            public Vector3 DirLightDirection;
            public float DirLightIntensity;
            public Vector4 DirLightColor;
        }

        public struct ConstBufferShadowDepthStruct
        {
            public Matrix WorldMatrix;
            public Matrix ViewProjectionMatrix;
        }

        public struct ConstBufferShadowMapLightStruct
        {
            public Matrix LightViewProjectionMatrix;
            public Vector2 LeftTop;
            public Vector2 RightBottom;
        }
    
        // NEW
        public struct ConstBufferPerFrameDefferedStruct
        {
            public Matrix View;
            public Matrix InverseView;
            public Matrix Projection;
            public Matrix InverseProjection;
            public Vector3 CameraPosition;
            public float FPS;
            public Vector4 PerspectiveValues;
        }

        public struct ConstBufferPerObjectDefferedStruct
        {
            public Matrix World;
            public Matrix WorldInverse;
        }

        public struct ConstBufferPerMaterialDefferedStruct
        {
            public Vector4 AlbedoColor;
            public Vector4 EmissiveColor;

            public float RoughnessValue;
            public float MetallicValue;
            public float SpecularValue;
            public float Unlit;

            public Vector2 textureTiling;
            public Vector2 textureShift;

            //r hasAlbedoMap
            //g hasMetallicMap
            //b hasEmissiveMap
            //a hasRoughnessMap
            public Vector4 optionsMask0;

            //r hasNormalMap
            //g hasSpecularMap
            //b hasOcclusionMap
            //a nonRecieveShadows
            public Vector4 optionsMask1;

            public float AlphaClip;
            public Vector3 filler;
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
            public Matrix LightMatrix;
            public float LightParam1;
            public float LightParam2;
            private Vector2 filler;
        }

        public struct ConstBufferPostProcessStruct
        {
            public float MiddleGrey;
            public float LumWhiteSqr;
            private Vector2 filler;
        }

        public static Vector4 FloatMaskValue(bool v0, bool v1, bool v2, bool v3)
        {
            return new Vector4(v0 ? 1f : 0, v1 ? 1f : 0, v2 ? 1f : 0, v3 ? 1f : 0);
        }
    }
}
