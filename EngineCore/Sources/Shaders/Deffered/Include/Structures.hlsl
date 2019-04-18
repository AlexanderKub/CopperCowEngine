#ifndef __DEPENDENCY_HLSL_DEFFEREDSTRUCTS__
#define __DEPENDENCY_HLSL_DEFFEREDSTRUCTS__

struct PerFrameBufferStruct
{
    // The view matrix
    float4x4 View;
    // The inverse view matrix
    float4x4 InverseView;
    // The projection matrix
    float4x4 Projection;
    // The inverse of the projection matrix
    float4x4 InverseProjection;
    // Camera position in world space
    float3 CameraPosition;
    float FPS;
    float4 PerspectiveValues;
};

struct PerObjectBufferStruct
{
    // The world matrix
    float4x4 World;
    // The inverse world matrix
    float4x4 WorldInverse;
};

struct PerMaterialBufferStruct
{
    float4 AlbedoColor;
    float4 EmissiveColor;

    float RoughnessValue;
    float MetallicValue;
    float SpecularValue;
    float MaterialID;

    float2 textureTiling;
    float2 textureShift;

    //r hasAlbedoMap
    //g hasMetallicMap
    //b hasEmissiveMap
    //a hasRoughnessMap
    float4 optionsMask0;
    
    //r hasNormalMap
    //g hasSpecularMap
    //b hasOcclusionMap
    //a nonRecieveShadows
    float4 optionsMask1;

    float AlphaClip;
};

struct PerLightBufferStruct
{
    float3 Direction;
    uint Type; // 0=Direction, 1=Point, 2=spot
    float3 Position;
    float Radius;
    float3 Color;
    float Intensity;
};

struct GBufferAttributes
{
    float3 AlbedoColor;
    float MetallicValue;

    float3 EmmisiveColor;
    float RoughnessValue;

    float SpecularValue;
    float AOValue;
    bool Unlit;
    bool NonShadow;

    float3 Normal;
    float3 Position;
};

#endif