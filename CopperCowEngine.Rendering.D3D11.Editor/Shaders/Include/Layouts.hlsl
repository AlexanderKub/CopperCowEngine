#ifndef __DEPENDENCY_HLSL_LAYOUTS__
#define __DEPENDENCY_HLSL_LAYOUTS__
struct COMMON_VS_IN
{
    float3 pos : POSITION;      //12 bytes
    float4 color : COLOR;       //16 bytes
    float2 uv0 : TEXCOORD0;     //8 bytes
    float4 uv1 : TEXCOORD1;     //16 bytes
    float3 normal : NORMAL;     //12 bytes
    float3 tangent : TANGENT;   //12 bytes
                                //Total: 76 bytes 6 attributes
};

struct COMMON_PS_IN
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
    float3 posWS : POSITION;
    float2 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float3 normal : NORMAL;
    float3 tangent : TANGENT;
};

struct COMMON_POSITION_ONLY_PS_IN
{
    float4 Position : SV_POSITION;
};

struct COMMON_POSITION_AND_UV_PS_IN
{
	float4 Position : SV_POSITION;
    float2 TextureUV : TEXCOORD0;
};

float3 CalcBinormal(float3 normal, float3 tangent)
{
    return normalize(cross(normal, tangent));
}
#endif