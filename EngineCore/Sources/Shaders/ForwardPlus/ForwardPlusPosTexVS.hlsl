#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

COMMON_POSITION_AND_UV_PS_IN VSMain(COMMON_VS_IN Input)
{
    COMMON_POSITION_AND_UV_PS_IN Output;
    
    Input.pos.w = 1.0f;
    Output.Position = mul(Input.pos, cbPerObject.WorldViewProjMatrix);
    Output.PrevPosition = mul(Input.pos, cbPerObject.PreviousWorldViewProjMatrix);

    Output.TextureUV = Input.uv0.xy;
    Output.TextureUV.x *= cbPerObject.textureTiling.x;
    Output.TextureUV.x += cbPerObject.textureShift.x;
    Output.TextureUV.y *= cbPerObject.textureTiling.y;
    Output.TextureUV.y += cbPerObject.textureShift.y;
    
    return Output;
}