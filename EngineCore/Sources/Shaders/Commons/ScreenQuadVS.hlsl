struct VSQuadOut {
    float4 position : SV_Position;
    float4 posProj : TEXCOORD0;
    float2 texcoord : TEXCOORD1;
};

VSQuadOut VSMain(uint VertexID : SV_VertexID) {
    VSQuadOut Out;
    Out.texcoord = float2((VertexID << 1) & 2, VertexID & 2) * 0.5;
    Out.position = float4(Out.texcoord * float2(2, -2) + float2(-1, 1), 1, 1);
    Out.posProj = Out.position;
    return Out;
}