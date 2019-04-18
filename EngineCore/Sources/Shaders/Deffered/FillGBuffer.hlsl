#include "./Include/Layouts.hlsl"
#include "../CommonsInclude/StandardHeaders.hlsl"

#if TEXTURE_CUBE_ALBEDO_MAP
TextureCube<float4> AlbedoMap : register(t0);
#else
Texture2D AlbedoMap : register(t0);
#endif

Texture2D MetallicMap : register(t1);

Texture2D EmissiveMap : register(t2);
Texture2D RoughnessMap : register(t3);

Texture2D NormalMap : register(t4);

Texture2D SpecularMap : register(t5);
Texture2D AmbientOcclusionMap : register(t6);

SamplerState Sampler : register(s0);

GBufferPixelIn FillGBufferVS(VertexShaderInput vertex)
{
    GBufferPixelIn result = (GBufferPixelIn) 0;
    vertex.Position.w = 1.0f;
    result.Position = vertex.Position;
    result.Position = mul(result.Position, PerObjectBuffer.World);
    result.Position = mul(result.Position, PerFrameBuffer.View);
    result.Position = mul(result.Position, PerFrameBuffer.Projection);

    result.TextureUV = vertex.UV0;
    result.TextureUV *= PerMaterialBuffer.textureTiling;
    result.TextureUV += PerMaterialBuffer.textureShift;

    result.WorldNormal = mul(vertex.Normal, PerObjectBuffer.World).xyz;
    result.WorldTangent = mul(vertex.Tangent, (float3x3) PerObjectBuffer.World).xyz;
    return result;
}

void FillGBuffer(float2 uv, float3 vnormal, float3 vtangent, out GBufferOutput result);

GBufferOutput FillGBufferPS(GBufferPixelIn pixel)
{
    #if MASKED
    float alpha = PerMaterialBuffer.AlbedoColor.a;
    if (PerMaterialBuffer.optionsMask0.r > 0)
    {
        #if TEXTURE_CUBE_ALBEDO_MAP
        alpha = AlbedoMap.Sample(Sampler, -pixel.WorldNormal).a;
        #else
        alpha = AlbedoMap.Sample(Sampler, pixel.TextureUV).a;
        #endif
    }
    if (alpha < PerMaterialBuffer.AlphaClip)
    {
        discard;
    }
    #endif

    GBufferOutput result = (GBufferOutput) 0;
    FillGBuffer(pixel.TextureUV, pixel.WorldNormal, pixel.WorldTangent, result);
    return result;
}


float3 ApplyNormalMap(float3 normal, float3 tangent, float3 normalSample)
{
    // Remap normalSample to the range -1,1
    normalSample = (2.0 * normalSample) - 1.0;

    // Ensure tangent is orthogonal to normal vector - Gram-Schmidt orthogonalize
    float3 T = normalize(tangent.xyz - normal * dot(normal, tangent.xyz));

    // Create the Bitangent (tangent.w contains handedness)
    float3 bitangent = cross(normal, T);

    // Create the TBN matrix to transform from tangent space
    float3x3 TBN = float3x3(T, bitangent, normal);

    return normalize(mul(normalSample, TBN));
}

float3 SRGBToLinear(float3 srgb)
{
    return pow(abs(srgb), 2.2);
}

void FillGBuffer(float2 uv, float3 vnormal, float3 vtangent, out GBufferOutput result)
{
    float3 albedo = SRGBToLinear(PerMaterialBuffer.AlbedoColor.rgb);
    if (PerMaterialBuffer.optionsMask0.r > 0)
    {
        #if TEXTURE_CUBE_ALBEDO_MAP
        albedo = AlbedoMap.Sample(Sampler, -vnormal).rgb;
        #else
        albedo = AlbedoMap.Sample(Sampler, uv).rgb;
        #endif
    }

    float metallic = PerMaterialBuffer.MetallicValue;
    if (PerMaterialBuffer.optionsMask0.g > 0)
    {
        metallic = MetallicMap.Sample(Sampler, uv).r;
    }

    float3 emissive = PerMaterialBuffer.EmissiveColor.rgb;
    if (PerMaterialBuffer.optionsMask0.b > 0)
    {
        emissive = EmissiveMap.Sample(Sampler, uv).rgb;
    }

    float roughness = PerMaterialBuffer.RoughnessValue;
    if (PerMaterialBuffer.optionsMask0.a > 0)
    {
        roughness = RoughnessMap.Sample(Sampler, uv).r;
    }

    float specular = PerMaterialBuffer.SpecularValue;
    if (PerMaterialBuffer.optionsMask1.g > 0)
    {
        specular = SpecularMap.Sample(Sampler, uv).r;
    }

    float occlusion = 1;
    if (PerMaterialBuffer.optionsMask1.b > 0)
    {
        occlusion = AmbientOcclusionMap.Sample(Sampler, uv).r;
    }
    
    float3 normal = normalize(vnormal);
    float3 tangent = normalize(vtangent);
    if (PerMaterialBuffer.optionsMask1.r > 0)
    {
        float3 normalMap = NormalMap.Sample(Sampler, uv).xyz;
        normal = ApplyNormalMap(normal, tangent, normalMap);
    }
    normal = normalize(normal);
    
    result.AlbedoAndMetallic.xyz = albedo;
    result.AlbedoAndMetallic.w = metallic;
    result.EmissiveAndRoughness.xyz = emissive;
    result.EmissiveAndRoughness.w = roughness;
    
    result.PackedNormals = float4(normal * 0.5 + 0.5, 0);

    result.SpecularAOUnlitNonShadows.r = specular;
    result.SpecularAOUnlitNonShadows.g = occlusion;
    result.SpecularAOUnlitNonShadows.b = PerMaterialBuffer.MaterialID;
    result.SpecularAOUnlitNonShadows.a = PerMaterialBuffer.optionsMask1.a;
}
