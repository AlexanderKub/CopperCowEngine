struct ConstBuffer
{
	float4x4 transformMatrix;
};

cbuffer constants : register(b0)
{
	ConstBuffer ConstantBuffer;
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
	PS_IN output = (PS_IN)0;
    output.pos = mul(float4(input.pos.xyz, 1), ConstantBuffer.transformMatrix);
	output.tex = input.tex.xy;
	return output;
}