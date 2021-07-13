//Depth Parallex
//#pragma shader_feature_local _PARALLEX
half2 ParallexMapping(Texture2D depthTexture,SamplerState depthSampler, float2 uv,half3 viewDirTS,half offset,half scale,uint parallexCount,inout half depthOS)
{
    half3 viewDir=normalize(viewDirTS);
    viewDir.z+=offset;
    half2 uvOffset=viewDir.xy/viewDir.z*scale;
#ifndef _PARALLEX
    depthOS=SAMPLE_TEXTURE2D_LOD(depthTexture,depthSampler,uv,0).r;
    return uv-uvOffset*depthOS;
#else
    half marchDelta=saturate(dot(half3(0.h,0.h,1.h),viewDirTS));
    int marchCount=min(lerp(parallexCount/2u,parallexCount,marchDelta),128u);
    half deltaDepth=1.0h/marchCount;
    half2 deltaUV=uvOffset/marchCount;
    half depthLayer=0.h;
    half depthSample=1.h;
    half preDepthSample=0.h;
    while(depthSample>depthLayer)
    {
        preDepthSample=depthSample;
        depthSample=SAMPLE_TEXTURE2D_LOD(depthTexture,depthSampler,uv,0).r;
        depthLayer+=deltaDepth;
        uv-=deltaUV;
    }
    half d1=depthLayer-depthSample;
    half d2=(depthLayer-deltaDepth)-preDepthSample;
    half interpolate=d1*rcp(d1-d2);
    uv+=interpolate*deltaUV;
    depthOS=depthLayer-interpolate*deltaDepth;
    return uv;
#endif
}
