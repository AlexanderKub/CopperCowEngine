float3 GetShadowMapCoordinates(float4 lightViewPosition)
{
    float2 lightProjectTexCoord;
    //lightViewPosition = normalize(lightViewPosition);
    lightProjectTexCoord.x = lightViewPosition.x / lightViewPosition.w * 0.5f + 0.5f;
    lightProjectTexCoord.y = -lightViewPosition.y / lightViewPosition.w * 0.5f + 0.5f;

    float depth;
    [branch]
    if ((saturate(lightProjectTexCoord.x) == lightProjectTexCoord.x) &&
        (saturate(lightProjectTexCoord.y) == lightProjectTexCoord.y))
    {
        depth = lightViewPosition.z / lightViewPosition.w;
    }
    else
    {
        depth = 2;
    }
    return float3(lightProjectTexCoord, depth);

}

float GetShadowOneSample(float3 ShadowCoord, Texture2D ShadowMap, SamplerComparisonState ShadowsSampler)
{
    return ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy, ShadowCoord.z);
}

float texelSize = 1.0f / 4096.0f;

float2 texOffset(int u, int v)
{
    return float2(u * texelSize, v * texelSize);
}

float GetShadow16X(float3 ShadowCoord, Texture2D ShadowMap, SamplerComparisonState ShadowsSampler)
{
    float sum = 0;
    float x, y;
    for (y = -1.5; y <= 1.5; y += 1.0)
    {
        for (x = -1.5; x <= 1.5; x += 1.0)
        {
            sum += ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + texOffset(x, y), ShadowCoord.z);
        }
    }

    return sum / 16.0;
}

float GetShadow8X(float3 ShadowCoord, Texture2D ShadowMap, SamplerComparisonState ShadowsSampler)
{
    return (
		2.0 * ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy, ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(-texelSize, 0), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(texelSize, 0), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(0, -texelSize), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(0, texelSize), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(-texelSize, -texelSize), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(texelSize, -texelSize), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(-texelSize, texelSize), ShadowCoord.z) +
		ShadowMap.SampleCmpLevelZero(ShadowsSampler, ShadowCoord.xy + float2(texelSize, texelSize), ShadowCoord.z)
	) / 10.0;
}
