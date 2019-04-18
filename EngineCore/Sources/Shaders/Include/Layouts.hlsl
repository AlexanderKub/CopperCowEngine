#ifndef __DEPENDENCY_HLSL_LAYOUTS__
#define __DEPENDENCY_HLSL__
struct COMMON_VS_IN
{
    float4 pos : POSITION;
    float4 color : COLOR;
    float2 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct COMMON_PS_IN
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
    float4 posWS : POSITION;
    float2 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 normal : NORMAL;
    float4 tangent : TANGENT;
};

struct COMMON_POSITION_ONLY_PS_IN
{
    float4 Position : SV_POSITION;
    float4 PrevPosition : POSITION;
    float2 Velocity : COLOR;
};

struct COMMON_POSITION_AND_UV_PS_IN
{
    float4 Position : SV_POSITION;
    float4 PrevPosition : POSITION;
    float2 TextureUV : TEXCOORD0;
};

/*struct PixelOutputTypes
{
    float4 backBuffer : SV_Target0;
    float4 normalsTarget : SV_Target1;
};*/

float3 CalcBinormal(float3 normal, float3 tangent)
{
    return normalize(cross(normal, tangent));
}
#endif