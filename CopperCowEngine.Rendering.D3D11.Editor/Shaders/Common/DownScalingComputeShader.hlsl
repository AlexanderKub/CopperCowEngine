#include "../Include/Constants.hlsl"
#include "../Include/Structures.hlsl"

cbuffer cbDownScaleBuffer : register(b0)
{
    DownScaleConstants cbDownScale;
}

#if MSAA
Texture2DMS<float4> HDRTex : register(t0);
#else
Texture2D HDRTex : register(t0);
#endif
StructuredBuffer<float> PrevAvgLum : register(t1);
StructuredBuffer<float> AverageValues1D : register(t2);

RWStructuredBuffer<float> AverageLum : register(u0);
RWTexture2D<float4> HDRDownScale : register(u1);

// Group shared memory to store the intermidiate results
groupshared float SharedPositions[1024];

[numthreads(1024, 1, 1)]
void DownScaleFirstPass(uint3 groupId : SV_GroupID, uint3 groupThreadId : SV_GroupThreadID,
    uint3 dispatchThreadId : SV_DispatchThreadID)
{
    uint2 CurPixel = uint2(dispatchThreadId.x % cbDownScale.Res.x, dispatchThreadId.x / cbDownScale.Res.x);

	// Skip out of bound pixels
    float avgLum = 0.0;
    if (CurPixel.y < cbDownScale.Res.y)
    {
        int3 nFullResPos = int3(CurPixel * 4, 0);
        float3 downScaled = float3(0.0, 0.0, 0.0);
		[unroll]
        for (int i = 0; i < 4; i++)
        {
			[unroll]
            for (int j = 0; j < 4; j++)
            {
#if MSAA
                downScaled += HDRTex.Load(nFullResPos.xy, nFullResPos.z, int2(j, i)).rgb;
#else
                downScaled += HDRTex.Load(nFullResPos, int2(j, i)).rgb;
#endif
            }
        }
        downScaled /= 16.0; // Average
#if BLUR
        HDRDownScale[CurPixel.xy] = float4(downScaled, 1.0);
#endif
        avgLum = dot(downScaled.rgb, LUM_FACTOR); // Calculate the lumenace value for this pixel
    }
    SharedPositions[groupThreadId.x] = avgLum; // Store in the group memory for further reduction

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Down scale from 1024 to 256
    if (groupThreadId.x % 4 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 1 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 1] : avgLum;
        stepAvgLum += dispatchThreadId.x + 2 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 2] : avgLum;
        stepAvgLum += dispatchThreadId.x + 3 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 3] : avgLum;
		
		// Store the results
        avgLum = stepAvgLum;
        SharedPositions[groupThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 256 to 64
    if (groupThreadId.x % 16 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 4 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 4] : avgLum;
        stepAvgLum += dispatchThreadId.x + 8 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 8] : avgLum;
        stepAvgLum += dispatchThreadId.x + 12 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 12] : avgLum;

		// Store the results
        avgLum = stepAvgLum;
        SharedPositions[groupThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 64 to 16
    if (groupThreadId.x % 64 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 16 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 16] : avgLum;
        stepAvgLum += dispatchThreadId.x + 32 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 32] : avgLum;
        stepAvgLum += dispatchThreadId.x + 48 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 48] : avgLum;

		// Store the results
        avgLum = stepAvgLum;
        SharedPositions[groupThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 16 to 4
    if (groupThreadId.x % 256 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 64 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 64] : avgLum;
        stepAvgLum += dispatchThreadId.x + 128 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 128] : avgLum;
        stepAvgLum += dispatchThreadId.x + 192 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 192] : avgLum;

		// Store the results
        avgLum = stepAvgLum;
        SharedPositions[groupThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 4 to 1
    if (groupThreadId.x == 0)
    {
		// Calculate the average lumenance for this thread group
        float fFinalAvgLum = avgLum;
        fFinalAvgLum += dispatchThreadId.x + 256 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 256] : avgLum;
        fFinalAvgLum += dispatchThreadId.x + 512 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 512] : avgLum;
        fFinalAvgLum += dispatchThreadId.x + 768 < cbDownScale.Domain ? SharedPositions[groupThreadId.x + 768] : avgLum;
        fFinalAvgLum /= 1024.0;
        
         // Write the final value into the 1D UAV which will be used on the next step
        AverageLum[groupId.x] = max(fFinalAvgLum, 0.0001);
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Second pass - convert the 1D average values into a single value
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#define MAX_GROUPS 64

// Group shared memory to store the intermidiate results
groupshared float SharedAvgFinal[MAX_GROUPS];

[numthreads(MAX_GROUPS, 1, 1)]
void DownScaleSecondPass(uint3 groupId : SV_GroupID, uint3 groupThreadId : SV_GroupThreadID,
    uint3 dispatchThreadId : SV_DispatchThreadID)
{
	// Fill the shared memory with the 1D values
    float avgLum = 0.0;
    if (dispatchThreadId.x < cbDownScale.GroupSize)
    {
        avgLum = AverageValues1D[dispatchThreadId.x];
    }
    SharedAvgFinal[dispatchThreadId.x] = avgLum;

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 64 to 16
    if (dispatchThreadId.x % 4 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 1 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 1] : avgLum;
        stepAvgLum += dispatchThreadId.x + 2 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 2] : avgLum;
        stepAvgLum += dispatchThreadId.x + 3 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 3] : avgLum;
		
		// Store the results
        avgLum = stepAvgLum;
        SharedAvgFinal[dispatchThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 16 to 4
    if (dispatchThreadId.x % 16 == 0)
    {
		// Calculate the luminance sum for this step
        float stepAvgLum = avgLum;
        stepAvgLum += dispatchThreadId.x + 4 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 4] : avgLum;
        stepAvgLum += dispatchThreadId.x + 8 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 8] : avgLum;
        stepAvgLum += dispatchThreadId.x + 12 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 12] : avgLum;

		// Store the results
        avgLum = stepAvgLum;
        SharedAvgFinal[dispatchThreadId.x] = stepAvgLum;
    }

    GroupMemoryBarrierWithGroupSync(); // Sync before next step

	// Downscale from 4 to 1
    if (dispatchThreadId.x == 0)
    {
		// Calculate the average luminace
        float fFinalLumValue = avgLum;
        fFinalLumValue += dispatchThreadId.x + 16 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 16] : avgLum;
        fFinalLumValue += dispatchThreadId.x + 32 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 32] : avgLum;
        fFinalLumValue += dispatchThreadId.x + 48 < cbDownScale.GroupSize ? SharedAvgFinal[dispatchThreadId.x + 48] : avgLum;
        fFinalLumValue /= 64.0;
        
        float previosLumValue = PrevAvgLum[0];

		// Calculate the adaptive luminance
        float f = fFinalLumValue <= previosLumValue ? cbDownScale.AdaptationLower : cbDownScale.AdaptationGreater;
        float fAdaptedAverageLum = lerp(previosLumValue, fFinalLumValue, f);

		// Store the final value
        AverageLum[0] = max(fAdaptedAverageLum, 0.0001);
    }
}
