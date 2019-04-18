#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

COMMON_PS_IN VSMain(COMMON_VS_IN Input)
{
    COMMON_PS_IN Output = (COMMON_PS_IN) 0;
    
    Input.pos.w = 1.0f;
    Output.pos = mul(Input.pos, cbPerObject.WorldViewProjMatrix);
    Output.posWS = mul(Input.pos, cbPerObject.WorldMatrix);
    
    Output.color = Input.color;

    Output.uv0 = Input.uv0;
    Output.uv0.x *= cbPerObject.textureTiling.x;
    Output.uv0.x += cbPerObject.textureShift.x;
    Output.uv0.y *= cbPerObject.textureTiling.y;
    Output.uv0.y += cbPerObject.textureShift.y;
    Output.uv1 = Input.uv1;

    Output.normal = normalize(mul(float4(Input.normal.xyz, 0), cbPerObject.WorldMatrix));
    Output.tangent = normalize(mul(float4(Input.tangent.xyz, 0), cbPerObject.WorldMatrix));
 
    return Output;
}