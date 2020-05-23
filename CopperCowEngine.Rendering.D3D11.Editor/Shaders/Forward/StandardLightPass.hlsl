#include "../Include/ConstantBuffers.hlsl"
#include "../Include/Samplers.hlsl"
#include "../Include/LightsStructures.hlsl"
#include "../Include/Layouts.hlsl"
#include "../Include/OutputLayouts.hlsl"
#include "../Include/ColorSpace.hlsl"
#include "../Include/TextureMapsHelpers.hlsl"
#include "../Include/ColorSpace.hlsl"
#include "../PBR/LightSurface.hlsl"

cbuffer cbPerLightBatchBuffer : register(b0)
{
    ConstBufferPerLightBatchStruct cbPerLightBatch;
}

#if !NON_TEXTURED
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);
Texture2D EmissiveMap : register(t5);
#endif
TextureCube EnvMap : register(t6);
TextureCube PrefilteredEnvMap : register(t7);
TextureCube IrradianceEnvMap : register(t8);
Texture2D<float2> BRDFxLUT : register(t9);
StructuredBuffer<LightParams> LightsBuffer : register(t10);

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

Material ExtractMaterial(COMMON_PS_IN Input)
{
    Material OUT = (Material) 0;
    
    OUT.Albedo = cbPerMaterial.AlbedoColor;
    OUT.Albedo = float4(SRGBToLinear(OUT.Albedo.rgb), OUT.Albedo.a);
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.r > 0)
    {
        OUT.Albedo = AlbedoMap.Sample(Sampler, Input.uv0.xy);
    }
#endif
#if LDR
    OUT.Albedo = saturate(OUT.Albedo);
#endif
    
    OUT.NormalWS = normalize(Input.normal.xyz);
#if !NON_TEXTURED
    if (cbPerMaterial.OptionsMask0.g > 0)
    {
        float3 sampleNormal = NormalMap.Sample(Sampler, Input.uv0.xy).xyz;
        OUT.NormalWS = ApplyNormalMap(OUT.NormalWS, normalize(Input.tangent.xyz), sampleNormal);
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
    
    OUT.Unlit = cbPerMaterial.OptionsMask1.b;
    OUT.NonShadow = cbPerMaterial.OptionsMask1.a;
    
    OUT.PositionWS = Input.posWS.xyz;
    OUT.V = normalize(cbPerFrame.CameraPosition.xyz - Input.posWS.xyz);
    
    return OUT;
}

float3 CalculateLight(Material material, LightParams light)
{
    light.Color = SRGBToLinear(light.Color);
#if LDR
    light.Color = saturate(light.Color);
    light.Intensity = saturate(light.Intensity);
#endif
    return DirectionalLight(material, light);
    //return PointLight(material, light);
}

OutputStruct PSMain(COMMON_PS_IN Input)
{
    OutputStruct OUT = (OutputStruct) 0;
    
    Material material = ExtractMaterial(Input);
    
    if (material.Unlit > 0)
    {
        OUT.Target0 = material.Albedo;
        return OUT;
    }
    
    float3 finalColor = 0;
    
    uint lightIndex0 = cbPerLightBatch.LightsIndices.x;
    LightParams light = LightsBuffer[lightIndex0];
    if (light.Type > 0)
    {
        finalColor += CalculateLight(material, light);
    }
    
    uint lightIndex1 = cbPerLightBatch.LightsIndices.y;
    light = LightsBuffer[lightIndex1];
    if (light.Type > 0)
    {
        finalColor += CalculateLight(material, light);
    }
    
    uint lightIndex2 = cbPerLightBatch.LightsIndices.z;
    light = LightsBuffer[lightIndex2];
    if (light.Type > 0)
    {
        finalColor += CalculateLight(material, light);
    }
    
    uint lightIndex3 = cbPerLightBatch.LightsIndices.a;
    light = LightsBuffer[lightIndex3];
    if (light.Type > 0)
    {
        finalColor += CalculateLight(material, light);
    }
    
    //finalColor += IBLLightSurface(material, EnvMap, PrefilteredEnvMap, IrradianceEnvMap, BRDFxLUT) * material.Occlusion;
    finalColor += material.Emissive.rgb;
    
#if LDR
    finalColor = saturate(finalColor);
#endif
    
    OUT.Target0 = float4(finalColor, material.Albedo.a);
    return OUT;
}