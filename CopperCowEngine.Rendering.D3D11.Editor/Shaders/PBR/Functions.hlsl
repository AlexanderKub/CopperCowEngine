#include "../Include/Constants.hlsl"
#include "../Include/Math.hlsl"

#ifndef __DEPENDENCY_HLSL_PBR_FUNCTIONS__
#define __DEPENDENCY_HLSL_PBR_FUNCTIONS__

float3 DiffuseLambert(float3 DiffuseColor)
{
    return DiffuseColor * ONE_OVER_PI;
}

// GGX/Towbridge-Reitz normal distribution function.
// Uses Disney's reparametrization of alpha = roughness^2.
float NormDistGGX(float NoH, float alpha2)
{
    float denom = (NoH * NoH) * (alpha2 - 1.0) + 1.0;
    return alpha2 / (PI * denom * denom);
}

// Single term for separable Schlick-GGX below.
float gaSchlickG1(float cosTheta, float k)
{
    return cosTheta / (cosTheta * (1.0 - k) + k);
}

// Schlick-GGX approximation of geometric attenuation function using Smith's method.
float GeomAttenSchlickGGX(float cosLi, float cosLo, float roughness)
{
    float r = roughness + 1.0;
     // Epic suggests using this roughness remapping for analytic lights.
    float k = (r * r) / 8.0;
    return gaSchlickG1(cosLi, k) * gaSchlickG1(cosLo, k);
}

float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + float3(1.0 - F0) * pow(1.0 - cosTheta, 5.0);
    //pow(2.0, (-5.55473 * cosTheta - 6.98316) * cosTheta);
}

float3 FresnelSchlick(float cosTheta, float3 F0, float3 F90)
{
    return F0 + float3(F90 - F0) * pow(2.0, (-5.55473 * cosTheta - 6.98316) * cosTheta);
}
#endif