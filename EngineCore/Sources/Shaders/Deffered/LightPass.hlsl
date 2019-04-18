#include "./Include/Layouts.hlsl"
#include "../CommonsInclude/StandardHeaders.hlsl"

VSQuadOut VSMain(VertexShaderInput vertex)
{
    VSQuadOut result = (VSQuadOut) 0;
    vertex.Position.w = 1.0f;
    result.Position = vertex.Position;
    result.Position = mul(result.Position, PerObjectBuffer.World);
    result.Position = mul(result.Position, PerFrameBuffer.View);
    result.Position = mul(result.Position, PerFrameBuffer.Projection);

    result.PosProj = float4(result.Position.xy / result.Position.w, 1, 1);
    // Determine UV from device coords
    result.Texcoord.xy = result.Position.xy / result.Position.w;
    // The UV coords: top-left [0,0] bottom-right [1,1]
    result.Texcoord.x = result.Texcoord.x * 0.5 + 0.5;
    result.Texcoord.y = result.Texcoord.y * -0.5 + 0.5;

    return result;
}

#include "./Include/LightSurface.hlsl"
#include "./Include/PBR.hlsl"
#include "./Include/BRDF.hlsl"

#if MSAA
Texture2DMS<float4> AlbedoAndMetallic : register(t0);
Texture2DMS<float4> EmissiveAndRoughness : register(t1);
Texture2DMS<float3> PackedNormals : register(t2);
Texture2DMS<float4> SpecularAOUnlitNonShadows : register(t3);
Texture2DMS<float> DepthMap : register(t4);
TextureCube RaddianceEnvMap : register(t5);
TextureCube IrradianceEnvMap : register(t6);
Texture2D<float2> BRDFxLUT : register(t7);
#else
Texture2D AlbedoAndMetallic : register(t0);
Texture2D EmissiveAndRoughness : register(t1);
Texture2D PackedNormals : register(t2);
Texture2D SpecularAOUnlitNonShadows : register(t3);
Texture2D<float> DepthMap : register(t4);
TextureCube RaddianceEnvMap : register(t5);
TextureCube IrradianceEnvMap : register(t6);
Texture2D<float2> BRDFxLUT : register(t7);
#endif

SamplerState Sampler : register(s0);
SamplerState PreIntegratedSampler : register(s1);

float ConvertDepthToLinear(float depth)
{
    return PerFrameBuffer.PerspectiveValues.z / (depth + PerFrameBuffer.PerspectiveValues.w);
}

float3 CalcWorldPos(float2 csPos, float linearDepth)
{
    float4 position;
    position.xy = csPos.xy * PerFrameBuffer.PerspectiveValues.xy * linearDepth;
    position.z = linearDepth;
    position.w = 1.0;
    return mul(position, PerFrameBuffer.InverseView).xyz;
}

void ExtractGBufferAttributes(float3 pos, float2 cPos, int sampleIndex, out GBufferAttributes attrs)
{
    #if MSAA
    int2 screenPos = int2(pos.xy);
    attrs.AlbedoColor = AlbedoAndMetallic.Load(screenPos, sampleIndex).rgb;
    attrs.MetallicValue = AlbedoAndMetallic.Load(screenPos, sampleIndex).a;

    attrs.EmmisiveColor = EmissiveAndRoughness.Load(screenPos, sampleIndex).rgb;
    attrs.RoughnessValue = EmissiveAndRoughness.Load(screenPos, sampleIndex).a;

    attrs.SpecularValue = SpecularAOUnlitNonShadows.Load(screenPos, sampleIndex).r;
    attrs.AOValue = SpecularAOUnlitNonShadows.Load(screenPos, sampleIndex).g;
    attrs.Unlit = SpecularAOUnlitNonShadows.Load(screenPos, sampleIndex).b > 0;
    attrs.NonShadow = SpecularAOUnlitNonShadows.Load(screenPos, sampleIndex).a > 0;
    
    attrs.Normal = PackedNormals.Load(screenPos, sampleIndex).rgb;
    float linearDepth = ConvertDepthToLinear(DepthMap.Load(screenPos, sampleIndex));
    #else
    int3 screenPos = int3(pos.xy, 0);
    attrs.AlbedoColor = AlbedoAndMetallic.Load(screenPos).rgb;
    attrs.MetallicValue = AlbedoAndMetallic.Load(screenPos).a;

    attrs.EmmisiveColor = EmissiveAndRoughness.Load(screenPos).rgb;
    attrs.RoughnessValue = EmissiveAndRoughness.Load(screenPos).a;

    attrs.SpecularValue = SpecularAOUnlitNonShadows.Load(screenPos).r;
    attrs.AOValue = SpecularAOUnlitNonShadows.Load(screenPos).g;
    attrs.Unlit = SpecularAOUnlitNonShadows.Load(screenPos).b > 0;
    attrs.NonShadow = SpecularAOUnlitNonShadows.Load(screenPos).a > 0;
    
    attrs.Normal = PackedNormals.Load(screenPos).rgb;
    float linearDepth = ConvertDepthToLinear(DepthMap.Load(screenPos));
    #endif

    attrs.Normal = normalize(attrs.Normal * 2.0 - 1.0);
    // Reconstruct the world position
    attrs.Position = CalcWorldPos(cPos, linearDepth);
}

