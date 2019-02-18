struct ConstBuffer
{
	float4x4 transformMatrix;
};

cbuffer constants : register(b0)
{
	ConstBuffer ConstantBuffer;
}

Texture2D Diffuse : register(t0);
SamplerState Sampler : register(s0);

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
	PS_IN output = (PS_IN)0;
    output.pos = mul(float4(input.pos.xyz, 1), ConstantBuffer.transformMatrix);
	output.tex = input.tex.xy;
	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	float4 texColor = Diffuse.Sample(Sampler, input.tex);
	return texColor;
}