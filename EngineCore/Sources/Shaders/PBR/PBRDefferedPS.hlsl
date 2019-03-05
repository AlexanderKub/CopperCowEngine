#include "../Include/Structures.hlsl"
#include "../Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

SamplerState Sampler : register(s0);
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);

PixelOutputType PSMain(COMMON_PS_IN Input)
{
    PixelOutputType output;
	//Albedo
    if (cbPerObject.optionsMask0.r > 0)
    {
        output.albedo = AlbedoMap.Sample(Sampler, Input.uv0.xy);
    } else {
        output.albedo = float4(cbPerObject.AlbedoColor.rgb, 1.0f);
    }
	
	//Position
    output.position = Input.posWS;
	
	//Normal
    float3 NormalValue;
    if (cbPerObject.optionsMask0.g > 0)
    {
        NormalValue = NormalMap.Sample(Sampler, Input.uv0.xy).xyz * 2.0f - 1.0f;
        float3 binormal = CalcBinormal(Input.normal.xyz, Input.tangent.xyz);
        NormalValue = (NormalValue.x * Input.tangent.xyz) + (NormalValue.y * binormal) + (NormalValue.z * Input.normal.xyz);
    } else {
        NormalValue = Input.normal.xyz;
    }
    NormalValue = normalize(NormalValue);

    output.normal = float4(NormalValue, 1.0f);
	
	//Roughness Metallic Depth
    float roughness;
    if (cbPerObject.optionsMask0.b > 0)
    {
        roughness = RoughnessMap.Sample(Sampler, Input.uv0.xy).r;
    } else {
        roughness = cbPerObject.Roughness;
    }
	
    float metallic;
    if (cbPerObject.optionsMask0.a > 0)
    {
        metallic = MetallicMap.Sample(Sampler, Input.uv0.xy).r;
    } else {
        metallic = cbPerObject.Metallic;
    }
	
    float depth = Input.pos.z / Input.pos.w;
    output.roughnessMetallicDepth = float4(roughness, metallic, depth, 1.0f);
    
	//Occlusion Unlit NonShadow
    float occlusion = 1.0f;
    if (cbPerObject.optionsMask1.r > 0)
    {
        occlusion = OcclusionMap.Sample(Sampler, Input.uv0.xy).r;
    }
    output.occlusionUnlitNonShadow = float4(occlusion, cbPerObject.optionsMask1.g, cbPerObject.optionsMask1.b, 1.0f);

    return output;
}
