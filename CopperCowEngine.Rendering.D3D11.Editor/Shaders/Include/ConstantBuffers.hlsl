#include "../Include/Structures.hlsl"

#ifndef __DEPENDENCY_HLSL_CONSTANT_BUFFERS__
#define __DEPENDENCY_HLSL_CONSTANT_BUFFERS__

#define PerFrameBufferSlot b1
#define PerObjectBufferSlot b2
#define PerMaterialBufferSlot b3
#define PerFramePreviousBufferSlot b4
#define PerObjectPreviousBufferSlot b5
#define PostProcessBufferSlot b6

cbuffer cbPerFrameBuffer : register(PerFrameBufferSlot)
{
    ConstBufferPerFrameStruct cbPerFrame;
}

cbuffer cbPerObjectBuffer : register(PerObjectBufferSlot)
{
    ConstBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerMaterialBuffer : register(PerMaterialBufferSlot)
{
    ConstBufferPerMaterialStruct cbPerMaterial;
}

cbuffer cbPerFramePreviousBuffer : register(PerFramePreviousBufferSlot)
{
    ConstBufferPerFramePreviousStruct cbPerFramePrevious;
}

cbuffer cbPerObjectPreviousBuffer : register(PerObjectPreviousBufferSlot)
{
    ConstBufferPerObjectPreviousStruct cbPerObjectPrevious;
}

cbuffer cbPostProcessBuffer : register(PostProcessBufferSlot)
{
    ConstBufferPostProcessStruct cbPostProcess;
}

#endif