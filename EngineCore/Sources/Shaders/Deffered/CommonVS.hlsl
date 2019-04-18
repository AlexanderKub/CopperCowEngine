#include "./Include/Layouts.hlsl"
#include "../Include/Layouts.hlsl"
#include "../CommonsInclude/StandardHeaders.hlsl"

COMMON_PS_IN VSMain(VertexShaderInput vertex)
{
    COMMON_PS_IN result = (COMMON_PS_IN) 0;
    result.pos = vertex.Position;
    result.pos.w = 1.0f;
    result.pos = mul(result.pos, PerObjectBuffer.World);
    result.posWS = result.pos;
    result.pos = mul(result.pos, PerFrameBuffer.View);
    result.pos = mul(result.pos, PerFrameBuffer.Projection);
    
    result.color = 1;
    
    result.uv0 = vertex.UV0;
    result.uv0 *= PerMaterialBuffer.textureTiling;
    result.uv0 += PerMaterialBuffer.textureShift;

    result.normal = float4(mul(vertex.Normal, PerObjectBuffer.World).xyz, 1);
    result.tangent = float4(mul(vertex.Tangent, (float3x3) PerObjectBuffer.World).xyz, 1);
 
    return result;
}