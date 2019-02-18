#include "../Include/Structures.hlsl"

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}

SamplerState Sampler : register(s0);
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);

struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
    float4 tex : TEXCOORD0;
    float4 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
    float4 worldPos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
};

PS_IN VSMain(VS_IN input)
{
    PS_IN output = (PS_IN) 0;
    
    input.pos.w = 1.0f;
    output.worldPos = mul(input.pos, ConstantBuffer.worldMatrix);
    output.pos = mul(output.worldPos, ConstantBuffer.viewProjMatrix);
    
    output.col = input.col;

    output.tex = input.tex.xy;
    output.tex.x *= ConstantBuffer.textureTiling.x;
    output.tex.x += ConstantBuffer.textureShift.x;
    output.tex.y *= ConstantBuffer.textureTiling.y;
    output.tex.y += ConstantBuffer.textureShift.y;
    
    output.normal = normalize(mul(float4(input.normal.xyz, 0), ConstantBuffer.worldMatrix).xyz);
    output.binormal = normalize(mul(float4(input.binormal.xyz, 0), ConstantBuffer.worldMatrix).xyz);
    output.tangent = normalize(mul(float4(input.tangent.xyz, 0), ConstantBuffer.worldMatrix).xyz);

    return output;
}

PixelOutputType PSMain(PS_IN input)
{
    PixelOutputType output;
	//Albedo
    if (ConstantBuffer.optionsMask0.r > 0)
    {
        output.albedo = AlbedoMap.Sample(Sampler, input.tex);
    } else {
        output.albedo = float4(ConstantBuffer.AlbedoColor.rgb, 1.0f);
    }
	
	//Position
    output.position = input.worldPos;
	
	//Normal
    float3 NormalValue;
    if (ConstantBuffer.optionsMask0.g > 0)
    {
        NormalValue = NormalMap.Sample(Sampler, input.tex).xyz * 2.0f - 1.0f;
        NormalValue = (NormalValue.x * input.tangent) + (NormalValue.y * input.binormal) + (NormalValue.z * input.normal);
    } else {
        NormalValue = input.normal;
    }
    NormalValue = normalize(NormalValue);

    output.normal = float4(NormalValue, 1.0f);
	
	//Roughness Metallic Depth
    float roughness;
    if (ConstantBuffer.optionsMask0.b > 0)
    {
        roughness = float4(RoughnessMap.Sample(Sampler, input.tex).rgb, 1.0f).r;
    } else {
        roughness = ConstantBuffer.Roughness;
    }
	
    float metallic;
    if (ConstantBuffer.optionsMask0.a > 0)
    {
        metallic = float4(MetallicMap.Sample(Sampler, input.tex).rgb, 1.0f).r;
    } else {
        metallic = ConstantBuffer.Metallic;
    }
	
    float depth = input.pos.z / input.pos.w;
    output.roughnessMetallicDepth = float4(roughness, metallic, depth, 1.0f);
    
	//Occlusion Unlit NonShadow
    float occlusion = 1.0f;
    if (ConstantBuffer.optionsMask1.r > 0)
    {
        occlusion = OcclusionMap.Sample(Sampler, input.tex).r;
    }
    output.occlusionUnlitNonShadow = float4(occlusion, ConstantBuffer.optionsMask1.g, ConstantBuffer.optionsMask1.b, 1.0f);

    return output;
}
