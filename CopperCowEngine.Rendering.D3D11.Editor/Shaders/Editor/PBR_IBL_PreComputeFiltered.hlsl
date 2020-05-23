#include "PBR_PreComputeCommon.hlsl"
static const float Epsilon = 1e-4f;

static const uint NumSamples = 1024u;
static const float InvNumSamples = 1.0 / float(NumSamples);

cbuffer SpecularMapFilterSettings : register(b0)
{
    float roughness;
};

TextureCube InputTexture : register(t0);
RWTexture2DArray<float4> OutputTexture : register(u0);
SamplerState DefaultSampler : register(s0);

[numthreads(32, 32, 1)]
void CSMain(uint3 ThreadID : SV_DispatchThreadID)
{
    uint outputWidth, outputHeight, outputDepth;
    OutputTexture.GetDimensions(outputWidth, outputHeight, outputDepth);
    if (ThreadID.x >= outputWidth || ThreadID.y >= outputHeight)
    {
        return;
    }
	
    float inputWidth, inputHeight, inputLevels;
    InputTexture.GetDimensions(0, inputWidth, inputHeight, inputLevels);

    float wt = 4.0 * PI / (6 * inputWidth * inputHeight);
	
    float3 N = GetSamplingVector(ThreadID, OutputTexture);
    float3 V = N;
	
    float3 S, T;
    ComputeBasisVectors(N, Epsilon, S, T);

    float3 color = 0.0;
    float weight = 0.0;

    for (uint i = 0u; i < NumSamples; ++i)
    {
        float2 u = SampleHammersley(i, InvNumSamples);
        float3 H = TangentToWorld(SampleGGX(u.x, u.y, roughness), N, S, T);
        float3 L = normalize(2.0 * dot(V, H) * H - V);
        
        float NdotH = saturate(dot(N, H));
        float HdotV = saturate(dot(H, V));
        float NdotL = saturate(dot(N, L));

        if (NdotL > 0.0)
        {
            float pdf = NdfGGX(HdotV, roughness) * 0.25 + Epsilon;
            
            float ws = 1.0 / (float(NumSamples) * pdf + Epsilon);
            float mipLevel = max(0.5 * log2(ws / wt) + 1.0, 0.0);
            
            color += InputTexture.SampleLevel(DefaultSampler, L, mipLevel).rgb * NdotL;
            weight += NdotL;
        }
    }
    color /= max(weight, Epsilon);

    OutputTexture[ThreadID] = float4(color, 1.0);
}
