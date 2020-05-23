#ifndef __DEPENDENCY_HLSL_IBL_COMMON__
#define __DEPENDENCY_HLSL_IBL_COMMON__
static const float PI = 3.14159265359;
static const float TwoPI = 2 * PI;

float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

float2 SampleHammersley(uint i, float invNumSamples)
{
    return float2(i * invNumSamples, RadicalInverse_VdC(i));
}

float3 SampleGGX(float u1, float u2, float roughness)
{
    float alpha = roughness * roughness;

    float cosTheta = sqrt((1.0 - u2) / (1.0 + (alpha * alpha - 1.0) * u2));
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
    float phi = TwoPI * u1;

    return float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
}

float GaSchlickG1(float cosTheta, float k)
{
    return cosTheta / (cosTheta * (1.0 - k) + k);
}

float GaSchlickGGX_IBL(float cosLi, float cosLo, float roughness)
{
    float r = roughness;
    float k = (r * r) / 2.0;
    return GaSchlickG1(cosLi, k) * GaSchlickG1(cosLo, k);
}

float NdfGGX(float cosLh, float roughness)
{
    float alpha = roughness * roughness;
    float alphaSq = alpha * alpha;

    float denom = (cosLh * cosLh) * (alphaSq - 1.0) + 1.0;
    return alphaSq / (PI * denom * denom);
}

float3 SampleHemisphere(float u1, float u2)
{
    const float u1p = sqrt(max(0.0, 1.0 - u1 * u1));
    return float3(cos(TwoPI * u2) * u1p, sin(TwoPI * u2) * u1p, u1);
}

float3 GetSamplingVector(uint3 ThreadID, RWTexture2DArray<float4> outputTexture)
{
    float outputWidth, outputHeight, outputDepth;
    outputTexture.GetDimensions(outputWidth, outputHeight, outputDepth);

    float2 st = ThreadID.xy / float2(outputWidth, outputHeight);
    float2 uv = 2.0 * float2(st.x, 1.0 - st.y) - 1.0;

    float3 ret = 0;
    switch (ThreadID.z)
    {
        case 0:
            ret = float3(-1.0, uv.y, -uv.x);
            break;
        case 1:
            ret = float3(1.0, uv.y, uv.x);
            break;
        case 2:
            ret = float3(-uv.x, 1.0, -uv.y);
            break;
        case 3:
            ret = float3(-uv.x, -1.0, uv.y);
            break;
        case 4:
            ret = float3(-uv.x, uv.y, 1.0);
            break;
        case 5:
            ret = float3(uv.x, uv.y, -1.0);
            break;
    }
    return normalize(ret);
}

void ComputeBasisVectors(const float3 N, const float epsilon, out float3 S, out float3 T)
{
    T = cross(N, float3(0.0, 1.0, 0.0));
    T = lerp(cross(N, float3(1.0, 0.0, 0.0)), T, step(epsilon, dot(T, T)));

    T = normalize(T);
    S = normalize(cross(N, T));
}

float3 TangentToWorld(const float3 v, const float3 N, const float3 S, const float3 T)
{
    return S * v.x + T * v.y + N * v.z;
}
#endif
