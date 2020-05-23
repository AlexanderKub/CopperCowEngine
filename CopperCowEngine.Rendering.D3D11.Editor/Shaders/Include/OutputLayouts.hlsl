#ifndef __DEPENDENCY_HLSL_OUTPUT_LAYOUTS__
#define __DEPENDENCY_HLSL_OUTPUT_LAYOUTS__

struct OutputStruct1
{
    float4 Target0 : SV_Target0;
};

struct OutputStruct
{
    float4 Target0 : SV_Target0;
    float3 Target1 : SV_Target1;
};
#endif