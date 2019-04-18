#ifndef __DEPENDENCY_HLSL_LIGHTSURFACE__
#define __DEPENDENCY_HLSL_LIGHTSURFACE__
#include "../../CommonsInclude/Constants.hlsl"
#include "./Structures.hlsl"

float3 Fresnel_Shlick(in float3 f0, in float3 f90, in float x)
{
    return f0 + (f90 - f0) * pow(1.f - x, 5.f);
}

float Specular_D_GGX(in float alpha, in float NdotH)
{
    const float alpha2 = alpha * alpha;
    const float lower = (NdotH * NdotH * (alpha2 - 1)) + 1;
    return alpha2 / max(EPSILON, PI * lower * lower);
}

float G_Shlick_Smith_Hable(float alpha, float LdotH)
{
    return rcp(lerp(LdotH * LdotH, 1, alpha * alpha * 0.25f));
}

float3 Specular_BRDF(in float alpha, in float3 specularColor, in float NdotV, in float NdotL, in float LdotH, in float NdotH)
{
    float specular_D = Specular_D_GGX(alpha, NdotH);
    
    float3 specular_F = Fresnel_Shlick(specularColor, 1, LdotH);
    
    float specular_G = G_Shlick_Smith_Hable(alpha, LdotH);

    return specular_D * specular_F * specular_G;
}

/// ------------------------ Image based lightning -------------------------- ///
#define MAX_REFLECTION_MIP 5
float ComputeCubemapMipFromRoughness(float Roughness, float MipCount)
{
    float Level = 3 - 1.15 * log2(Roughness);
    return MipCount - Level;
}

float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(1.0 - roughness, F0) - F0) * pow(abs(1.0 - cosTheta), 5.0);
}

// Epic Games prefiltered map and integrate BRDF
float3 IBLLightSurface(float3 V, GBufferAttributes attrs,
    TextureCube PrefilteredMap, TextureCube IrradianceMap, Texture2D<float2> BRDFxLUT, 
    SamplerState IBLSampler, SamplerState PreIntegratedSampler)
{
    float NdotV = max(dot(attrs.Normal, V), 0.0);

    //float3 F0 = lerp(0.04, attrs.AlbedoColor, attrs.MetallicValue);
    float3 F0 = lerp(0.08, attrs.AlbedoColor, attrs.MetallicValue);
    float3 albedo = lerp(attrs.AlbedoColor, 0, attrs.MetallicValue);

    float3 F = FresnelSchlickRoughness(NdotV, F0, attrs.RoughnessValue);
    
    float3 R = reflect(-V, attrs.Normal);

    float3 kS = F;
    float3 kD = 1.0 - kS;
    kD *= 1.0 - attrs.MetallicValue;
  
    float3 irradiance = IrradianceMap.Sample(IBLSampler, attrs.Normal).rgb;
    //float3 diffuse = irradiance * albedo;
    float3 diffuse = irradiance * attrs.AlbedoColor;

    float3 prefilteredColor = PrefilteredMap.SampleLevel(IBLSampler, R, MAX_REFLECTION_MIP * attrs.RoughnessValue).rgb;
    //float3 prefilteredColor = PrefilteredMap.SampleLevel(IBLSampler, R, 
    //    ComputeCubemapMipFromRoughness(attrs.RoughnessValue, MAX_REFLECTION_MIP)).rgb;

    float2 envBRDF = BRDFxLUT.Sample(PreIntegratedSampler, float2(NdotV, attrs.RoughnessValue)).rg;

    float3 specular = prefilteredColor * (F * envBRDF.x + envBRDF.y);
  
    //return (diffuse + specular) * attrs.AOValue;
    return (kD * diffuse + specular) * attrs.AOValue;
}

/// ------------------------------------------------------------------------- ///
#endif