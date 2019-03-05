using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.RenderTechnique
{
    public enum RenderPath
    {
        Forward,
        ForwardPlus,
        Deffered,
    };

    public class CommonStructs
    {

        public struct ConstBufferPerObjectStruct
        {
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
            public Vector3 CameraPos;
            public float AlphaTest;
            public uint NumLights;
            public uint WindowWidth;
            public uint WindowHeight;
            public uint MaxNumLightsPerTile;
            public uint DirLightsNum;
            public Vector3 filler;
        }

        public struct ConstBufferDirLightStruct
        {
            public Vector3 DirLightDirection;
            public float DirLightIntensity;
            public Vector4 DirLightColor;
        }
    }
}
