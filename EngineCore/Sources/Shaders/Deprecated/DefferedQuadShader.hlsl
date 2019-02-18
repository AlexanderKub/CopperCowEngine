#include "CookTorranceLightModel.hlsl"

Texture2D Diffuse : register(t0);
Texture2D Positions : register(t1);
Texture2D Normals : register(t2);
Texture2D RoughAO : register(t3);
Texture2D Specular : register(t4);
SamplerState Sampler : register(s0);

struct ConstBuffer
{
    float4x4 transformMatrix;
    float4 eyeWorldPosition;
    float4 Type;
};

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}

cbuffer lights : register(b1)
{
    Light light;
}

struct VS_IN
{
    float4 pos : POSITION;
    float4 tex : TEXCOORD0;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD0;
};

PS_IN VSMain(VS_IN input)
{
    PS_IN output = (PS_IN) 0;
    output.pos = mul(float4(input.pos.xyz, 1), ConstantBuffer.transformMatrix);
    output.tex = input.tex.xy;
    return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
    float4 diffuse = Diffuse.Sample(Sampler, input.tex);
    float4 position = Positions.Sample(Sampler, input.tex);
    float4 normal = Normals.Sample(Sampler, input.tex);
    float4 roughnessAO = RoughAO.Sample(Sampler, input.tex);
    float4 specular = Specular.Sample(Sampler, input.tex);
    
    float4 lightColor = float4(0, 0, 0, 0);
    float depthValue = 1.0;

    if (ConstantBuffer.Type.r == 1.0f)
    {
        return diffuse;
    }
    
    if (ConstantBuffer.Type.r == 2.0f)
    {
        return 1.0f - saturate(position * 0.1f);
    }
    
    if (ConstantBuffer.Type.r == 3.0f)
    {
        return (normal + 1.0f) * 0.5f;
    }

	if (ConstantBuffer.Type.r == 4.0f)
	{
		return specular;
	}

    if (ConstantBuffer.Type.r == 5.0f)
    {
        return roughnessAO;
    }
   
    if (light.type == 1)
    {
        lightColor += getPointLight(
            position.xyz,
            ConstantBuffer.eyeWorldPosition.xyz,
		    normal.xyz,
		    light,
            depthValue,
            diffuse.rgb,
            specular.r,
            roughnessAO.rg,
            roughnessAO.b
	    );
    }
    else if (light.type == 2)
    {
        lightColor += getSpotLight(
            position.xyz,
            ConstantBuffer.eyeWorldPosition.xyz,
		    normal.xyz,
            light,
            depthValue,
            diffuse.rgb,
            specular.r,
            roughnessAO.rg,
            roughnessAO.b
	    );
    }
    else
    {
        lightColor += getDirectionalLight(
            position.xyz,
            ConstantBuffer.eyeWorldPosition.xyz,
		    normal.xyz,
            light,
            depthValue,
            diffuse.rgb,
            specular.r,
            roughnessAO.rg,
            roughnessAO.b
	    );
    }
    return lightColor;

    //float4 postProcessTest = lightColor;
    /* Grayscale with red
    if (postProcessTest.r < (postProcessTest.g + postProcessTest.b)  * 1.5f)
    {
        float grayScale = (postProcessTest.r + postProcessTest.g + postProcessTest.b) * 0.333;
        postProcessTest.rgb = float3(grayScale, grayScale, grayScale);
    }
    */
    
    //postProcessTest = saturate(postProcessTest);
    //return postProcessTest;
}