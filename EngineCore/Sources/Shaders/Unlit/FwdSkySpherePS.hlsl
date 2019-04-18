#include "../Include/Layouts.hlsl"
SamplerState Sampler : register(s0);
TextureCube DiffuseMap : register(t0);

float4 PSMain(COMMON_PS_IN Input) : SV_Target
{
    return DiffuseMap.Sample(Sampler, -Input.normal.xyz);
}
