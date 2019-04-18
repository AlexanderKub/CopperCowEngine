#include "./Include/Layouts.hlsl"
#include "./Include/Structures.hlsl"

/////////////////////////////////////////////////////////////////////////////
// Vertex shader
/////////////////////////////////////////////////////////////////////////////
float4 LightVolumesVS() : SV_Position
{
    return float4(0.0, 0.0, 0.0, 1.0);
}

/////////////////////////////////////////////////////////////////////////////
// Hull shader
/////////////////////////////////////////////////////////////////////////////

HS_CONSTANT_DATA_OUTPUT LightVolumesConstantHS()
{
    HS_CONSTANT_DATA_OUTPUT Output;
	
    float tessFactor = 18.0;
    Output.Edges[0] = Output.Edges[1] = Output.Edges[2] = Output.Edges[3] = tessFactor;
    Output.Inside[0] = Output.Inside[1] = tessFactor;

    return Output;
}

static const float3 HemilDir[2] =
{
    float3(1.0, 1.0, 1.0),
	float3(-1.0, 1.0, -1.0)
};

[domain("quad")]
[partitioning("integer")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[patchconstantfunc("LightVolumesConstantHS")]
HS_OUTPUT LightVolumesHS(uint PatchID : SV_PrimitiveID)
{
    HS_OUTPUT Output;

    Output.HemiDir = HemilDir[PatchID];

    return Output;
}

/////////////////////////////////////////////////////////////////////////////
// Domain shader
/////////////////////////////////////////////////////////////////////////////

cbuffer cbPointLightDomain : register(b0)
{
    float4x4 LightProjection : packoffset(c0);
    #if SPOT_LIGHT
    float SinAngle : packoffset(c4);
    float CosAngle : packoffset(c4.y);
    #elif CAPSULE_LIGHT
    float HalfSegmentLen : packoffset(c4);
    float CapsuleRange : packoffset(c4.y);
    #endif
}

#if SPOT_LIGHT
#define CylinderPortion 0.2 
#define ExpendAmount (1.0 + CylinderPortion)
#elif CAPSULE_LIGHT
#define CylinderPortion 0.2
#define SpherePortion   (1.0 - CylinderPortion)
#define ExpendAmount    (1.0 + CylinderPortion)
#endif

[domain("quad")]
DS_OUTPUT LightVolumeDS(HS_CONSTANT_DATA_OUTPUT input, float2 UV : SV_DomainLocation, const OutputPatch<HS_OUTPUT, 4> quad)
{
    #if POINT_LIGHT

	// Transform the UV's into clip-space
    float2 posClipSpace = UV.xy * 2.0 - 1.0;

	// Find the absulate maximum distance from the center
    float2 posClipSpaceAbs = abs(posClipSpace.xy);
    float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);

	// Generate the final position in clip-space
    float3 normDir = normalize(float3(posClipSpace.xy, (maxLen - 1.0)) * quad[0].HemiDir);
    float4 posLS = float4(normDir.xyz, 1.0);
	
	// Transform all the way to projected space
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);

	// Store the position for per pixel calculate clip space position
    Output.PosProj = Output.Position;
    return Output;

    #elif SPOT_LIGHT

    // Transform the UV's into clip-space   
    float2 posClipSpace = UV.xy * float2(2.0, -2.0) + float2(-1.0, 1.0);
    // Find the vertex offsets based on the UV   
    float2 posClipSpaceAbs = abs(posClipSpace.xy);
    float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);
    // Force the cone vertices to the mesh edge   
    float2 posClipSpaceNoCylAbs = saturate(posClipSpaceAbs * ExpendAmount);
    float maxLenNoCapsule = max(posClipSpaceNoCylAbs.x, posClipSpaceNoCylAbs.y);
    float2 posClipSpaceNoCyl = sign(posClipSpace.xy) * posClipSpaceNoCylAbs;
    // Convert the positions to half sphere with the cone vertices on the edge   
    float3 halfSpherePos = normalize(float3(posClipSpaceNoCyl.xy, 1.0 - maxLenNoCapsule));
    // Scale the sphere to the size of the cones rounded base   
    halfSpherePos = normalize(float3(halfSpherePos.xy * SinAngle, CosAngle));
    // Find the offsets for the cone vertices (0 for cone base)
    float cylinderOffsetZ = saturate((maxLen * ExpendAmount - 1.0) / CylinderPortion);
    // Offset the cone vertices to their final position  
    float4 posLS = float4(halfSpherePos.xy * (1.0 - cylinderOffsetZ), halfSpherePos.z - cylinderOffsetZ * CosAngle, 1.0);
    // Transform all the way to projected space and generate the UV  coordinates   
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);
    Output.PosProj = Output.Position;
    return Output;

    #elif CAPSULE_LIGHT

    // Transform the UV's into clip-space
    float2 posClipSpace = UV.xy * 2.0 - 1.0;
    // Find the vertex offsets based on the UV   
    float2 posClipSpaceAbs = abs(posClipSpace.xy);
    float maxLen = max(posClipSpaceAbs.x, posClipSpaceAbs.y);
    float2 posClipSpaceNoCylAbs = saturate(posClipSpaceAbs * ExpendAmount);
    float2 posClipSpaceNoCyl = sign(posClipSpace.xy) * posClipSpaceNoCylAbs;
    float maxLenNoCapsule = max(posClipSpaceNoCylAbs.x, posClipSpaceNoCylAbs.y);
    // Generate the final position in clip-space   
    float3 normDir = normalize(float3(posClipSpaceNoCyl.xy, maxLenNoCapsule - 1.0)) * CapsuleRange;
    float cylinderOffsetZ = saturate(maxLen - min(maxLenNoCapsule, SpherePortion)) / CylinderPortion;
    float4 posLS = float4(normDir.xy, normDir.z + cylinderOffsetZ * HalfSegmentLen - HalfSegmentLen, 1.0);
    // Move the vertex to the selected capsule sid
    posLS *= float4(quad[0].HemiDir, 1);
    // Transform all the way to projected space and generate the UV     coordinates   
    DS_OUTPUT Output;
    Output.Position = mul(posLS, LightProjection);
    Output.PosProj = Output.Position;
    return Output;

    #endif
}
