#ifndef __DEPENDENCY_HLSL_STRUCTURES__
#define __DEPENDENCY_HLSL_STRUCTURES__
struct CBufferPerObjectStruct
{
    float4x4 WorldViewProjMatrix;
    float4x4 WorldViewMatrix;
    float4x4 WorldMatrix;
    float2 textureTiling;
    float2 textureShift;

    float4 AlbedoColor;
    float Roughness;
    float Metallic;
    float2 filler;
    
    //r hasAlbedoMap;
    //g hasNormalMap;
    //b hasRoughnessMap;
    //a hasMetallicMap;
    float4 optionsMask0;
    
    //r hasOcclusionMap;
    //g unlit;
    //b nonRecieveShadows;
    float4 optionsMask1;
};

struct CBufferPerFrameStruct
{
    float4x4 Projection;
    float4x4 ProjectionInv;
    float3 CameraPos;
    float AlphaTest;
    uint NumLights;
    uint WindowWidth;
    uint WindowHeight;
    uint MaxNumLightsPerTile;
    uint DirLightNum;
    float3 filler;
};

struct CBufferDirLightStruct
{
    float3 DirLightDirection;
    float DirLightIntensity;
    float4 DirLightColor;
};


//TODO: refactoring
struct LightBuffer
{
    float4x4 lightViewProjMatrix;
    float4 lightTint;
    float type;
    float3 position;
    float3 direction;
    float distanceSqr;
};

struct PixelOutputType
{
    float4 albedo : SV_Target0;
    float4 position : SV_Target1;
    float4 normal : SV_Target2;
    float4 roughnessMetallicDepth : SV_Target3;
    float4 occlusionUnlitNonShadow : SV_Target4;
};
#endif