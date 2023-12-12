//Depth Parallax
float ParallaxMappingPOM(TEXTURE2D_PARAM(_texture,_sampler), float depthOffset,float2 uv,float2 uvOffset,uint marchCount)
{
    float deltaParallax=1.0h/marchCount;
    float2 deltaUV=uvOffset/marchCount;
    float layer=0.h;
    float parallaxSample=SAMPLE_TEXTURE2D_LOD(_texture,_sampler,uv,0).r-depthOffset;
    float preParallaxSample=0.h;
    uint loopTimes=min(marchCount,128u);
    for(uint i=0u;i<loopTimes;i+=1u)
    {
        if(parallaxSample<layer)
            break;
        preParallaxSample=parallaxSample;
        parallaxSample=SAMPLE_TEXTURE2D_LOD(_texture,_sampler,uv,0).r-depthOffset;
        layer+=deltaParallax;
        uv-=deltaUV;
    }
    float d1=layer-parallaxSample;
    float d2=(layer-deltaParallax)-preParallaxSample;
    float interpolate=d1*rcp(d1-d2);
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

void ParallaxUVMapping(inout float2 uv,inout float depth,inout float3 positionWS,half3x3 TBNWS,half3 viewDirWS)
{
    #if _DEPTHMAP
        half3 viewDirTS=mul(TBNWS, viewDirWS);
        half3 viewDir=normalize(viewDirTS);
        float2 uvOffset=viewDir.xy/viewDir.z*INSTANCE(_DepthScale);
        float marchDelta=saturate(dot(half3(0.,0.,1.),viewDirTS));
        float parallax=0.;
        float depthOffset=INSTANCE(_DepthOffset);
        
        #if _PARALLAX
            uint parallaxCount=INSTANCE(_ParallaxCount);
            parallaxCount=min(lerp(parallaxCount/2u,parallaxCount,marchDelta),128u);
            parallax=ParallaxMappingPOM(_DepthTex,sampler_DepthTex,depthOffset, uv,uvOffset,parallaxCount);
        #else
            parallax= SAMPLE_TEXTURE2D_LOD(_DepthTex,sampler_DepthTex,uv,0).r-depthOffset;
        #endif
        
        #if _DEPTHBUFFER
            float projectionDistance=parallax*INSTANCE(_DepthScale)*step(0.01h,marchDelta)* rcp(marchDelta);
            positionWS = positionWS - viewDirWS*projectionDistance*INSTANCE(_DepthBufferScale);
            depth=EyeToRawDepth(TransformWorldToEyeDepth(positionWS,UNITY_MATRIX_V));
        #endif
        
        uv-=uvOffset*parallax;
    #endif
}