#include "../Include/Constants.hlsl"

#ifndef __DEPENDENCY_HLSL_COLOR_SPACE__
#define __DEPENDENCY_HLSL_COLOR_SPACE__

float3 LinearToSRGB(float3 color)
{
    return pow(abs(color), 1 / 2.2);
}

float3 SRGBToLinear(float3 srgb)
{
    return pow(abs(srgb), 2.2);
}
#endif