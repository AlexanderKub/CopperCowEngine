#include "../Include/ForwardPlusCommon.hlsl"
#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"
#include "../PBR/PBRLightSurface.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

cbuffer cbPerFrameBuffer : register(b2)
{
    CBufferDirLightStruct DirLightsData[3];
}

//PBR Material
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);
TextureCube RaddianceEnvMap : register(t5);
TextureCube IrradianceEnvMap : register(t6);
SamplerState Sampler : register(s0);

//Lights Data
Buffer<float4> g_NonDirLightBufferCenterAndRadius : register(t7);
Buffer<float4> g_NonDirtLightBufferParams : register(t8);
Buffer<float4> g_NonDirLightBufferColor : register(t9);
Buffer<uint> g_PerTileLightIndexBuffer : register(t10);

float4 PSMain(COMMON_PS_IN Input) : SV_TARGET
{
    float unlit = cbPerObject.optionsMask1.g;
    float nonShadow = cbPerObject.optionsMask1.b;

    float4 AlbedoValue = cbPerObject.AlbedoColor;
    if (cbPerObject.optionsMask0.r > 0) {
        AlbedoValue = AlbedoMap.Sample(Sampler, Input.uv0.xy);
    }

    if (unlit > 0.0f) {
        return AlbedoValue;
    }

    float3 NormalValue = normalize(Input.normal.xyz);
    if (cbPerObject.optionsMask0.g > 0) {
        NormalValue = NormalMap.Sample(Sampler, Input.uv0.xy).xyz * 2.0f - 1.0f;
        float3 binormal = CalcBinormal(Input.normal.xyz, Input.tangent.xyz);
        NormalValue = (NormalValue.x * Input.tangent.xyz) + (NormalValue.y * binormal) + (NormalValue.z * Input.normal.xyz);
        NormalValue = normalize(NormalValue);
    }

    float RoughnessValue = cbPerObject.Roughness;
    if (cbPerObject.optionsMask0.b > 0) {
        RoughnessValue = RoughnessMap.Sample(Sampler, Input.uv0.xy).r;
    }

    float MetallicValue = cbPerObject.Metallic;
    if (cbPerObject.optionsMask0.a > 0) {
        MetallicValue = MetallicMap.Sample(Sampler, Input.uv0.xy).r;
    }

    float OcclusionValue = 1.0f;
    if (cbPerObject.optionsMask1.r > 0) {
        OcclusionValue = OcclusionMap.Sample(Sampler, Input.uv0.xy).r;
    }

    float shadowDepthValue = 1.0;
    float3 V = normalize(cbPerFrame.CameraPos.xyz - Input.posWS.xyz);

    uint nTileIndex = GetTileIndex(Input.pos.xy, cbPerFrame.WindowWidth);
    uint nIndex = cbPerFrame.MaxNumLightsPerTile * nTileIndex;
    uint nNextLightIndex = g_PerTileLightIndexBuffer[nIndex];
    float3 color = LightSurfaceTiled(
        V, NormalValue, Input.posWS.xyz, 
        AlbedoValue.rgb, RoughnessValue, MetallicValue, OcclusionValue,
        RaddianceEnvMap, IrradianceEnvMap, Sampler, 
        nIndex, nNextLightIndex, 
        g_NonDirLightBufferCenterAndRadius, g_NonDirLightBufferColor, 
        g_NonDirtLightBufferParams, g_PerTileLightIndexBuffer, 
        cbPerFrame.DirLightNum, DirLightsData
    );

    return float4(color, min(AlbedoValue.a, cbPerObject.AlbedoColor.a));
}