Texture2D Diffuse : register(t0);
SamplerState Sampler : register(s0);

struct PS_IN
{
	float4 pos : SV_POSITION;
	float2 tex : TEXCOORD0;
};

float4 PSMain(PS_IN input) : SV_Target
{
	float4 texColor = Diffuse.Sample(Sampler, input.tex);
	return texColor;
}