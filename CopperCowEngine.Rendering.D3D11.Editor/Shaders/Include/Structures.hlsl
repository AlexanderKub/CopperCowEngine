#ifndef __DEPENDENCY_HLSL_STRUCTURES__
#define __DEPENDENCY_HLSL_STRUCTURES__

struct ConstBufferPerFrameStruct
{
	float4x4 View;
	float4x4 Projection;
	float3 CameraPosition;
    float FrameTime;
	float4 PerspectiveValues;
};

struct ConstBufferPerFramePreviousStruct
{
	float4x4 PreviousViewProjection;
};

struct ConstBufferPerFrameInverseStruct
{
    float4x4 InverseViewProjection;
};
    
struct ConstBufferPerMaterialStruct
{
	float4 AlbedoColor;
	float4 EmissiveColor;

	float RoughnessValue;
	float MetallicValue;
	float Reflectance;
	float Unlit;

	float2 TextureTiling;
	float2 TextureShift;
	
    //r hasAlbedoMap;
    //g hasNormalMap;
    //b hasMetallicMap;
    //a hasRoughnessMap;
	float4 OptionsMask0;
	
    //r hasOcclusionMap
    //g hasEmissiveMap
    //b unlit
    //a nonReceiveShadows
	float4 OptionsMask1;

    float AlphaClip;
	float3 _filler;
};
    
struct ConstBufferPerObjectStruct
{
    float4x4 World;
    float4x4 WorldViewProjection;
};

struct ConstBufferPerObjectPreviousStruct
{
    float4x4 PreviousWorldViewProjection;
};

struct ConstBufferPerObjecInversetStruct
{
    float4x4 InverseWorldViewProjection;
};

struct ConstBufferPerLightBatchStruct
{
    uint4 LightsIndices;
};

struct ConstBufferPostProcessStruct
{
    float MiddleGrey;
    float MinLuminance;
    float MaxLuminance;
    float ExposurePow;
    
    float NumeratorMultiplier;
    float TonemapA;
    float TonemapB;
    float TonemapC;
    
    float TonemapD;
    float TonemapE;
    float TonemapF;
    float BloomScale;
    
    float4 DOFFarValues;
};

struct DownScaleConstants
{
    // Resolution of the down scaled target: x - width, y - height
    uint2 Res;
    // Total pixel in the downscaled image
    uint Domain;
    // Number of groups dispached on the first pass
    uint GroupSize;
    float AdaptationGreater;
    float AdaptationLower;
    float fBloomThreshold;
    float filler;
};

#endif