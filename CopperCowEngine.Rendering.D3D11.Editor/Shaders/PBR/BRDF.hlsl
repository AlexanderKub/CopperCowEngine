#include "../Include/Constants.hlsl"
#include "../Include/Math.hlsl"
#include "../Include/Material.hlsl"
#include "../Include/Samplers.hlsl"
#include "Functions.hlsl"

#ifndef __DEPENDENCY_HLSL_BRDF__
#define __DEPENDENCY_HLSL_BRDF__

TextureCube PrefilteredEnvMap : register(t6);
TextureCube IrradianceEnvMap : register(t7);
Texture2D<float2> BRDFxLUT : register(t8);

float3 Shading(const PerPixelBrdfContext pixelContext, const PerLightBrdfContext lightContext, 
    const PerLight light, const PerMaterial material)
{
    float D = NormDistGGX(lightContext.NoH, pixelContext.Roughness2);
    float G = GeomAttenSchlickGGX(pixelContext.NoV, lightContext.NoL, pixelContext.Roughness);
    float3 F = FresnelSchlick(lightContext.VoH, pixelContext.F0);
    
    float3 kd = lerp(1 - F, 0, material.Metallic);
    float3 diff = kd * DiffuseLambert(material.Albedo);
    
    float3 specular = (F * (D * G)) / max(4.0 * lightContext.NoL * pixelContext.NoV, EPSILON);
    
    return (diff + specular) * lightContext.NoL * (light.Color * light.Intensity);
}

//#define MAX_REFLECTION_MIP 8
uint queryTextureLevels()
{
    uint width, height, levels;
    PrefilteredEnvMap.GetDimensions(0, width, height, levels);
    return levels; //MAX_REFLECTION_MIP
}

float3 IBLLight(const PerPixel pixel, const PerPixelBrdfContext pixelContext, const PerMaterial material)
{
    float3 R = 2 * pixelContext.NoV * pixel.N - pixel.V;
    float3 F = FresnelSchlick(pixelContext.NoV, pixelContext.F0);
    float3 kd = lerp(1.0 - F, 0.0, material.Metallic);
    
    float3 irradiance = IrradianceEnvMap.Sample(IBLSampler, pixel.N).rgb;
    float3 diff = kd * material.Albedo * irradiance;
    
    uint specularTextureLevels = queryTextureLevels();
    float mip = specularTextureLevels * material.Roughness * (2 - material.Roughness);
    
    float3 prefiltered = PrefilteredEnvMap.SampleLevel(IBLSampler, R, mip).rgb;
    
    float2 specularBRDF = BRDFxLUT.Sample(IBLSampler, float2(pixelContext.NoV, material.Roughness)).rg;
    float3 specular = prefiltered * (pixelContext.F0 * specularBRDF.x + specularBRDF.y);
    
    return diff + specular;
}
#endif