float3 LinearToSRGB(float3 color)
{
    return pow(abs(color), 1 / 2.2f);
}

#if MSAA
float4 PSMain(in VSQuadOut input, uint coverage : SV_Coverage, uint sampleIndex : SV_SampleIndex) : SV_Target
#else
float4 PSMain(in VSQuadOut input) : SV_Target
#endif
{
    GBufferAttributes attrs;
    #if MSAA
    // Is sample not covered
    if (!(coverage & (1 << sampleIndex)))
    {
        discard;
        return 0;
    }
    ExtractGBufferAttributes(input.Position.xyz, input.PosProj.xy, sampleIndex, attrs);
    #else
    ExtractGBufferAttributes(input.Position.xyz, input.PosProj.xy, 0, attrs);
    #endif
    //return float4(attrs.Normal, 1);
    //return float4(attrs.Normal * 0.5 + 0.5, 1);


    [branch]
    if (attrs.Unlit)
    {
        return float4(attrs.AlbedoColor, 1);
    }
    else
    {
        const float3 V = normalize(PerFrameBuffer.CameraPosition - attrs.Position);
        return float4(IBLLightSurface(V, attrs, RaddianceEnvMap, IrradianceEnvMap,
            BRDFxLUT, Sampler, PreIntegratedSampler), 1);
    }
}

float3 CalcPointLight(GBufferAttributes attrs);
float3 CalcDirLight(GBufferAttributes attrs);

#if MSAA
float4 PSDirectionalLight(in VSQuadOut input, uint coverage : SV_Coverage, uint sampleIndex : SV_SampleIndex) : SV_Target
#else
float4 PSDirectionalLight(in VSQuadOut input) : SV_Target
#endif
{
    GBufferAttributes attrs;
    #if MSAA
    if (!(coverage & (1 << sampleIndex)))
    {
        discard;
        return 0;
    }
    ExtractGBufferAttributes(input.Position.xyz, input.PosProj.xy, sampleIndex, attrs);
    #else
    ExtractGBufferAttributes(input.Position.xyz, input.PosProj.xy, 0, attrs);
    #endif
    
    if (attrs.Unlit)
    {
        discard;
    }

    return float4(CalcDirLight(attrs), 1);
}

#if SQUAD 
    #if MSAA
float4 PSPointLight(in VSQuadOut input, in uint coverage : SV_Coverage, in uint sampleIndex : SV_SampleIndex) : SV_Target
    #else
float4 PSPointLight(in VSQuadOut input) : SV_Target
    #endif
#else
    #if MSAA
float4 PSPointLight(in DS_OUTPUT input, in uint coverage : SV_Coverage, in uint sampleIndex : SV_SampleIndex) : SV_TARGET
#else
float4 PSPointLight(in DS_OUTPUT input) : SV_Target
    #endif
#endif
{
    GBufferAttributes attrs;

    #if SQUAD 
    float2 uv = input.PosProj.xy;
    #else
    float2 uv = input.PosProj.xy / input.PosProj.w;
    #endif

    //return float4(uv, 0, 1);

    #if MSAA
    if (!(coverage & (1 << sampleIndex)))
    {
        discard;
        return 0;
    }
    ExtractGBufferAttributes(input.Position.xyz, uv, sampleIndex, attrs);
    #else
    ExtractGBufferAttributes(input.Position.xyz, uv, 0, attrs);
    #endif

    if (attrs.Unlit)
    {
        discard;
    }
    
    return float4(CalcPointLight(attrs), 1);
}

