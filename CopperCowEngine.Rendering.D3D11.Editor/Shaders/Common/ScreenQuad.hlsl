#include "../Include/ConstantBuffers.hlsl"
#include "../Include/Samplers.hlsl"
#include "../Include/OutputLayouts.hlsl"
#include "../Include/ToneMapping.hlsl"

struct SCREEN_QUAD_PS_IN
{
    float4 position : SV_Position;
    float4 posProj : TEXCOORD0;
    float2 texcoord : TEXCOORD1;
};

#if MSAA 
Texture2DMS<float4> hdrBuffer : register(t0);
Texture2DMS<float4> VelocityMap : register(t1);
Texture2DMS<float> DepthMap : register(t2);
#else
Texture2D<float4> hdrBuffer : register(t0);
Texture2D<float4> VelocityMap : register(t1);
Texture2D<float> DepthMap : register(t2);
#endif
StructuredBuffer<float> AvgLum : register(t3);
Texture2D<float4> BloomTexture : register(t4);
Texture2D<float4> DOFBlurMap : register(t5);

SCREEN_QUAD_PS_IN VSMain(uint VertexID : SV_VertexID)
{
    SCREEN_QUAD_PS_IN Out = (SCREEN_QUAD_PS_IN) 0;
    Out.texcoord = float2((VertexID << 1) & 2, VertexID & 2) * 0.5;
    Out.position = float4(Out.texcoord * float2(2, -2) + float2(-1, 1), 1, 1);
    Out.posProj = Out.position;
    return Out;
}

float ConvertZToLinearDepth(float depth)
{
    float linearDepth = cbPerFrame.PerspectiveValues.w / (depth + cbPerFrame.PerspectiveValues.z);
    return linearDepth;
}

float3 DistanceDOF(float3 colorFocus, float3 colorBlurred, float depth)
{ 
    // Find the depth based blur factor   
    float blurFactor = saturate((depth - cbPostProcess.DOFFarValues.x) * cbPostProcess.DOFFarValues.y);
    // Lerp with the blurred color based on the CoC factor   
    return lerp(colorFocus, colorBlurred, blurFactor); 
} 

#if MSAA
OutputStruct PSMain(SCREEN_QUAD_PS_IN input, uint coverage : SV_Coverage, uint sampleIndex : SV_SampleIndex)
#else
OutputStruct PSMain(SCREEN_QUAD_PS_IN input)
#endif
{
    OutputStruct OUT = (OutputStruct) 0;
    
#if !LDR
    
#if MSAA
    float4 hdr = hdrBuffer.Load(int2(input.position.xy), sampleIndex);
#else
    float4 hdr = hdrBuffer.Sample(PointClampSampler, input.texcoord);
#endif
    
#if !MSAA
    float depth = DepthMap.Sample(PointClampSampler, input.texcoord.xy);
    if (depth < 1.0)
    {
        // Convert the full resolution depth to linear depth      
        depth = ConvertZToLinearDepth(depth);
        // Get the blurred color from the down scaled HDR texture      
        float3 colorBlurred = DOFBlurMap.Sample(LinearSampler, input.texcoord.xy).xyz;
        // Compute the distance DOF color      
        hdr.rgb = DistanceDOF(hdr.rgb, colorBlurred, depth);
        
        //OUT.Target0 = depth;
        //return OUT;
    }
#endif
    
#if BLOOM
    hdr.rgb += cbPostProcess.BloomScale * BloomTexture.Sample(LinearSampler, input.texcoord).xyz;
#endif
    float3 sdr = ToneMapping(hdr.rgb, AvgLum[0]);
    OUT.Target0 = float4(saturate(sdr.rgb), hdr.a);
#else
    
#if MSAA
    float4 sdr = hdrBuffer.Load(int2(input.position.xy), sampleIndex);
#else
    float4 sdr = hdrBuffer.Sample(PointClampSampler, input.texcoord);
#endif
    OUT.Target0 = float4(sdr.rgb, sdr.a);
#endif
    return OUT;

}
