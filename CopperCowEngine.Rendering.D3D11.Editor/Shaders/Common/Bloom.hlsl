#include "../Include/Constants.hlsl"
#include "../Include/Structures.hlsl"

cbuffer cbDownScaleBuffer : register(b0)
{
    DownScaleConstants cbDownScale;
}

Texture2D<float4> HDRDownScaleTex : register(t0);
StructuredBuffer<float> AvgLum : register(t1);
RWTexture2D<float4> Bloom : register(u0);

[numthreads(1024, 1, 1)]
void BrightPass(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    uint2 CurPixel = uint2(dispatchThreadId.x % cbDownScale.Res.x, dispatchThreadId.x / cbDownScale.Res.x);
    // Skip out of bound pixels   
    if (CurPixel.y < cbDownScale.Res.y)
    {
        float4 color = HDRDownScaleTex.Load(int3(CurPixel, 0));
        float Lum = dot(color.rgb, LUM_FACTOR);
        // Find the color scale     
        float colorScale = saturate(Lum - AvgLum[0] * cbDownScale.fBloomThreshold);
        // Store the scaled bloom value     
        Bloom[CurPixel.xy] = color * colorScale;
    }
}

