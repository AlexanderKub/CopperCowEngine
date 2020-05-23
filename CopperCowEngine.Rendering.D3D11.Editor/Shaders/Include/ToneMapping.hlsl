#include "../Include/Constants.hlsl"
#include "../Include/Math.hlsl"
#include "../Include/ConstantBuffers.hlsl"

#ifndef __DEPENDENCY_HLSL_TONE_MAPPING__
#define __DEPENDENCY_HLSL_TONE_MAPPING__

float computeEV100(float aperture, float shutterTime, float ISO)
{
    return log2(Pow2(aperture) / shutterTime * 100 / ISO);
}

float computeEV100FromAvgLuminance(float avgLuminance, float middleGray)
{
    return log2(avgLuminance * 100.0f / middleGray);//12.5f
}

float convertEV100ToExposure(float EV100)
{
    float maxLuminance = 1.2f * pow(2.0f, EV100);
    return 1.0f / maxLuminance;
}

static const bool useAutoExposure = false;

float GetExposure(float avgLuminance, float middleGray)
{
    float shutterTime = 1.0f / 80.0f;
    float aperture = 1.0 / 5.6;
    float ISO = 400;
    
    float EV100 = computeEV100(aperture, shutterTime, ISO);
    float AutoEV100 = computeEV100FromAvgLuminance(avgLuminance, middleGray);
    float currentEV = useAutoExposure ? AutoEV100 : EV100;
    return convertEV100ToExposure(currentEV);
}

float GetExposure(float avgLuminance, float minLuminance, float maxLuminance, float middleGray, float powParam)
{
    avgLuminance = clamp(avgLuminance, minLuminance, maxLuminance);
    avgLuminance = max(avgLuminance, 1e-4);
    
    float scaledWhitePoint = middleGray * 11.2;
    
    float luminance = pow(abs(avgLuminance / scaledWhitePoint), powParam) * scaledWhitePoint;
    
    return middleGray / luminance;
}

float3 U2Func(float A, float B, float C, float D, float E, float F, float3 x)
{
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 ToneMapU2Func(float A, float B, float C, float D, float E, float F, float3 color, float numMultiplier)
{
    float3 numerator = U2Func(A, B, C, D, E, F, color);
    numerator = max(numerator, 0);
    numerator.rgb *= numMultiplier;
   
    float3 denominator = U2Func(A, B, C, D, E, F, 11.2);
    denominator = max(denominator, 1e-4);
   
    return numerator / denominator;
}

float3 ToneMapping(float3 x, float avgLum)
{
    /*
    float exposure = GetExposure(avgLum, 
        cbPostProcess.MinLuminance, cbPostProcess.MaxLuminance, 
        cbPostProcess.MiddleGrey, cbPostProcess.ExposurePow);
    */
    float exposure = GetExposure(avgLum, 12.5f);
    
    return ToneMapU2Func(cbPostProcess.TonemapA, cbPostProcess.TonemapB, 
        cbPostProcess.TonemapC, cbPostProcess.TonemapD, 
        cbPostProcess.TonemapE, cbPostProcess.TonemapF, 
        x, cbPostProcess.NumeratorMultiplier);
}

#endif