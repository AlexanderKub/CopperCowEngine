#ifndef __DEPENDENCY_HLSL_FORWARDPLUSCOMMON__
#define __DEPENDENCY_HLSL_FORWARDPLUSCOMMON__

#define TILE_RES 16
#define MAX_NUM_LIGHTS_PER_TILE 544

#define LIGHT_INDEX_BUFFER_SENTINEL 0x7fffffff

uint GetTileIndex(float2 ScreenPos, uint width)
{
    float fTileRes = (float) TILE_RES;
    uint nNumCellsX = (width + TILE_RES - 1) / TILE_RES;
    uint nTileIdx = floor(ScreenPos.x / fTileRes) + floor(ScreenPos.y / fTileRes) * nNumCellsX;
    return nTileIdx;
}
#endif