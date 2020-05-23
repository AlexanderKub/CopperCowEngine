#include "PBR_PreComputeCommon.hlsl"
static const float Epsilon = 0.001;
static const uint NumSamples = 1024;
static const float InvNumSamples = 1.0 / float(NumSamples);
//static const uint NumSamplesCloth = 4096;
//static const float InvNumSamplesCloth = 1.0 / float(NumSamplesCloth);

RWTexture2D<float1> LUT : register(u0);

/*float VisibilityAshikhmin(float NoV, float NoL) 
{
    return 1 / (4 * (NoL + NoV - NoL * NoV));
}

float DistributionCharlie(float NoH, float linearRoughness)
{
    float a = linearRoughness;
    float invAlpha = 1 / a;
    float cos2h = NoH * NoH;
    float sin2h = 1 - cos2h;
    return (2.0f + invAlpha) * pow(sin2h, invAlpha * 0.5f) / (2.0f * (float) PI);
}*/

[numthreads(32, 32, 1)]
void CSMain(uint2 ThreadID : SV_DispatchThreadID)
{
    float outputWidth, outputHeight;
    LUT.GetDimensions(outputWidth, outputHeight);

    float cosLo = ThreadID.x / outputWidth;
    float roughness = 1.0 - ThreadID.y / outputHeight;

    cosLo = max(cosLo, Epsilon);

    float3 Lo = float3(sqrt(1.0 - cosLo * cosLo), 0.0, cosLo);

    float DFG1 = 0;
    float DFG2 = 0;

    for (uint i = 0; i < NumSamples; ++i)
    {
        float2 u = SampleHammersley(i, InvNumSamples);

        float3 Lh = SampleGGX(u.x, u.y, roughness);

        float3 Li = 2.0 * dot(Lo, Lh) * Lh - Lo;

        float cosLi = Li.z;
        float cosLh = Lh.z;
        float cosLoLh = max(dot(Lo, Lh), 0.0);

        if (cosLi > 0.0)
        {
            float G = GaSchlickGGX_IBL(cosLi, cosLo, roughness);
            float Gv = G * cosLoLh / (cosLh * cosLo);
            float Fc = pow(1.0 - cosLoLh, 5);

            DFG1 += (1 - Fc) * Gv;
            DFG2 += Fc * Gv;
        }
    }
    LUT[ThreadID] = float2(DFG1, DFG2) * InvNumSamples;
    /*
    float DFG3 = 0;
    for (uint j = 0; j < NumSamplesCloth; j++)
    {
        float2 u = SampleHammersley(j, InvNumSamplesCloth);
        
        float3 Lh = SampleHemisphere(u.x, u.y);

        float3 Li = 2.0 * dot(Lo, Lh) * Lh - Lo;

        float cosLi = Li.z;
        float cosLh = Lh.z;
        float cosLoLh = max(dot(Lo, Lh), 0.0);
            
        if (cosLi > 0.0)
        {
            float v = VisibilityAshikhmin(cosLi, cosLo);
            float d = DistributionCharlie(cosLh, roughness);
            DFG3 += v * d * cosLo * cosLoLh;
        }
        
    }

    float2 dfg = float2(DFG1, DFG2) * InvNumSamples;
    LUT[ThreadID] = float4(dfg, DFG3 * InvNumSamplesCloth, 1);*/
}
