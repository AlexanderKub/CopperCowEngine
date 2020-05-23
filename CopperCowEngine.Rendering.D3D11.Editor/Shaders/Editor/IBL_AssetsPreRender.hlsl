struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float3 UV : POSITION;
};

struct CBufferCubeStruct
{
    float4x4 View;
    float4x4 Projection;
};

cbuffer cbCubeBuffer : register(b0)
{
    CBufferCubeStruct cbCube;
};

static const float PI = 3.14159265359;
static const float EPSILON = 1e-4f;

/// ------------------------ Spherical map convertation -------------------------- ///
PixelShaderInput VS_SphereToCubeMap(uint VertexID : SV_VertexID)
{
    PixelShaderInput result = (PixelShaderInput) 0;
    
    // extract vertices
    int r = int(VertexID > 6);
    int i = r == 1 ? 13 - VertexID : VertexID;
    int x = int(i < 3 || i == 4);
    int y = r ^ int(i > 0 && i < 4);
    int z = r ^ int(i < 2 || i > 5);

    result.UV = float3(x, y, z) * 2.0 - 1.0;
    result.Position = mul(float4(result.UV, 1), mul(cbCube.View, cbCube.Projection));
    result.Position = result.Position.xyww;

    return result;
}

static const float2 invAtan = float2(0.1591, 0.3183);
float2 SampleSphericalMap(float3 v)
{
    float2 uv = float2(atan2(v.z, v.x), asin(v.y));
    uv *= invAtan;
    uv += 0.5;
    uv = 1 - uv;
    return uv;
}

Texture2D<float3> EquirectangularMap : register(t0);
SamplerState Sampler : register(s0);

float3 LinearToSRGB(float3 color)
{
    return pow(abs(color), 1 / 2.2);
}

float3 SRGBToLinear(float3 srgb)
{
    return pow(abs(srgb), 2.2);
}

float4 PS_SphereToCubeMap(PixelShaderInput pixel) : SV_Target
{
    float2 uv = SampleSphericalMap(normalize(pixel.UV));
    float3 color = EquirectangularMap.Sample(Sampler, uv);
    
    return float4(color, 1);
}
/// ------------------------------------------------------------------------------ ///


/// ------------------------ Irradiance map calculation -------------------------- ///
TextureCube<float4> EnvironmentMap : register(t1);

float4 PS_Irradiance(PixelShaderInput pixel) : SV_Target
{
    float3 normal = normalize(pixel.UV);
  
    float3 irradiance = 0.0;
    
    // tangent space calculation from origin point
    float3 up = float3(0.0, 1.0, 0.0);
    float3 right = normalize(cross(up, normal));
    up = cross(normal, right);

    float sampleDelta = 0.025;
    float sampleDeltaTheta = 0.015;
    float nrSamples = 0.0;
    for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDeltaTheta)
        {
            // spherical to cartesian (in tangent space)
            float sinTheta = sin(theta);
            float3 tangentSample = float3(sinTheta * cos(phi), sinTheta * sin(phi), cos(theta));

            // tangent space to world
            float3 sampleVector = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal;
            
            irradiance += EnvironmentMap.Sample(Sampler, sampleVector).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }
    irradiance = PI * irradiance * (1.0 / float(nrSamples));
    
    return float4(irradiance, 1);
}
/// ------------------------------------------------------------------------------ ///


/// ------------------------ Prefiltered map calculation ------------------------- ///
struct CBufferBRDFStruct
{
    float Roughness;
    float3 filler;
};

cbuffer cbBRDFBuffer : register(b1)
{
    CBufferBRDFStruct cbBRDF;
}

float RadicalInverse_VdC(uint bits);
float2 Hammersley(uint i, uint N);
float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness);
float DistributionGGX(in float NdotH, in float alpha);

