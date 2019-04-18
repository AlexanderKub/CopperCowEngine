//<TEXTURES>
//<SAMPLERS>
//<BUFFERS>
struct PS_IN
{
    float4 position : SV_POSITION;
    float2 uv0 : TEXCOORD0;
};

struct COMMON_PS_IN
{
    float4 pos : SV_POSITION;
    float4 cololr : COLOR;
    float4 posWS : POSITION;
    float2 uv0 : TEXCOORD0;
    float4 uv1 : TEXCOORD1;
    float4 normal : NORMAL;
    float4 tangent : TANGENT;
};

float4 PSMain(COMMON_PS_IN input) : SV_Target
{
    float3 EmissiveColor = float3(0.8, 0.8, 0.8);
#if (MASKED_BLEND || ALPHA_BLEND)
    float Opacity = 1;
#if (MASKED_BLEND)
    float OpacityMaskClipValue = 0.3333f;
#endif
#endif
    //<VARIABLES>
    //<CODE>
#if (MASKED_BLEND)
    if (Opacity < OpacityMaskClipValue) {
        discard;
    }
#endif
#if (ALPHA_BLEND)
    return float4(EmissiveColor, Opacity);
#else
    return float4(EmissiveColor, 1);
#endif
}