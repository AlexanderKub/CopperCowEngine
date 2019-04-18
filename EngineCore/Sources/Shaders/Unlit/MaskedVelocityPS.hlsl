#include "../Include/Layouts.hlsl"
#include "../Include/Structures.hlsl"

Texture2D g_TxDiffuse : register(t0);
SamplerState g_Sampler : register(s0);

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

float4 PSMain(COMMON_POSITION_AND_UV_PS_IN Input) : SV_TARGET
{
    float4 diffuse = g_TxDiffuse.Sample(g_Sampler, Input.TextureUV);
    if (diffuse.a < cbPerFrame.AlphaTest)
        discard;

    
    float2 a = Input.Position.xy / Input.Position.w;
    float2 b = Input.PrevPosition.xy / Input.PrevPosition.w;
    float2 velocity = (a - b) * (1.0f / cbPerFrame.currentFPS) * (cbPerFrame.currentFPS / 60);
    float4 output = float4(velocity * 0.5 + 0.5, 1, 1);
    return output;
}
