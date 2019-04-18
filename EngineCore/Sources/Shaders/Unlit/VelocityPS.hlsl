#include "../Include/Layouts.hlsl"
#include "../Include/Structures.hlsl"

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

float4 PSMain(COMMON_POSITION_ONLY_PS_IN Input) : SV_TARGET
{
    float2 a = Input.Position.xy / Input.Position.w;
    float2 b = Input.PrevPosition.xy / Input.PrevPosition.w;
    float2 velocity = (a - b) * (1.0f / cbPerFrame.currentFPS) * (cbPerFrame.currentFPS / 60);
    //a = Input.Velocity.xy * 0.5 + 0.5;
    //b = Input.Velocity.xy * 0.5 + 0.5;
    float4 output = float4(velocity * 0.5 + 0.5, 1, 1);
    return output;
}
