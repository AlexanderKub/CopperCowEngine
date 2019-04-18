#include "../Include/Structures.hlsl"

float3 ToneMapReinhard(float3 color)
{
    return color / (1.0f + color);
}

float3 ToneMapACESFilmic(float3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
}

float4 MotionBlur(Texture2D hdrBuffer, Texture2D velocityMap, SamplerState Sampler, float2 uv, float intensity, int currentFps)
{
    float2 speed = velocityMap.Sample(Sampler, uv).rg;
    speed = speed.r == 1 && speed.g == 1 ? 0 : speed;
    //speed = float2(-1, -1) + 2.0f * speed;
    speed = intensity * speed * (currentFps / (uint) 60);

    float3 color = float3(0, 0, 0);
 
    int nSamples = 8;
    [loop]
    for (int i = 1; i < nSamples; ++i)
    {
        // get offset in range [-0.5, 0.5]:
        float2 offset = speed * (float(i) / float(nSamples - 1) - 0.5);
  
        // sample & add to result:
        color += hdrBuffer.Sample(Sampler, uv + offset).rgb;
    }
    color /= float(nSamples);

    return float4(color, 1.);
}

struct ParametersStruct
{
    uint currentFps;
    float3 filler;
};

struct VSQuadOut {
    float4 position : SV_Position;
    float4 posProj : TEXCOORD0;
    float2 texcoord : TEXCOORD1;
};

cbuffer cbPerFrameBuffer : register(b0)
{
    CBufferPerFrameStruct cbPerFrame;
}
#if MSAA 
Texture2DMS<float4> hdrBuffer : register(t0);
Texture2DMS<float4> VelocityMap : register(t1);
Texture2DMS<float> DepthMap : register(t2);
#else
Texture2D hdrBuffer : register(t0);
Texture2D VelocityMap : register(t1);
Texture2D DepthMap : register(t2);
StructuredBuffer<float> AvgLum : register(t3);
#endif

struct CBufferPostProcessStruct
{
    float MiddleGrey;
    float LumWhiteSqr;
};

cbuffer cbPostProcessBuffer : register(b4)
{
    CBufferPostProcessStruct cbPostProcess;
}
SamplerState Sampler : register(s0);

float2 VelocityCalculate(float4 posProj, float2 texcoord)
{
    // Constants
    float far = 0.001f;
    float near = 10000;
    float ProjectionA = far / (far - near);
    float ProjectionB = (-far * near) / (far - near);

    // Normalize the view ray
    float3 viewRay = mul(posProj, cbPerFrame.ProjectionInv).xyz;
    viewRay = normalize(float3(viewRay.xy / viewRay.z, 1.0f));

    // Sample the depth buffer and convert it to linear depth
    //float depth = DepthMap.Sample(Sampler, texcoord).r; //TODO: PointSampler
    float depth = 1;
    float linearDepth = ProjectionB / (depth - ProjectionA);
    //return linearDepth / 100;

    float4 positionVS = float4(viewRay * linearDepth, 1);
    float4 positionWS = mul(positionVS, cbPerFrame.ViewInv);

    float4 prevPositionVS = mul(positionWS, cbPerFrame.PrevView);

    return prevPositionVS.xy - positionVS.xy;
}

float3 LinearToSRGB(float3 color)
{
    return pow(abs(color), 1 / 2.2);
}

float3 SRGBToLinear(float3 srgb)
{
    return pow(abs(srgb), 2.2);
}

static const float3 LUM_FACTOR = float3(0.299, 0.587, 0.114);

float3 ToneMappingLum(float3 HDRColor)
{
	// Find the luminance scale for the current pixel
    float LScale = dot(HDRColor, LUM_FACTOR);
    LScale *= cbPostProcess.MiddleGrey / AvgLum[0];
    LScale = (LScale + LScale * LScale / cbPostProcess.LumWhiteSqr) / (1.0 + LScale);
     // Apply the luminance scale to the pixels color
    return HDRColor * LScale;
}

float3 ToneMapACESFilmicLum(float3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;

    float LScale = dot(x, LUM_FACTOR);
    LScale *= cbPostProcess.MiddleGrey / AvgLum[0];
    float3 dx = x * (LScale + LScale * LScale / cbPostProcess.LumWhiteSqr);
    return saturate((dx * (a * dx + b)) / (dx * (c * dx + d) + e));
}

#if MSAA
float4 PSMain(VSQuadOut input, uint coverage : SV_Coverage, uint sampleIndex : SV_SampleIndex) : SV_Target
#else
float4 PSMain(VSQuadOut input) : SV_Target
#endif
{
    //return float4(VelocityCalculate(input.posProj, input.texcoord), 0, 1);
   
    //float2 velocity = VelocityMap.Sample(Sampler, input.texcoord).rg * 2 - 1;
    //return float4(velocity, 0, 1);

    //float2 velocity = VelocityCalculate(input.posProj, input.texcoord);
    #if MSAA
    float4 hdr = hdrBuffer.Load(int2(input.position.xy), sampleIndex).rgba;
    #else
    float4 hdr = hdrBuffer.Sample(Sampler, input.texcoord).rgba;
    #endif

    /*int nSamples = 8;
    for (int i = 1; i < nSamples; ++i)
    {
        // get offset in range [-0.5, 0.5]:
        float2 offset = velocity * (float(i) / float(nSamples - 1) - 0.5);
  
        // sample & add to result:
        hdr += hdrBuffer.Sample(Sampler, input.texcoord + offset).rgba;
    }
    hdr /= float(nSamples);*/

   // hdr = MotionBlur(hdrBuffer, VelocityMap, Sampler, input.texcoord, 0.001, cbPerFrame.currentFPS);

    /*float LScale = dot(hdr.rgb, LUM_FACTOR);
    LScale = cbPostProcess.MiddleGrey / AvgLum[0];
    return LScale + LScale * LScale / cbPostProcess.LumWhiteSqr;*/

    //float3 sdr = ToneMapACESFilmic(hdr.rgb * LScale);
    //float3 sdr = ToneMapReinhard(hdr.rgb);

    //float3 sdr = ToneMapACESFilmicLum(hdr.rgb);
    float3 sdr = ToneMappingLum(hdr.rgb);
    sdr = LinearToSRGB(sdr.rgb);
    //sdr = SRGBToLinear(sdr);

    return float4(sdr.rgb, hdr.a);
}
