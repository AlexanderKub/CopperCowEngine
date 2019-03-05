#include "../Include/ForwardPlusCommon.hlsl"
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

Texture2D g_TxDiffuse : register(t0);
SamplerState g_Sampler : register(s0);

float4 PSMain(COMMON_POSITION_AND_UV_PS_IN Input) : SV_TARGET
{
    float4 DiffuseTex = g_TxDiffuse.Sample(g_Sampler, Input.TextureUV);
    float fAlpha = min(DiffuseTex.a, cbPerObject.AlbedoColor.a);
    if (fAlpha < cbPerFrame.AlphaTest) {
        discard;
    }    
    return DiffuseTex;
}