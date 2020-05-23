#include "PBR_PreComputeCommon.hlsl"

Texture2D InputTexture : register(t0);
RWTexture2DArray<float4> OutputTexture : register(u0);
SamplerState DefaultSampler : register(s0);

[numthreads(32, 32, 1)]
void CSMain(uint3 ThreadID : SV_DispatchThreadID)
{
    float3 v = GetSamplingVector(ThreadID, OutputTexture);
	
    float phi = atan2(v.z, v.x);
    float theta = acos(v.y);

    float4 color = InputTexture.SampleLevel(DefaultSampler, float2(phi / TwoPI, theta / PI), 0);

    OutputTexture[ThreadID] = color;
}