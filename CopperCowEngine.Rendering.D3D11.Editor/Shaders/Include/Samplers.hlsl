#ifndef __DEPENDENCY_HLSL_SAMPLERS__
#define __DEPENDENCY_HLSL_SAMPLERS__

SamplerState LinearSampler : register(s2);
SamplerState TrilinearWrapSampler : register(s3);
SamplerState PointClampSampler : register(s4);
SamplerState BilinearWrapSampler : register(s5);
SamplerState IBLSampler : register(s6);
SamplerState PreIntegratedSampler : register(s7);
#endif