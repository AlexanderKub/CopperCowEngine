#include "PBR_PreComputeCommon.hlsl"
static const float Epsilon = 0.00001;
static const uint NumSamples = 64 * 1024u;
static const float InvNumSamples = 1.0 / float(NumSamples);

TextureCube InputTexture : register(t0);
RWTexture2DArray<float4> OutputTexture : register(u0);
SamplerState DefaultSampler : register(s0);

[numthreads(32, 32, 1)]
void CSMain(uint3 ThreadID : SV_DispatchThreadID)
{
    float3 N = GetSamplingVector(ThreadID, OutputTexture);
	
    float3 S, T;
    ComputeBasisVectors(N, Epsilon, S, T);
    
    float3 irradiance = 0.0;
    
    /*float sampleDelta = 0.025;
    float sampleDeltaTheta = 0.015;
    float nrSamples = 0.0;
    for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDeltaTheta)
        {
            float sinTheta = sin(theta);
            float cosTheta = cos(theta);
            float3 tangentSample = float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
            float3 sampleVector = TangentToWorld(tangentSample, N, S, T);
            
            irradiance += InputTexture.SampleLevel(DefaultSampler, sampleVector, 0).rgb * cosTheta * sinTheta;
            nrSamples++;
        }
    }
    irradiance = PI * irradiance * (1.0 / float(nrSamples));*/
    
    for (uint i = 0; i < NumSamples; ++i)
    {
        float2 u = SampleHammersley(i, InvNumSamples);
        float3 Li = TangentToWorld(SampleHemisphere(u.x, u.y), N, S, T);
        float cosTheta = max(0.0, dot(Li, N));

        irradiance += 2.0 * InputTexture.SampleLevel(DefaultSampler, Li, 0).rgb * cosTheta;
    }
    irradiance *= InvNumSamples;

    OutputTexture[ThreadID] = float4(irradiance, 1.0);
}