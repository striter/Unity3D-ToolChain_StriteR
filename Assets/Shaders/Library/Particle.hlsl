#include "Assets/Shaders/Library/Common.hlsl"
#include "PostProcess/Depth.hlsl"

float _SmoothParticleDistance;

float SmoothParticleLinear(float4 positionHCS,float3 positionWS)
{
    #ifndef _SMOOTHPARTICLE
        return 1;
    #endif
    
    float2 ndc = TransformHClipToNDC(positionHCS);
    return saturate(length(TransformNDCToWorld(ndc) - positionWS)/_SmoothParticleDistance);
}