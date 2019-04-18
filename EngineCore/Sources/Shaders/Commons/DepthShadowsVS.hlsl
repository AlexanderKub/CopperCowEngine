struct ConstBuffer
{
    float4x4 worldMatrix;
	float4x4 viewProjMatrix;
};

cbuffer constants : register(b0)
{
	ConstBuffer ConstantBuffer;
}

struct VS_IN
{
	float4 pos : POSITION;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 depthPos : POSITION;
};

PS_IN VSMain(VS_IN input)
{
	PS_IN output = (PS_IN)0;
    input.pos.w = 1.0f;
    output.pos = mul(input.pos, ConstantBuffer.worldMatrix);
    output.pos = mul(output.pos, ConstantBuffer.viewProjMatrix);
    output.depthPos = output.pos;
	return output;
}