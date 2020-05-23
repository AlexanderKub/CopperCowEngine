#include "../Include/Samplers.hlsl"
#include "../Include/ConstantBuffers.hlsl"
#include "../Include/Layouts.hlsl"
#include "../Include/VelocityLayouts.hlsl"
#include "../Include/OutputLayouts.hlsl"
#include "../Include/Math.hlsl"

Texture2D g_TxDiffuse : register(t0);

// START OF VERTEX SHADER //
#if VELOCITY
#if MASKED
VELOCITY_POSITION_AND_UV_PS_IN
#else
VELOCITY_POSITION_ONLY_PS_IN
#endif
#else
#if MASKED
COMMON_POSITION_AND_UV_PS_IN
#else 
COMMON_POSITION_ONLY_PS_IN
#endif
#endif
VSMain(COMMON_VS_IN Input)
{
#if VELOCITY
#if MASKED
    VELOCITY_POSITION_AND_UV_PS_IN Output = (VELOCITY_POSITION_AND_UV_PS_IN) 0;
#else
    VELOCITY_POSITION_ONLY_PS_IN Output = (VELOCITY_POSITION_ONLY_PS_IN) 0;
#endif
#else
#if MASKED
    COMMON_POSITION_AND_UV_PS_IN Output = (COMMON_POSITION_AND_UV_PS_IN) 0;
#else
    COMMON_POSITION_ONLY_PS_IN Output = (COMMON_POSITION_ONLY_PS_IN) 0;
#endif
#endif
    
    Output.Position = mul(float4(Input.pos, 1), cbPerObject.WorldViewProjection);
#if VELOCITY
    Output.CurPosition = Output.Position;
    Output.PrevPosition = mul(float4(Input.pos, 1), cbPerObjectPrevious.PreviousWorldViewProjection);
#endif
#if MASKED
    Output.TextureUV = Input.uv0.xy * cbPerMaterial.TextureTiling + cbPerMaterial.TextureShift;
#endif
    return Output;
};
// END OF VERTEX SHADER //

float4 GetVelocity(float4 pos, float4 prevPos)
{
    float2 a = (pos.xyz / pos.w).xy;
    float2 b = (prevPos.xyz / prevPos.w).xy;
    float2 velocity = Pow3((a - b) * 0.5f + 0.5f);
    return float4(velocity, 1.0f, 1.0f);
}

// START OF PIXEL SHADER //
#if VELOCITY
#if MASKED
OutputStruct1 PSMain(VELOCITY_POSITION_AND_UV_PS_IN Input)
#else
OutputStruct1 PSMain(VELOCITY_POSITION_ONLY_PS_IN Input)
#endif
#else
OutputStruct1 PSMain(COMMON_POSITION_AND_UV_PS_IN Input)
#endif
{
    OutputStruct1 OUT = (OutputStruct1) 0;
#if MASKED
    float alpha = g_TxDiffuse.Sample(BilinearWrapSampler, Input.TextureUV).a;
    if (alpha < cbPerMaterial.AlphaClip)
    {
        discard;
    }
#endif
    
#if VELOCITY
    OUT.Target0 = GetVelocity(Input.CurPosition, Input.PrevPosition);
#else
    OUT.Target0 = 1;
#endif
    return OUT;
}
// END OF VERTEX SHADER //