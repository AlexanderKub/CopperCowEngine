
#include "../Include/Samplers.hlsl"

#ifndef __DEPENDENCY_HLSL_MATERIAL__
#define __DEPENDENCY_HLSL_MATERIAL__

#define MIN_PERCEPTUAL_ROUGHNESS 0.045
#define MIN_ROUGHNESS 0.002025
#define MIN_N_DOT_V 1e-4

float ClampNoV(float NoV)
{
    return max(NoV, MIN_N_DOT_V);
}

float3 ComputeDiffuseColor(const float3 baseColor, float metallic)
{
    return baseColor.rgb * (1.0 - metallic);
}

float3 ComputeF0(const float3 baseColor, float metallic, float reflectance)
{
    return lerp(reflectance, baseColor.rgb, metallic);
}

float ComputeDielectricF0(float reflectance)
{
    return 0.16 * reflectance * reflectance;
}

float PerceptualRoughnessToRoughness(float perceptualRoughness)
{
    return clamp(perceptualRoughness * perceptualRoughness, MIN_ROUGHNESS, 1);
}

struct PerPixel
{
    float3 N;
    float3 V;
    float3 PositionWS;
};

struct PerLight
{
    float3 L;
    float3 Color;
    float Intensity;
};

struct PerMaterial
{
    float3 Albedo;
    float Alpha;
    float Roughness;
    float Metallic;
    float Reflectance;
    float Occlusion;
    float4 Emissive;
};

struct PerPixelBrdfContext
{
    float NoV;
    float Roughness;
    float Roughness2;
    float3 F0;
};

struct PerLightBrdfContext
{
    float3 L;
    float3 H;
    float NoL;
    float NoH;
    float LoH;
    float VoH;
};

void PrepareBrdfContext(PerPixel pixel, PerMaterial material, inout PerPixelBrdfContext context)
{
    context.NoV = ClampNoV(dot(pixel.N, pixel.V));
    float perceptualRoughness = clamp(material.Roughness, MIN_PERCEPTUAL_ROUGHNESS, 1);
    context.Roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    context.Roughness2 = context.Roughness * context.Roughness;
    float reflectance = ComputeDielectricF0(ComputeDielectricF0(material.Reflectance));
    context.F0 = ComputeF0(material.Albedo, material.Metallic, reflectance);
}

void PrepareBrdfContext(PerPixel pixel, PerLight light, inout PerLightBrdfContext context)
{
    context.L = light.L;
    context.H = normalize(context.L + pixel.V);
    context.NoL = saturate(dot(pixel.N, context.L));
    context.NoH = saturate(dot(pixel.N, context.H));
    context.LoH = saturate(dot(context.L, context.H));
    context.VoH = saturate(dot(pixel.N, context.H));
}
#endif