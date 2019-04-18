struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 depthPos : POSITION;
};

float4 PSMain(PS_IN input) : SV_Target
{
    float depthValue = input.depthPos.z / input.depthPos.w;
    return float4(depthValue, depthValue, depthValue, 1.0f);
}