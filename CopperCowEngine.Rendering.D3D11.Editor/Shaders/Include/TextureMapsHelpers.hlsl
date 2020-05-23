#ifndef __DEPENDENCY_HLSL_TEXTURE_MAPS_HELPERS__
#define __DEPENDENCY_HLSL_TEXTURE_MAPS_HELPERS__
float3 ApplyNormalMap(float3 normal, float3 tangent, float3 normalSample)
{
    // Remap normalSample to the range -1,1
    normalSample = (2.0 * normalSample) - 1.0;
    //normalSample *= 0.1;
    // Ensure tangent is orthogonal to normal vector - Gram-Schmidt orthogonalize
    float3 T = normalize(tangent.xyz - normal * dot(tangent.xyz, normal));
    // Create the Bitangent (tangent.w contains handedness)
    float3 bitangent = cross(normal, T);
    // Create the TBN matrix to transform from tangent space
    float3x3 TBN = float3x3(T, bitangent, normal);
    return normalize(mul(normalSample, TBN));
}
#endif