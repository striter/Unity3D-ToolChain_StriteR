//Depth Parallax
half ParallaxMappingPOM(TEXTURE2D_PARAM(_texture,_sampler), half depthOffset,half2 uv,half2 uvOffset,uint marchCount)
{
    half deltaParallax=1.0h/marchCount;
    half2 deltaUV=uvOffset/marchCount;
    half layer=0.h;
    half parallaxSample=SAMPLE_TEXTURE2D_LOD(_texture,_sampler,uv,0).r-depthOffset;
    half preParallaxSample=0.h;
    while(parallaxSample>layer)
    {
        preParallaxSample=parallaxSample;
        parallaxSample=SAMPLE_TEXTURE2D_LOD(_texture,_sampler,uv,0).r-depthOffset;
        layer+=deltaParallax;
        uv-=deltaUV;
    }
    half d1=layer-parallaxSample;
    half d2=(layer-deltaParallax)-preParallaxSample;
    half interpolate=d1*rcp(d1-d2);
    return layer-interpolate*deltaParallax;
}

// [Header(Depth)]
// [ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
// [Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
// [Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(-.5,.5))=0
// [Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=1
// [Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=1
// [Toggle(_PARALLAX)]_Parallax("Parallax",float)=0
// [Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallax Count",int)=16

//#pragma shader_feature_local _DEPTHMAP
//#pragma shader_feature_local _PARALLAX
//#pragma shader_feature_local _DEPTHBUFFER

//TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
// INSTANCING_PROP(float,_DepthScale)
// INSTANCING_PROP(float,_DepthOffset)
// INSTANCING_PROP(float,_DepthBufferScale)
// INSTANCING_PROP(int ,_ParallaxCount)

void ParallaxUVMapping(inout half2 uv,inout float depth,inout float3 positionWS,float3x3 TBNWS,float3 viewDirWS)
{
    #if _DEPTHMAP
    half3 viewDirTS=mul(TBNWS, viewDirWS);
    half3 viewDir=normalize(viewDirTS);
    half2 uvOffset=viewDir.xy/viewDir.z*INSTANCE(_DepthScale);
    half marchDelta=saturate(dot(half3(0.h,0.h,1.h),viewDirTS));
    half parallax=0.h;
    half depthOffset=INSTANCE(_DepthOffset);
    
    #if _PARALLAX
        uint parallaxCount=INSTANCE(_ParallaxCount);
        parallaxCount=min(lerp(parallaxCount/2u,parallaxCount,marchDelta),128u);
        parallax=ParallaxMappingPOM(_DepthTex,sampler_DepthTex,depthOffset, uv,uvOffset,parallaxCount);
    #else
        parallax= SAMPLE_TEXTURE2D_LOD(_DepthTex,sampler_DepthTex,uv,0).r-depthOffset;
    #endif
    
    #if _DEPTHBUFFER
        half projectionDistance=parallax*INSTANCE(_DepthScale)*step(0.01h,marchDelta)* rcp(marchDelta);
        positionWS = positionWS- viewDirWS*projectionDistance*INSTANCE(_DepthBufferScale);
        depth=EyeToRawDepth(TransformWorldToEyeDepth(positionWS,UNITY_MATRIX_V));
    #endif
    
    uv-=uvOffset*parallax;
    #endif
}