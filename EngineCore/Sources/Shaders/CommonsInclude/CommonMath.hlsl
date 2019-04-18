#ifndef __DEPENDENCY_HLSL_COMMONMATH__
#define __DEPENDENCY_HLSL_COMMONMATH__

float Pow2(float x)
{
    return x * x;
}

float2 Pow2(float2 x)
{
    return x * x;
}

float3 Pow2(float3 x)
{
    return x * x;
}

float4 Pow2(float4 x)
{
    return x * x;
}

float Pow3(float x)
{
    return x * x * x;
}

float2 Pow3(float2 x)
{
    return x * x * x;
}

float3 Pow3(float3 x)
{
    return x * x * x;
}

float4 Pow3(float4 x)
{
    return x * x * x;
}

float Pow4(float x)
{
    float xx = x * x;
    return xx * xx;
}

float2 Pow4(float2 x)
{
    float2 xx = x * x;
    return xx * xx;
}

float3 Pow4(float3 x)
{
    float3 xx = x * x;
    return xx * xx;
}

float4 Pow4(float4 x)
{
    float4 xx = x * x;
    return xx * xx;
}

float Pow5(float x)
{
    float xx = x * x;
    return xx * xx * x;
}

float2 Pow5(float2 x)
{
    float2 xx = x * x;
    return xx * xx * x;
}

float3 Pow5(float3 x)
{
    float3 xx = x * x;
    return xx * xx * x;
}

float4 Pow5(float4 x)
{
    float4 xx = x * x;
    return xx * xx * x;
}

float Pow6(float x)
{
    float xx = x * x;
    return xx * xx * xx;
}

float2 Pow6(float2 x)
{
    float2 xx = x * x;
    return xx * xx * xx;
}

float3 Pow6(float3 x)
{
    float3 xx = x * x;
    return xx * xx * xx;
}

float4 Pow6(float4 x)
{
    float4 xx = x * x;
    return xx * xx * xx;
}

float ClampedPow(float X, float Y)
{
    return pow(max(abs(X), 0.000001f), Y);
}

float PhongShadingPow(float X, float Y)
{
    return ClampedPow(X, Y);
}
#endif // __DEPENDENCY_HLSL_COMMONMATH__