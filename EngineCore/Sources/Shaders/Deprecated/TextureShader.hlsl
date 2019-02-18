//#include "DirectionalPhongLight.hlsl"
#include "CookTorranceLightModel.hlsl"
#include "ShadowMap.hlsl"

struct ConstBuffer
{
    float4x4 viewProjMatrix;
    float4x4 worldMatrix;
    float4x4 lightViewProjMatrix;
    float4 eyeWorldPosition;
    float2 textureTiling;
    float2 textureShift;
};

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}

cbuffer lights : register(b1)
{
    Light light;
}

Texture2D ShadowMap : register(t0);
SamplerComparisonState ShadowsSampler : register(s0);

Texture2D AlbedoMap : register(t1);
Texture2D NormalMap : register(t2);
Texture2D RoughnessMap : register(t3);
Texture2D MEtallicMap : register(t4);
Texture2D OcclusionMap : register(t5);
SamplerState Sampler : register(s1);

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
    float4 worldPos : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
    float3 binormal : BINORMAL;
    float receiveShadows : RECIVESHADOWS;
};

PS_IN VSMain(VS_IN input)
{
    PS_IN output = (PS_IN) 0;

    output.worldPos = mul(float4(input.pos.xyz, 1.0f), ConstantBuffer.worldMatrix);
    output.pos = mul(output.worldPos, ConstantBuffer.viewProjMatrix);
    
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

float4 PSMain(PS_IN input) : SV_Target
{
    float3 DiffuseColor = AlbedoMap.Sample(Sampler, input.tex).rgb;
    float3 SpecularColor = SpecularMap.Sample(Sampler, input.tex).rgb;

    float3 NormalValue;
    if (light.hasNormalMap == 0.0)
    {
        NormalValue = normalize(input.normal);
    }
    else
    {
        NormalValue = NormalMap.Sample(Sampler, input.tex).xyz * 2.0f - 1.0f;
        NormalValue = (NormalValue.x * input.tangent) + (NormalValue.y * input.binormal) + (NormalValue.z * input.normal);
        NormalValue = normalize(NormalValue);
    }

    float2 RoughnessValue = RoughnessMap.Sample(Sampler, input.tex).rg * light.hasRoughnessMap;
    RoughnessValue += float2(0.2, 0.25) * (1 - light.hasRoughnessMap);
	
    float AOValue = (1 - light.hasAOMap) + OcclusionMap.Sample(Sampler, input.tex).r * light.hasAOMap;

    float depthValue = 1.0;
    if (light.recieveShadows == 1)
    {
       float4 lightViewPosition = mul(input.worldPos, ConstantBuffer.lightViewProjMatrix);
        depthValue = GetShadow16X(GetShadowMapCoordinates(lightViewPosition), ShadowMap, ShadowsSampler);
    }
    
    int i;
    float4 lightColor = float4(0, 0, 0, 0);
    if (light.type == 1)
    {
        lightColor += getPointLight(
                input.worldPos.xyz,
                ConstantBuffer.eyeWorldPosition.xyz,
		        NormalValue,
		        light,
                depthValue,
                DiffuseColor,
                SpecularColor,
                RoughnessValue,
                AOValue
	        );
    }
    else if (light.type == 2)
    {
        lightColor += getSpotLight(
                input.worldPos.xyz,
                ConstantBuffer.eyeWorldPosition.xyz,
		        NormalValue,
                light,
                depthValue,
                DiffuseColor,
                SpecularColor,
                RoughnessValue,
                AOValue
	        );
    }
    else
    {
        lightColor += getDirectionalLight(
                input.worldPos.xyz,
                ConstantBuffer.eyeWorldPosition.xyz,
		        NormalValue,
                light,
                depthValue,
                DiffuseColor,
                SpecularColor,
                RoughnessValue,
                AOValue
	        );
    }

    return lightColor;
}