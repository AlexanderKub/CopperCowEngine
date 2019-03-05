#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

SamplerState Sampler : register(s0);
TextureCube DiffuseMap : register(t0);

PixelOutputType PSMain(COMMON_PS_IN Input)
{
    PixelOutputType output;

    float3 viewDirection = normalize(Input.posWS.xyz - cbPerFrame.CameraPos.xyz);
    float3 texcoord = normalize(reflect(viewDirection, normalize(Input.normal.xyz)));

	output.albedo = saturate(DiffuseMap.Sample(Sampler, texcoord));
    output.position = Input.posWS;
    output.normal = Input.normal;
    float depth = Input.pos.z / Input.pos.w;
    output.roughnessMetallicDepth = float4(0.85f, 0.85f, depth, 1.0f);
    output.occlusionUnlitNonShadow = float4(1.0f, 1.0f, 0.0f, 1.0f);
    return output;
}
