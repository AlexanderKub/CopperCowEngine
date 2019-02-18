#ifndef __DEPENDENCY_HLSL__
#define __DEPENDENCY_HLSL__
struct ConstBuffer
{
    float4x4 viewProjMatrix;
    float4x4 worldMatrix;
    float4 cameraPosition;
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