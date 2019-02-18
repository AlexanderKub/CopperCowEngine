struct ConstBuffer
{
    float4x4 viewProjMatrix;
    float4x4 modelMatrix;
    float4 lightPosition;
};

cbuffer constants : register(b0)
{
    ConstBuffer ConstantBuffer;
}

struct VS_IN
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct GSPS_IN
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

GSPS_IN VSMain(VS_IN input)
{
    GSPS_IN output = (GSPS_IN) 0;
    float4 worldPos = input.Position;
    output.Position = mul(worldPos, ConstantBuffer.modelMatrix);
   
    return output;
}

float4 PSMain(GSPS_IN input) : SV_Target
{
    return input.Color;
}

[maxvertexcount(29)]
void GSStream(inout TriangleStream<GSPS_IN> stream, triangleadj GSPS_IN input[6])
{

    uint i;
    float4 lightDir = float4(ConstantBuffer.lightPosition.xyz, 0);

    uint3 face[4];
    float3 faceNormal[4];
    bool backface[4];
    
    face[0] = uint3(0, 2, 4);
    face[1] = uint3(0, 1, 2);
    face[2] = uint3(2, 3, 4);
    face[3] = uint3(4, 5, 0);
    
    for (i = 0; i < 4; i++)
    {
        float3 U = input[face[i].y].Position.xyz - input[face[i].x].Position.xyz;
        float3 V = input[face[i].y].Position.xyz - input[face[i].z].Position.xyz;
        faceNormal[i] = cross(U, V);
        backface[i] = dot(faceNormal[i], lightDir.xyz) < 0;
    }

    GSPS_IN output = (GSPS_IN) 0;
    //Debug triangle
    output.Position = mul(input[0].Position + float4(faceNormal[0], 0), ConstantBuffer.viewProjMatrix);
    //output.Position = input[0].Position + float4(faceNormal[0], 0);
    output.Color = backface[0] ? float4(0, 0, 1, 1) : float4(1, 1, 1, 1);
    stream.Append(output);

    output.Position = mul(input[2].Position + float4(faceNormal[0], 0), ConstantBuffer.viewProjMatrix);
    //output.Position = input[2].Position + float4(faceNormal[0], 0);
    output.Color = backface[0] ? float4(0, 0, 1, 1) : float4(1, 1, 1, 1);
    stream.Append(output);

    output.Position = mul(input[4].Position + float4(faceNormal[0], 0), ConstantBuffer.viewProjMatrix);
    //output.Position = input[4].Position + float4(faceNormal[0], 0);
    output.Color = backface[0] ? float4(0, 0, 1, 1) : float4(1, 1, 1, 1);
    stream.Append(output);

    float4 debugColors[4];
    debugColors[0] = float4(1, 0, 0, 1);
    debugColors[1] = float4(0, 1, 0, 1);
    debugColors[2] = float4(0, 0, 1, 1);
    debugColors[3] = float4(1, 1, 1, 1);
    
    for (i = 1; i < 4; i++)
    {
        if (backface[0] && !backface[i])
        {
            stream.RestartStrip();

            output.Position = mul(input[face[i].x].Position, ConstantBuffer.viewProjMatrix);
            //output.Position = input[face[i].x].Position;
            output.Color = debugColors[0];
            stream.Append(output);
            
            output.Position = lightDir;
            //output.Position = input[face[i].x].Position;
            output.Position.w = 0;
            output.Position = mul(output.Position, ConstantBuffer.viewProjMatrix);
            //output.Position = output.Position;
            output.Color = debugColors[i];
            stream.Append(output);

            output.Position = mul(input[face[i].z].Position, ConstantBuffer.viewProjMatrix);
            //output.Position = input[face[i].z].Position;
            output.Color = debugColors[0];
            stream.Append(output);
            
            output.Position = lightDir;
            output.Position.w = 0;
            output.Position = mul(output.Position, ConstantBuffer.viewProjMatrix);
            //output.Position = output.Position;
            output.Color = debugColors[i];
            stream.Append(output);
        }
    }
}