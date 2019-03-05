#include "../Include/ShadowMap.hlsl"
#include "../Include/Constants.hlsl"
#include "../Include/Structures.hlsl"
#include "PBRLightSurface.hlsl"

SamplerState Sampler : register(s0);
Texture2D Albedo : register(t0);
Texture2D Positions : register(t1);
Texture2D Normals : register(t2);
Texture2D RoughnessMetallicDepth : register(t3);
Texture2D OcclusionUnlitNonShadow : register(t4);
TextureCube RaddianceEnvMap : register(t5);
TextureCube IrradianceEnvMap : register(t6);

Texture2D ShadowMap : register(t7);
SamplerComparisonState ShadowsSampler : register(s1);

struct QuadConstBuffer
{
    float4x4 transformMatrix;
    float4 eyeWorldPosition;
    float4 Type;
};

cbuffer constants : register(b0)
{
    QuadConstBuffer ConstantBuffer;
}

cbuffer Lights : register(b1)
{
    LightBuffer LightData[MaxLightsCount];
}

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 PSMain(PS_IN input) : SV_Target
{
    float4 position = Positions.Sample(Sampler, input.tex);
    float4 normal = Normals.Sample(Sampler, input.tex);

    float4 occlusionUnlitNonShadow = OcclusionUnlitNonShadow.Sample(Sampler, input.tex);
    float occlusion = occlusionUnlitNonShadow.r;
    float unlit = occlusionUnlitNonShadow.g;
    float nonShadow = occlusionUnlitNonShadow.b;
    
    float3 N = normalize(normal.xyz);
    float3 L = normalize(LightData[0].position.xyz - position.xyz);
    float3 V = normalize(ConstantBuffer.eyeWorldPosition.xyz - position.xyz);

    float shadowDepthValue = 1.0;
    /*if (nonShadow < 1.0f) {
        float4 lightViewPosition = mul(position, LightData[0].lightViewProjMatrix);
        //GetShadowOneSample GetShadow16X
        shadowDepthValue = GetShadow8X(GetShadowMapCoordinates(lightViewPosition), ShadowMap, ShadowsSampler);
    }*/
    //float r = ShadowMap.Sample(Sampler, input.tex).r;
    //return r * 10;

    float4 albedo = Albedo.Sample(Sampler, input.tex);
    float4 rmd = RoughnessMetallicDepth.Sample(Sampler, input.tex);
    float roughness = rmd.r;
    float metallic = rmd.g;
    float depth = rmd.b;

    if (ConstantBuffer.Type.r == 1.0f) {
        return float4(albedo.rgb, 1.0f);
    }
    
    if (ConstantBuffer.Type.r == 2.0f) {
        return float4((1.0f - saturate(position * 0.1f)).rgb, 1.0f);
    }
    
    if (ConstantBuffer.Type.r == 3.0f) {
        return float4(((normal + 1.0f) * 0.5f).rgb, 1.0f);
    }

	if (ConstantBuffer.Type.r == 4.0f) {
        return float4(roughness, roughness, roughness, 1.0f);
    }

    if (ConstantBuffer.Type.r == 5.0f) {
        return float4(metallic, metallic, metallic, 1.0f);
    }

    if (ConstantBuffer.Type.r == 6.0f) {
        return float4(depth, depth, depth, 1.0f);
    }

    if (ConstantBuffer.Type.r == 7.0f) {
        return float4(occlusion, occlusion, occlusion, 1.0f);
    }

    if (ConstantBuffer.Type.r == 8.0f)
    {
        return float4(unlit, unlit, unlit, 1.0f);
    }

    if (ConstantBuffer.Type.r == 9.0f)
    {
        return ShadowMap.Sample(Sampler, input.tex);
        return float4(nonShadow, nonShadow, nonShadow, 1.0f);
    }

    if (unlit) {
        return float4(albedo.rgb, 1.0f);
    }
    
    float4 color = float4(LightSurface(V, N, 1, LightData, albedo.rgb, roughness, 
        metallic, occlusion, RaddianceEnvMap, IrradianceEnvMap, Sampler, position.xyz), albedo.a);
    float4 result = color * 0.2f + color * shadowDepthValue * 0.8f;
    return result;
}