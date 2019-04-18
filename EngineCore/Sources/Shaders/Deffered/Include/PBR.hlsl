#ifndef __DEPENDENCY_HLSL_DEFPBR__
#define __DEPENDENCY_HLSL_DEFPBR__
#include "../../CommonsInclude/Constants.hlsl"

float DistributionGGX(in float alpha, in float NdotH)
{
    const float alpha2 = alpha * alpha;
    const float lower = (NdotH * NdotH * (alpha2 - 1)) + 1;
    return alpha2 / max(EPSILON, PI * lower * lower);
}

float GeometrySchlickGGX(in float NdotV, in float k)
{
    return NdotV / lerp(NdotV, 1, k);
}
  
float GeometrySmith(in float3 N, in float3 V, in float3 L, in float k)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx1 = GeometrySchlickGGX(NdotV, k);
    float ggx2 = GeometrySchlickGGX(NdotL, k);
	
    return ggx1 * ggx2;
}
float3 FresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}
#endif