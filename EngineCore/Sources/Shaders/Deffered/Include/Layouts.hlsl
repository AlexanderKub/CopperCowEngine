#ifndef __DEPENDENCY_HLSL_DEFFEREDLAYOUTS__
#define __DEPENDENCY_HLSL_DEFFEREDLAYOUTS__

// From Vertex shader to PSFillGBuffer
struct GBufferPixelIn
{
    float4 Position : SV_POSITION;
    float2 TextureUV : TEXCOORD0;
    float3 WorldTangent : TANGENT;
    float3 WorldNormal : NORMAL;
};

// Pixel Shader output structure
struct GBufferOutput
{
    float4 AlbedoAndMetallic : SV_Target0;
    float4 EmissiveAndRoughness : SV_Target1;
   // uint PackedNormals : SV_Target2;
    float4 PackedNormals : SV_Target2;
    float4 SpecularAOUnlitNonShadows : SV_Target3;
 // | -------------------32 bits-------------------|
 // | Albedo (RGB) --------------->| Metallic (A) -| RT0
 // | Emissive (RGB) ------------->| Roughness (A) | RT1
 // | Normal(RGB) -------------------------------->| RT2
 // | Spec (R) | AO(G) | Unlit (B) | NonShadow (A) | RT3
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
    float2 UV0 : TEXCOORD0;
    float4 UV1 : TEXCOORD1;
    float4 Normal : NORMAL;
    float3 Tangent : TANGENT;
};

struct VSQuadOut
{
    float4 Position : SV_Position;
    float4 PosProj : TEXCOORD0;
    float2 Texcoord : TEXCOORD1;
};

/////////////////////////////////////////////////////////////////////////////
// Tesselation
/////////////////////////////////////////////////////////////////////////////
struct HS_CONSTANT_DATA_OUTPUT
{
    float Edges[4] : SV_TessFactor;
    float Inside[2] : SV_InsideTessFactor;
};

struct HS_OUTPUT
{
    float3 HemiDir : POSITION;
};

struct DS_OUTPUT
{
    float4 Position : SV_POSITION;
    // Interpolate per pixel for correct coords
    float4 PosProj : TEXCOORD0;
};

#endif