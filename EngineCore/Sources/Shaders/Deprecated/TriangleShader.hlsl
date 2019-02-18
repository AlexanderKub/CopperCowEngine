/*struct ConstBuffer
{
    float4x4 viewProjMatrix;
    float4x4 modelMatrix;
    float4 lightDirection;
};

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}*/

struct VS_IN
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct GSPS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

GSPS_IN VSMain(VS_IN input)
{
    GSPS_IN output = (GSPS_IN) 0;
    output.Position = input.Position;
    output.Color = input.Color;
    return output;
}