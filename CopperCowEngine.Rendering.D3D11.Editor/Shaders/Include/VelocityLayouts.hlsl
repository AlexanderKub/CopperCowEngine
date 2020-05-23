#ifndef __DEPENDENCY_HLSL_VELOCITY_LAYOUTS__
#define __DEPENDENCY_HLSL_VELOCITY_LAYOUTS__
struct VELOCITY_POSITION_ONLY_PS_IN
{
    float4 Position : SV_POSITION;
    float4 CurPosition : POSITION1;
    float4 PrevPosition : POSITION0;
};

struct VELOCITY_POSITION_AND_UV_PS_IN
{
    float4 Position : SV_POSITION;
    float4 CurPosition : POSITION1;
    float4 PrevPosition : POSITION0;
	float2 TextureUV : TEXCOORD0;
};
#endif