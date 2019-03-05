#include "../Include/ForwardPlusCommon.hlsl"
#include "../Include/Structures.hlsl"

#define FLT_MAX 3.402823466e+38F

// Textures and Buffers
cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

Buffer<float4> g_NonDirLightBufferCenterAndRadius : register(t0);
Texture2D<float> g_DepthTexture : register(t1);

RWBuffer<uint> g_PerTileLightIndexBufferOut : register(u0);

// Group Shared Memory (aka local data share, or LDS)
groupshared uint ldsZMax;
groupshared uint ldsZMin;

groupshared uint ldsLightIdxCounter;
groupshared uint ldsLightIdx[MAX_NUM_LIGHTS_PER_TILE];

// Helper functions

// this creates the standard Hessian-normal-form plane equation from three points, 
// except it is simplified for the case where the first point is the origin
float3 CreatePlaneEquation(float3 b, float3 c)
{
    // normalize(cross( b-a, c-a )), except we know "a" is the origin
    // also, typically there would be a fourth term of the plane equation, 
    // -(n dot a), except we know "a" is the origin
    return normalize(cross(b, c));
}

// point-plane distance, simplified for the case where 
// the plane passes through the origin
float GetSignedDistanceFromPlane(float3 p, float3 eqn)
{
    // dot(eqn.xyz,p) + eqn.w, , except we know eqn.w is zero 
    // (see CreatePlaneEquation above)
    return dot(eqn, p);
}

bool TestFrustumSides(float3 c, float r, float3 plane0, float3 plane1, float3 plane2, float3 plane3)
{
    bool intersectingOrInside0 = GetSignedDistanceFromPlane(c, plane0) < r;
    bool intersectingOrInside1 = GetSignedDistanceFromPlane(c, plane1) < r;
    bool intersectingOrInside2 = GetSignedDistanceFromPlane(c, plane2) < r;
    bool intersectingOrInside3 = GetSignedDistanceFromPlane(c, plane3) < r;

    return (intersectingOrInside0 && intersectingOrInside1 &&
            intersectingOrInside2 && intersectingOrInside3);
}

// calculate the number of tiles in the horizontal direction
uint GetNumTilesX()
{
    return (uint) ((cbPerFrame.WindowWidth + TILE_RES - 1) / (float) TILE_RES);
}

// calculate the number of tiles in the vertical direction
uint GetNumTilesY()
{
    return (uint) ((cbPerFrame.WindowHeight + TILE_RES - 1) / (float) TILE_RES);
}

// convert a point from post-projection space into view space
float4 ConvertProjToView(float4 p)
{
    p = mul(p, cbPerFrame.ProjectionInv);
    p /= p.w;
    return p;
}

// convert a depth value from post-projection space into view space
float ConvertProjDepthToView(float z)
{
    z = 1.f / (z * cbPerFrame.ProjectionInv._34 + cbPerFrame.ProjectionInv._44);
    return z;
}

void CalculateMinMaxDepthInLds(uint3 globalThreadIdx)
{
    float depth = g_DepthTexture.Load(uint3(globalThreadIdx.x, globalThreadIdx.y, 0)).x;
    float viewPosZ = ConvertProjDepthToView(depth);
    uint z = asuint(viewPosZ);
    if (depth != 0.f)
    {
        InterlockedMax(ldsZMax, z);
        InterlockedMin(ldsZMin, z);
    }
}

//Light Culling
#define NUM_THREADS_X TILE_RES
#define NUM_THREADS_Y TILE_RES
#define NUM_THREADS_PER_TILE (NUM_THREADS_X * NUM_THREADS_Y)

