﻿#include "../Include/Structures.hlsl"

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}

SamplerState Sampler : register(s0);
TextureCube DiffuseMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D AOMap : register(t2);
Texture2D RoughnessMap : register(t3);


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
	output.albedo = saturate(DiffuseMap.Sample(Sampler, -input.normal));
    output.position = input.worldPos;
    output.normal = float4(input.normal, 1.0f);
    float depth = input.pos.z / input.pos.w;
    output.roughnessMetallicDepth = float4(0.0f, 0.0f, depth, 1.0f);
    output.occlusionUnlitNonShadow = float4(1.0f, ConstantBuffer.optionsMask1.b, ConstantBuffer.optionsMask1.a, 1.0f);
    return output;
}
