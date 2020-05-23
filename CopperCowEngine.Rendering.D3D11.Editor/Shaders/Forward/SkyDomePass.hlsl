#include "../Include/Samplers.hlsl"
#include "../Include/Layouts.hlsl"
#include "../Include/OutputLayouts.hlsl"
#include "../Include/ColorSpace.hlsl"

TextureCube SkyMap : register(t6);

OutputStruct PSMain(COMMON_PS_IN Input)
{
    OutputStruct OUT = (OutputStruct) 0;
    float3 color = SkyMap.SampleLevel(TrilinearWrapSampler, -Input.normal.xyz, 0);
#if LDR
    #if SRGB
    color = pow(OUT.Target0.rgb, 1 / 2.2);
    #else
    color = OUT.Target0.rgb;
    #endif
#endif
    OUT.Target0 = float4(color, 1);
    OUT.Target1 = normalize(Input.normal.xyz) * 0.5 + 0.5;
    return OUT;
}
