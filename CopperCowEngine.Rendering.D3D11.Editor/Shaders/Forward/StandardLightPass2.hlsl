#include "../Include/ConstantBuffers.hlsl"
#include "../Include/Samplers.hlsl"
#include "../Include/Layouts.hlsl"
#include "../Include/OutputLayouts.hlsl"
#include "../Include/ColorSpace.hlsl"
#include "../Include/TextureMapsHelpers.hlsl"
#include "../Include/ColorSpace.hlsl"
#include "../Include/Material.hlsl"
#include "../PBR/BRDF.hlsl"

cbuffer cbPerLightBatchBuffer : register(b0)
{
    ConstBufferPerLightBatchStruct cbPerLightBatch;
}

struct LightParams
{
    float Type;
    float3 Params;
    
    float3 Center;
    float InverseRange;
    
    float3 Color;
    float Intensity;
};

#if !NON_TEXTURED
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);
Texture2D EmissiveMap : register(t5);
#endif
//t6 t7 t8 - IBL Textures
StructuredBuffer<LightParams> LightsBuffer : register(t9);

#if !NON_TEXTURED
SamplerState Sampler : register(s0);
#endif

COMMON_PS_IN VSMain(COMMON_VS_IN Input)
{
    COMMON_PS_IN Output = (COMMON_PS_IN) 0;
    
    Output.pos = mul(float4(Input.pos, 1), cbPerObject.WorldViewProjection);
    Output.posWS = mul(float4(Input.pos, 1), cbPerObject.World).xyz;
    
    Output.uv0 = Input.uv0.xy * cbPerMaterial.TextureTiling + cbPerMaterial.TextureShift;
    Output.uv1 = Input.uv1;
    Output.color = Input.color;

    Output.normal = mul(float4(Input.normal, 0), cbPerObject.World).xyz;
    Output.tangent = mul(float4(Input.tangent, 0), cbPerObject.World).xyz;
    
    return Output;
};

void ExtractMaterial(const COMMON_PS_IN Input, inout PerPixel OUT1, inout PerMaterial OUT)
{
    OUT.Albedo = cbPerMaterial.AlbedoColor.rgb;
    OUT.Albedo = SRGBToLinear(OUT.Albedo.rgb);
    OUT.Alpha = cbPerMaterial.AlbedoColor.a;
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.r > 0)
    {
        float4 sampledAlbedo = AlbedoMap.Sample(Sampler, Input.uv0.xy);
        OUT.Albedo = sampledAlbedo.rgb;
        OUT.Alpha = sampledAlbedo.a;
    }
#endif
#if LDR
    OUT.Albedo = saturate(OUT.Albedo);
#endif
    
    OUT1.N = normalize(Input.normal.xyz);
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.g > 0)
    {
        float3 sampleNormal = NormalMap.Sample(Sampler, Input.uv0.xy).xyz;
        OUT1.N = ApplyNormalMap(OUT1.N, normalize(Input.tangent.xyz), sampleNormal);
    }
#endif
    
    OUT.Roughness = cbPerMaterial.RoughnessValue;
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.b > 0)
    {
        OUT.Roughness = RoughnessMap.Sample(Sampler, Input.uv0.xy).r;
    }
#endif
    
    OUT.Metallic = cbPerMaterial.MetallicValue;
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.a > 0)
    {
        OUT.Metallic = MetallicMap.Sample(Sampler, Input.uv0.xy).r;
    }
#endif
    
    OUT.Occlusion = 1.0f;
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask1.r > 0)
    {
        OUT.Occlusion = OcclusionMap.Sample(Sampler, Input.uv0.xy).r;
    }
#endif
    OUT.Emissive = cbPerMaterial.EmissiveColor;
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask1.g > 0)
    {
        OUT.Emissive = EmissiveMap.Sample(Sampler, Input.uv0.xy).r;
    }
#endif
    OUT.Reflectance = cbPerMaterial.Reflectance;
    
    OUT1.PositionWS = Input.posWS.xyz;
    OUT1.V = normalize(cbPerFrame.CameraPosition.xyz - OUT1.PositionWS);
}

OutputStruct PSMain(COMMON_PS_IN Input)
{
    OutputStruct OUT = (OutputStruct) 0;
    
    PerPixel pixel;
    PerMaterial material;
    ExtractMaterial(Input, pixel, material);
    
    PerPixelBrdfContext pixelContext;
    PrepareBrdfContext(pixel, material, pixelContext);
    
    float3 directLighting = 0.0;
    for (uint i = 0u; i < 4u; i++)
    {
        uint lightIndex = cbPerLightBatch.LightsIndices[i];
        LightParams light = LightsBuffer[lightIndex];
        
        if (light.Type > 0)
        {
            PerLight lightData = (PerLight) 0;
            lightData.Color = SRGBToLinear(light.Color);
            lightData.Intensity = light.Intensity;
            lightData.L = -normalize(light.Center);
        
            PerLightBrdfContext lightContext;
            PrepareBrdfContext(pixel, lightData, lightContext);
        
            directLighting += Shading(pixelContext, lightContext, lightData, material);
        }
    }
    
    float3 finalColor = directLighting;
    finalColor += IBLLight(pixel, pixelContext, material) * material.Occlusion;
    finalColor += material.Emissive.rgb;
#if LDR
    finalColor = saturate(finalColor);
#if SRGB
    finalColor = pow(abs(finalColor), 1.0 / 2.2);
#endif
#endif
    
    OUT.Target0 = float4(finalColor, material.Alpha);
    OUT.Target1 = pixel.N * 0.5 + 0.5;
    return OUT;
}