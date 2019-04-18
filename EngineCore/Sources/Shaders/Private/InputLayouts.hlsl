
// Structure for shadow map pass
struct POS_ONLY_VS_IN
{
    float4 pos : POSITION;
};

// Standard vertex structure
struct STANDARD_VS_IN
{
    float3 Position : POSITION;
    float4 UV : TEXCOORD;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
};