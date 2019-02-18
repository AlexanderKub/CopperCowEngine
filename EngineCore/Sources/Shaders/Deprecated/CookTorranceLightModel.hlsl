struct Light
{
    float4 ambientColor;
    float4 diffuseColor;
    float4 specularColor;
    float type;
    float3 position;
    float3 direction;
    float distanceSqr;
    float hasNormalMap;
    float hasRoughnessMap;
    float hasAOMap;
    float recieveShadows;
};

float4 GetLightColor(float3 worldPosition, float3 eyeWorldPosition, float3 normal, Light LightBuffer, float lightTypeCoef,
    float shadowDepth, float3 diffuseColor, float specularColor, float2 roughnessValue, float aOValue)
{
    roughnessValue.r *= 3.0f;
    
    float3 N = normalize(normal);
  
    float3 V = normalize(worldPosition - eyeWorldPosition.xyz); //to eye dir
    float3 L = normalize(LightBuffer.position.xyz - worldPosition); //to light dir
    float3 H = normalize(L + V);

    float NdotH = dot(N, H);
    float VdotH = dot(H, V);
    float NdotV = dot(N, V);
    float NdotL = dot(N, L);
  
    float G1 = (2.0f * NdotH * NdotV) / VdotH;
    float G2 = (2.0f * NdotH * NdotL) / VdotH;
    float G = min(1.0f, max(0.0f, min(G1, G2)));
  
    float F = roughnessValue.g + (1.0f - roughnessValue.g) * pow(1.0f - NdotV, 5.0f);
  
    float R_2 = roughnessValue.r * roughnessValue.r;
    float NDotH_2 = NdotH * NdotH;
    float A = 1.0f / (4.0f * R_2 * NDotH_2 * NDotH_2);
    float B = exp(-(1.0f - NDotH_2) / (R_2 * NDotH_2));
    float R = A * B;
  
    float3 S = LightBuffer.specularColor.rgb * specularColor * ((G * F * R) / (NdotL * NdotV));
    float3 Final = diffuseColor * LightBuffer.ambientColor.rgb;
    Final += LightBuffer.diffuseColor.rgb * (max(0.0f, NdotL) * lightTypeCoef) * (diffuseColor + S) * shadowDepth;
    Final *= aOValue;
 
    return float4(Final, 1.0f);
}

float4 getDirectionalLight(float3 worldPosition, float3 eyeWorldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float specularColor, float2 roughnessValue, float aOValue)
{
    return GetLightColor(worldPosition, eyeWorldPosition, normal, LightBuffer, 1.0, shadowDepth,
        diffuseColor, specularColor, roughnessValue, aOValue);
}

float4 getPointLight(float3 worldPosition, float3 eyeWorldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float specularColor, float2 roughnessValue, float aOValue)
{
    float distanceSqr = 100;
    float pointLightCoef = LightBuffer.distanceSqr / dot(LightBuffer.position.xyz - worldPosition, 
        LightBuffer.position.xyz - worldPosition);

    return GetLightColor(worldPosition, eyeWorldPosition, normal, LightBuffer, pointLightCoef, shadowDepth,
        diffuseColor, specularColor, roughnessValue, aOValue);
}

float4 getSpotLight(float3 worldPosition, float3 eyeWorldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float specularColor, float2 roughnessValue, float aOValue)
{
    float3 lightDirection = -normalize(LightBuffer.direction);
    float theta = 0.8;
   
    float spotLightCoef = (dot(lightDirection, normalize(LightBuffer.position.xyz - worldPosition)) > theta);
    float distanceLightCoef = LightBuffer.distanceSqr / dot(LightBuffer.position.xyz - worldPosition, 
        LightBuffer.position.xyz - worldPosition);
    spotLightCoef *= distanceLightCoef;

    return GetLightColor(worldPosition, eyeWorldPosition, normal, LightBuffer, spotLightCoef, shadowDepth,
        diffuseColor, specularColor, roughnessValue, aOValue);
}