float4 PS_PreFiltered(PixelShaderInput pixel) : SV_Target
{
    float3 N = normalize(pixel.UV);
    float3 V = N;
    
    float roughness = cbBRDF.Roughness;
    
    const uint SAMPLE_COUNT = 1024u;
    float totalWeight = 0.0;
    float3 prefilteredColor = 0.0;

    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, roughness);
        float3 L = normalize(2.0 * dot(V, H) * H - V);
        
        float NdotH = saturate(dot(N, H));
        float HdotV = saturate(dot(H, V));
        float NdotL = saturate(dot(N, L));
        
        if (NdotL > 0.0)
        {
            float D = DistributionGGX(NdotH, roughness);
            float pdf = (D * NdotH / (4.0 * HdotV)) + 0.0001f;
        
            float resolution = 1024.0;
            float saTexel = 4.0 * PI / (6.0 * resolution * resolution);
            float saSample = 1.0 / (float(SAMPLE_COUNT) * pdf + 0.0001f);
        
            float mipLevel = roughness == 0.0 ? 0.0 : 0.5 * log2(saSample / saTexel);

            prefilteredColor += EnvironmentMap.SampleLevel(Sampler, L, mipLevel).rgb * NdotL;
            totalWeight += NdotL;
        }
    }
    return float4(prefilteredColor / max(totalWeight, 0.001f), 1);
}
/// ------------------------------------------------------------------------------ ///


/// ------------------------ BRDF integrate -------------------------------------- ///
PixelShaderInput VS_IntegrateQuad(uint VertexID : SV_VertexID)
{
    PixelShaderInput result = (PixelShaderInput) 0;
    
    // extract vertices
    result.UV.xy = float2((VertexID << 1) & 2, VertexID & 2);
    result.Position = float4(result.UV.xy * float2(2, -2) + float2(-1, 1), 1, 1);

    return result;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness);

float2 IntegrateBRDF(float NdotV, float roughness)
{
    float2 res = (float2) 0.0f;

    float3 V = float3(sqrt(1.0 - NdotV * NdotV), 0.0, NdotV);
    float3 N = float3(0.0, 0.0, 1.0);
    
    roughness = max(0.02f, roughness);

    const uint SAMPLE_COUNT = 1024u;
    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, roughness);
        float3 L = normalize(2.0 * dot(V, H) * H - V);

        float NdotL = saturate(L.z);
        float NdotH = saturate(H.z);
        float VdotH = saturate(dot(V, H));

        if (NdotL > 0.0)
        {
            float G = GeometrySmith(N, V, L, roughness);
            float G_Vis = (G * VdotH) / (NdotH * NdotV);
            float Fc = pow(1.0 - VdotH, 5.0);

            res.x += (1.0 - Fc) * G_Vis;
            res.y += Fc * G_Vis;
        }
    }
    res /= float(SAMPLE_COUNT);
    return res;
}

float GDFG(float NoV, float NoL, float a)
{
    float a2 = a * a;
    float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
    float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
    return (2 * NoL) / (GGXV + GGXL);
}

float2 DFG(float NoV, float a)
{
    const uint SAMPLE_COUNT = 1024u;
    float3 V = float3(sqrt(1.0f - NoV * NoV), 0.0f, NoV);

    float2 r = 0.0f;
    for (uint i = 0; i < SAMPLE_COUNT; i++)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, NoV, a);
        float3 L = normalize(2.0f * dot(V, H) * H - V);

        float VoH = saturate(dot(V, H));
        float NoL = saturate(L.z);
        float NoH = saturate(H.z);

        if (NoL > 0.0f)
        {
            float G = GDFG(NoV, NoL, a);
            float Gv = G * VoH / NoH;
            float Fc = pow(1 - VoH, 5.0f);
            
            r.x += Gv * (1 - Fc);
            r.y += Gv * Fc;
        }
    }
    return r * (1.0f / SAMPLE_COUNT);
}


float4 PS_IntegrateBRDF(PixelShaderInput pixel) : SV_Target
{
    //float2 integratedBRDF = IntegrateBRDF(pixel.UV.x, 1 - pixel.UV.y);
    float2 integratedBRDF = DFG(pixel.UV.x, 1 - pixel.UV.y);
    return float4(integratedBRDF, 0, 1);
}
/// ------------------------------------------------------------------------------ ///


/// ------------------------ Functions -------------------------------------- ///
float DistributionGGX(in float NdotH, in float alpha)
{
    const float alpha2 = alpha * alpha;
    const float lower = (NdotH * NdotH * (alpha2 - 1)) + 1;
    return alpha2 / max(EPSILON, PI * lower * lower);
}

float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
{
    float a = roughness * roughness;
	
    float phi = 2.0 * PI * Xi.x;
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
	
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
	
    float3 up = abs(N.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
    float3 tangent = normalize(cross(up, N));
    float3 bitangent = cross(N, tangent);
	
    float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float a = roughness;
    float k = (a * a) / 2.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = saturate(dot(N, V));
    float NdotL = saturate(dot(N, L));
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}
/// ------------------------------------------------------------------------------ ///
