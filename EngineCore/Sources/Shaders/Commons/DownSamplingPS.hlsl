Texture2D InputMap : register(t0);
SamplerState Sampler : register(s0);


struct VSQuadOut
{
    float4 position : SV_Position;
    float2 texcoord : TexCoord;
};

float4 PSMain(VSQuadOut input) : SV_Target
{
    float4 texColor = InputMap.Sample(Sampler, input.texcoord);
	return texColor;
}