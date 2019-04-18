#ifndef __DEPENDENCY_HLSL_STANDARDHEADERS__
#define __DEPENDENCY_HLSL_STANDARDHEADERS__

#ifndef FORWARD
#define FORWARD 0
#endif

#if FORWARD

#include "../Include/Structures.hlsl"

#else

#include "../Deffered/Include/Structures.hlsl"
cbuffer PerFrame : register(b0)
{
    PerFrameBufferStruct PerFrameBuffer;
};

cbuffer PerObject : register(b1)
{
    PerObjectBufferStruct PerObjectBuffer;
};

cbuffer PerMaterial : register(b2)
{
    PerMaterialBufferStruct PerMaterialBuffer;
};

cbuffer PerLight : register(b3)
{
    PerLightBufferStruct PerLightBuffer;
};

#endif

#endif // __DEPENDENCY_HLSL_STANDARDHEADERS__