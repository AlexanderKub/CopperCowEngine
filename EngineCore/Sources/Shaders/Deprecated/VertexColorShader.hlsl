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
	Light LightBuffer;
}

Texture2D ShadowMap : register(t0);
SamplerComparisonState ShadowsSampler : register(s0);

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
	float4 col : COLOR;
    float3 normal : NORMAL;
    float receiveShadows : RECIVESHADOWS;
    float4 screenPos : TEXCOORD0;
};

PS_IN VSMain(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.worldPos = mul(float4(input.pos.xyz, 1.0f), ConstantBuffer.worldMatrix);
	output.pos = mul(output.worldPos, ConstantBuffer.viewProjMatrix);
    output.screenPos = output.pos;
	output.col = input.col;
    output.normal = normalize(mul(float4(input.normal.xyz, 0), ConstantBuffer.worldMatrix).xyz);
   
	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
    float depthValue = 1.0;
    if (LightBuffer.recieveShadows == 1) {
        float4 lightViewPosition = mul(input.worldPos, ConstantBuffer.lightViewProjMatrix);
        depthValue = GetShadow16X(GetShadowMapCoordinates(lightViewPosition), ShadowMap, ShadowsSampler);
    }

    float4 lightColor = getDirectionalLight(
        input.worldPos.xyz,
        ConstantBuffer.eyeWorldPosition.xyz,
		normalize(input.normal.xyz),
		LightBuffer,
        depthValue,
        input.col.rgb,
        float3(0.8, 0.8, 0.8),
        float2(0.5, 0.5),
        1
	);

    return lightColor;
}