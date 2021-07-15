//Depth Parallax
// [Header(Depth)]
// [ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
// [Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
// [Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(0,1))=.42
// [Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=1
// [Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=1
// [Toggle(_PARALLAX)]_Parallax("Parallax",float)=0
// [Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallax Count",int)=16

//#pragma shader_feature_local _DEPTHMAP
//#pragma shader_feature_local _PARALLAX
//#pragma shader_feature_local _DEPTHBUFFER

TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial_Parallax)
        INSTANCING_PROP(float,_DepthScale)
        INSTANCING_PROP(float,_DepthOffset)
        INSTANCING_PROP(float,_DepthBufferScale)
        INSTANCING_PROP(int ,_ParallaxCount)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial_Parallax)

half ParallaxMapping(float2 uv,half2 uvOffset,uint marchCount)
{
    half deltaParallax=1.0h/marchCount;
    half2 deltaUV=uvOffset/marchCount;
    half layer=0.h;
    half parallaxSample=SAMPLE_TEXTURE2D_LOD(_DepthTex,sampler_DepthTex,uv,0).r;
    half preParallaxSample=0.h;
    while(parallaxSample>layer)
    {
        preParallaxSample=parallaxSample;
        parallaxSample=SAMPLE_TEXTURE2D_LOD(_DepthTex,sampler_DepthTex,uv,0).r;
        layer+=deltaParallax;
        uv-=deltaUV;
    }
    half d1=layer-parallaxSample;
    half d2=(layer-deltaParallax)-preParallaxSample;
    half interpolate=d1*rcp(d1-d2);
    return layer-interpolate*deltaParallax;
}

void ParallaxUVMapping(inout half2 uv,inout float depth,inout float3 positionWS,float3x3 TBNWS,float3 viewDirWS)
{
    #ifndef _DEPTHMAP
        return;
    #endif

    half3 viewDirTS=mul(TBNWS, viewDirWS);
    half3 viewDir=normalize(viewDirTS);
    viewDir.z+=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_Parallax,_DepthOffset);
    half2 uvOffset=viewDir.xy/viewDir.z*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_Parallax,_DepthScale);
    half marchDelta=saturate(dot(half3(0.h,0.h,1.h),viewDirTS));
    half parallax=0.h;
    
    #if _PARALLAX
    uint parallaxCount=INSTANCE(_ParallaxCount);
    parallaxCount=min(lerp(parallaxCount/2u,parallaxCount,marchDelta),128u);
    parallax=ParallaxMapping( uv,uvOffset,parallaxCount);
    #else
    parallax= SAMPLE_TEXTURE2D_LOD(_DepthTex,sampler_DepthTex,uv,0).r;
    #endif
    
    #if _DEPTHBUFFER
    half projectionDistance=parallax*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_Parallax,_DepthScale)*step(0.01h,marchDelta)* rcp(marchDelta);
    positionWS = positionWS- viewDirWS*projectionDistance*UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_Parallax,_DepthBufferScale);
    depth=LinearEyeDepthToOutDepth(TransformWorldToLinearEyeDepth(positionWS,UNITY_MATRIX_V));
    #endif
    
    uv-=uvOffset*parallax;
}

void ParallaxPositionMapping(inout float3 positionWS,inout float depth,float3 viewDirWS)
{
     #if _DEPTHMAP
    half depthOffsetOS=0.h;
    
    
    #endif
}