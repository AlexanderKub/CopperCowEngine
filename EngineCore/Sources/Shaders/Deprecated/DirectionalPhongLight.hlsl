struct Light
{
    float4 ambientColor;
    float4 diffuseColor;
    float4 specularColor;
    float type;
    float4 eyeWorldPosition;
    float4x4 lightViewProjMatrix;
    float3 position;
    float3 direction;
    float distanceSqr;
    float hasNormalMap;
    float hasRoughnessMap;
    float hasAOMap;
    float recieveShadows;
};

float4 GetLightColor(float3 worldPosition, float3 normal, Light LightBuffer, float shadowDepth)
{
    float3 n = normalize(normal);
    float3 l = normalize(LightBuffer.position.xyz - worldPosition); //to light dir

    float diffuseLightIntensity = max(dot(l, n), 0.0f);
    float4 diffuseLightComponent = saturate(LightBuffer.diffuseColor * diffuseLightIntensity);

    float3 reflectVector = normalize(reflect(l, n));
    float3 v = normalize(worldPosition - LightBuffer.eyeWorldPosition.xyz); //to eye dir
    float3 h = (l + v) / length(l + v);

    float specularLightIntensity = pow(saturate(dot(h, v)), 1);
    float4 specularLightComponent = LightBuffer.specularColor * specularLightIntensity;

    return LightBuffer.ambientColor + (diffuseLightIntensity + specularLightComponent) * shadowDepth;
}

float4 getDirectionalLight(float3 worldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float3 specularColor, float2 roughnessValue, float aOValue)
{
    return GetLightColor(worldPosition, normal, LightBuffer, shadowDepth);
}

float4 getPointLight(float3 worldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float3 specularColor, float2 roughnessValue, float aOValue)
{
    return GetLightColor(worldPosition, normal, LightBuffer, shadowDepth);
}

float4 getSpotLight(float3 worldPosition, float3 normal, Light LightBuffer, float shadowDepth,
     float3 diffuseColor, float3 specularColor, float2 roughnessValue, float aOValue)
{
    return GetLightColor(worldPosition, normal, LightBuffer, shadowDepth);
}