/////////////////////////////////////////////////////////////////////////////
// Functions
/////////////////////////////////////////////////////////////////////////////
float GetAttenuation(float DistancePow2, float lightInnerR, float invLightOuterR)
{
    float d = max(DistancePow2, lightInnerR * lightInnerR);
    float DistanceAttenuation = 1 / (d + 1);
    DistanceAttenuation *= Pow2(saturate(1 - Pow2(d * Pow2(invLightOuterR))));
    return DistanceAttenuation;
}

float3 Specular_BRDF(float a2, float3 c_spec, BxDFContext context)
{
    // Unity paper
    float a22 = Pow2(a2);
    return a22 / (4 * PI * Pow2(Pow2(context.NoH) * (a22 - 1) + 1)) * Pow2(context.LoH) * (sqrt(a2) + 0.5) * c_spec;
    // UE4 source
    float specular_D = D_GGX(a2, context.NoH);
    float3 specular_F = F_Schlick(c_spec, context.VoH);
    float specular_G = Vis_SmithJointApprox(a2, context.NoV, context.NoL);
    return (specular_D * specular_F * specular_G);
    /* from ue4 sigraph paper
    specular_D = Pow2(a2) / Pow2(PI * (Pow2(context.NoH) * (Pow2(a2) - 1) + 1));
    float k = Pow2(sqrt(a2) + 1) / 8;
    specular_G = context.NoV / (context.NoV * (1 - k) + k);
    specular_G *= context.NoL / (context.NoL * (1 - k) + k);

    specular_F = c_spec + (1 - c_spec) * pow(2, (-5.55473 * context.VoH - 6.98316 * context.VoH));

    return (specular_D * specular_F * specular_G) / (4 * context.NoL * context.NoV);*/
}

float3 BRDF(float3 c_diff, float a2, float3 c_spec, BxDFContext context)
{
    return Diffuse_Lambert(c_diff) + Specular_BRDF(a2, c_spec, context);
}

float3 CalcDirLight(GBufferAttributes attrs)
{
    float3 L = -normalize(PerLightBuffer.Direction);
    float3 V = normalize(PerFrameBuffer.CameraPosition - attrs.Position);
    
    BxDFContext context;
    Init(context, attrs.Normal, V, L);
    context.NoV = saturate(abs(context.NoV) + 1e-4);
    //SphereMaxNoH(context, 0, true);

    const float3 c_diff = lerp(attrs.AlbedoColor, float3(0, 0, 0), attrs.MetallicValue);
    const float3 c_spec = lerp(kSpecularCoefficient, attrs.AlbedoColor, attrs.MetallicValue);
    
    float a2 = Pow4(max(attrs.RoughnessValue, 0.04f));
    float3 acc_color = context.NoL * BRDF(c_diff, a2, c_spec, context);
    acc_color *= PerLightBuffer.Intensity * PerLightBuffer.Color;

    return acc_color;
}

float3 CalcPointLight(GBufferAttributes attrs)
{
    float3 L = PerLightBuffer.Position - attrs.Position;
    float3 V = normalize(PerFrameBuffer.CameraPosition - attrs.Position);

    float DistancePow2 = length(L);
    L /= DistancePow2;
    DistancePow2 *= DistancePow2;
    
    float Attenuation = GetAttenuation(DistancePow2, 0.25, 1 / PerLightBuffer.Radius) * attrs.AOValue;
    
    BxDFContext context;
    Init(context, attrs.Normal, V, L);

    const float3 c_diff = lerp(attrs.AlbedoColor, float3(0, 0, 0), attrs.MetallicValue);
    const float3 c_spec = lerp(kSpecularCoefficient, attrs.AlbedoColor, attrs.MetallicValue);
    
    float a2 = Pow4(max(attrs.RoughnessValue, 0.04f));
    float3 acc_color = context.NoL * BRDF(c_diff, a2, c_spec, context);
    acc_color *= PerLightBuffer.Intensity * PerLightBuffer.Color * Attenuation;

    return acc_color;
}