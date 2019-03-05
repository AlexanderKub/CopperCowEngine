#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

COMMON_POSITION_ONLY_PS_IN VSMain(COMMON_VS_IN Input)
{
    COMMON_POSITION_ONLY_PS_IN Output;
    
    Input.pos.w = 1.0f;
    Output.Position = mul(Input.pos, cbPerObject.WorldViewProjMatrix);
    
    return Output;
}