#include "../Include/Constants.hlsl"
#include "../Include/Structures.hlsl"

float3 Fresnel_Shlick(in float3 f0, in float3 f90, in float x)
{
    return f0 + (f90 - f0) * pow(1.f - x, 5.f);
}

float Diffuse_Burley(in float NdotL, in float NdotV, in float LdotH, in float roughness)
{
    float fd90 = 0.5f + 2.f * roughness * LdotH * LdotH;
    return Fresnel_Shlick(1, fd90, NdotL).x * Fresnel_Shlick(1, fd90, NdotV).x;
}

float Specular_D_GGX(in float alpha, in float NdotH)
{
    const float alpha2 = alpha * alpha;
    const float lower = (NdotH * NdotH * (alpha2 - 1)) + 1;
    return alpha2 / max(EPSILON, PI * lower * lower);
}

float G_Shlick_Smith_Hable(float alpha, float LdotH)
{
    return rcp(lerp(LdotH * LdotH, 1, alpha * alpha * 0.25f));
}

float3 Specular_BRDF(in float alpha, in float3 specularColor, in float NdotV, in float NdotL, in float LdotH, in float NdotH)
{
    float specular_D = Specular_D_GGX(alpha, NdotH);
    
    float3 specular_F = Fresnel_Shlick(specularColor, 1, LdotH);
    
    float specular_G = G_Shlick_Smith_Hable(alpha, LdotH);

    return specular_D * specular_F * specular_G;
}

float3 Diffuse_IBL(in float3 N, TextureCube IrradianceTexture, SamplerState IBLSampler)
{
    return IrradianceTexture.Sample(IBLSampler, N).rgb;
}

float3 Specular_IBL(in float3 N, in float3 V, in float lodBias, TextureCube RadianceTexture, SamplerState IBLSampler)
{
    int NumRadianceMipLevels = 3;
    float mip = lodBias * NumRadianceMipLevels;
    float3 dir = reflect(-V, N);
    return RadianceTexture.SampleLevel(IBLSampler, dir, mip).rgb;
}

float3 LightSurface(float3 V, float3 N, int numLights, LightBuffer lightData[MaxLightsCount], float3 albedo, float roughness, 
    float metallic, float ambientOcclusion, TextureCube RadianceTexture, TextureCube IrradianceTexture, SamplerState IBLSampler, float3 position)
{
    static const float kSpecularCoefficient = 0.04;

    const float NdotV = saturate(dot(N, V));
    
    const float alpha = roughness * roughness;
    
    const float3 c_diff = lerp(albedo, float3(0, 0, 0), metallic) * ambientOcclusion;
    const float3 c_spec = lerp(kSpecularCoefficient, albedo, metallic) * ambientOcclusion;
    
    float3 acc_color = 0;
    
    [unroll(MaxLightsCount)]
    for (int i = 0; i < numLights; i++)
    {
        const float3 L = normalize(-lightData[i].direction);
        const float3 H = normalize(L + V);
        
        const float NdotL = saturate(dot(N, L));
        const float LdotH = saturate(dot(L, H));
        const float NdotH = saturate(dot(N, H));
    
        float lightTypeCoeff = 1.0f;
        if (lightData[i].type == 1)
        {
            float3 lightDirection = -normalize(lightData[i].direction);
            float theta = 0.8;
            lightTypeCoeff = (dot(lightDirection, normalize(lightData[i].position.xyz - position.xyz)) > theta);
            float distanceLightCoef = lightData[i].distanceSqr / dot(lightData[i].position.xyz - position.xyz, lightData[i].position.xyz - position.xyz);
            lightTypeCoeff *= distanceLightCoef;

        }
        else if (lightData[i].type == 2)
        {
            lightTypeCoeff = lightData[i].distanceSqr / dot(lightData[i].position.xyz - position.xyz, lightData[i].position.xyz - position.xyz) * 0.35;
        }
        
        float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, roughness);
        float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH);
        
        acc_color += NdotL * lightTypeCoeff * lightData[i].lightTint.rgb * (((c_diff * diffuse_factor) + specular));
    }
    
    float3 diffuse_env = Diffuse_IBL(N, IrradianceTexture, IBLSampler);
    acc_color += c_diff * diffuse_env;
    
    float3 specular_env = Specular_IBL(N, V, roughness, RadianceTexture, IBLSampler);
    acc_color += c_spec * specular_env;

    return acc_color;
}