[numthreads(NUM_THREADS_X, NUM_THREADS_Y, 1)]
void CSMain(uint3 globalIdx : SV_DispatchThreadID, uint3 localIdx : SV_GroupThreadID, uint3 groupIdx : SV_GroupID)
{
    uint localIdxFlattened = localIdx.x + localIdx.y * NUM_THREADS_X;
    if (localIdxFlattened == 0)
    {
        ldsZMin = 0x7f7fffff;  // FLT_MAX as a uint
        ldsZMax = 0;
        ldsLightIdxCounter = 0;
    }

    float3 frustumEqn0, frustumEqn1, frustumEqn2, frustumEqn3;
    // construct frustum for this tile
    {
        uint pxm = TILE_RES * groupIdx.x;
        uint pym = TILE_RES * groupIdx.y;
        uint pxp = TILE_RES * (groupIdx.x + 1);
        uint pyp = TILE_RES * (groupIdx.y + 1);

        uint uWindowWidthEvenlyDivisibleByTileRes = TILE_RES * GetNumTilesX();
        uint uWindowHeightEvenlyDivisibleByTileRes = TILE_RES * GetNumTilesY();

        // four corners of the tile, clockwise from top-left
        float3 frustum0 = ConvertProjToView(float4(pxm / (float) uWindowWidthEvenlyDivisibleByTileRes * 2.f - 1.f, 
            (uWindowHeightEvenlyDivisibleByTileRes - pym) / (float) uWindowHeightEvenlyDivisibleByTileRes * 2.f - 1.f, 1.f, 1.f)).xyz;
        float3 frustum1 = ConvertProjToView(float4(pxp / (float) uWindowWidthEvenlyDivisibleByTileRes * 2.f - 1.f, 
            (uWindowHeightEvenlyDivisibleByTileRes - pym) / (float) uWindowHeightEvenlyDivisibleByTileRes * 2.f - 1.f, 1.f, 1.f)).xyz;
        float3 frustum2 = ConvertProjToView(float4(pxp / (float) uWindowWidthEvenlyDivisibleByTileRes * 2.f - 1.f, 
            (uWindowHeightEvenlyDivisibleByTileRes - pyp) / (float) uWindowHeightEvenlyDivisibleByTileRes * 2.f - 1.f, 1.f, 1.f)).xyz;
        float3 frustum3 = ConvertProjToView(float4(pxm / (float) uWindowWidthEvenlyDivisibleByTileRes * 2.f - 1.f, 
            (uWindowHeightEvenlyDivisibleByTileRes - pyp) / (float) uWindowHeightEvenlyDivisibleByTileRes * 2.f - 1.f, 1.f, 1.f)).xyz;

        // create plane equations for the four sides of the frustum, 
        // with the positive half-space outside the frustum (and remember, 
        // view space is left handed, so use the left-hand rule to determine 
        // cross product direction)
        frustumEqn0 = CreatePlaneEquation(frustum0, frustum1);
        frustumEqn1 = CreatePlaneEquation(frustum1, frustum2);
        frustumEqn2 = CreatePlaneEquation(frustum2, frustum3);
        frustumEqn3 = CreatePlaneEquation(frustum3, frustum0);
    }

    GroupMemoryBarrierWithGroupSync();

    // calculate the min and max depth for this tile, 
    // to form the front and back of the frustum
    float minZ = FLT_MAX;
    float maxZ = 0.f;
    CalculateMinMaxDepthInLds( globalIdx );

    GroupMemoryBarrierWithGroupSync();
    maxZ = asfloat( ldsZMax );
    minZ = asfloat( ldsZMin );

    // loop over the lights and do a sphere vs. frustum intersection test
    uint uNumLights = cbPerFrame.NumLights & 0xFFFFu;
    for (uint i = localIdxFlattened; i < uNumLights; i += NUM_THREADS_PER_TILE)
    {
        float4 center = g_NonDirLightBufferCenterAndRadius[i];
        float r = center.w;
        center.xyz = mul(float4(center.xyz, 1), cbPerObject.WorldViewMatrix).xyz;

        // test if sphere is intersecting or inside frustum
        if (TestFrustumSides(center.xyz, r, frustumEqn0, frustumEqn1, frustumEqn2, frustumEqn3))
        {
            if (-center.z + minZ < r && center.z - maxZ < r)
            {
                // do a thread-safe increment of the list counter 
                // and put the index of this light into the list
                uint dstIdx = 0;
                InterlockedAdd(ldsLightIdxCounter, 1, dstIdx);
                ldsLightIdx[dstIdx] = i;
            }
        }
    }    
    GroupMemoryBarrierWithGroupSync();

    uint uNumLightsInThisTile = ldsLightIdxCounter;
    {   // write back
        uint tileIdxFlattened = groupIdx.x + groupIdx.y * GetNumTilesX();
        uint startOffset = cbPerFrame.MaxNumLightsPerTile * tileIdxFlattened;

        for (uint i = localIdxFlattened; i < uNumLightsInThisTile; i += NUM_THREADS_PER_TILE)
        {
            // per-tile list of light indices
            g_PerTileLightIndexBufferOut[startOffset + i] = ldsLightIdx[i];
        }

        for (uint j = (localIdxFlattened + uNumLightsInThisTile); j < ldsLightIdxCounter; j += NUM_THREADS_PER_TILE)
        {
            // per-tile list of light indices
            g_PerTileLightIndexBufferOut[startOffset + j + 1] = ldsLightIdx[j];
        }

        if (localIdxFlattened == 0)
        {
            // mark the end of each per-tile list with a sentinel
            g_PerTileLightIndexBufferOut[startOffset + uNumLightsInThisTile] = LIGHT_INDEX_BUFFER_SENTINEL;
        }
    }
}