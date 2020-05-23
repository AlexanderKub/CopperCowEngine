#ifndef __DEPENDENCY_HLSL_LIGHTS_STRUCTURES__
#define __DEPENDENCY_HLSL_LIGHTS_STRUCTURES__

struct LightParams
{
    float Type;
    float3 Params;
    
    float3 Center;
    float InverseRange;
    
    float3 Color;
    float Intensity;
};

struct Material
{
    float4 Albedo;
    float3 NormalWS;
    float Unlit;
    
    float NonShadow;
    float Roughness;
    float Metallic;
    float Occlusion;
    float4 Emissive;
    
    float3 PositionWS;
    uint LightsCount;
    float3 V;
    float Filler;
};
#endif