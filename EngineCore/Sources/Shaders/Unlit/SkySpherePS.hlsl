#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

SamplerState Sampler : register(s0);
TextureCube DiffuseMap : register(t0);

PixelOutputType PSMain(COMMON_PS_IN Input)
{
    PixelOutputType output;
    output.albedo = saturate(DiffuseMap.Sample(Sampler, -Input.normal.xyz));
    output.position = Input.posWS;
    output.normal = float4(Input.normal.xyz, 1.0f);
    float depth = Input.pos.z / Input.pos.w;
    output.roughnessMetallicDepth = float4(0.0f, 0.0f, depth, 1.0f);
    output.occlusionUnlitNonShadow = float4(1.0f, cbPerObject.optionsMask1.b, cbPerObject.optionsMask1.a, 1.0f);
    return output;
}
