#define IDEPTH
half _FocalStart;
half _FocalEnd;
TEXTURE2D(_CameraFocalMaskTexture); SAMPLER(sampler_CameraFocalMaskTexture);
half3 SampleBlurTex(TEXTURE2D_PARAM(_tex,_sampler),float2 uv,float2 offset)
{
    
    #if _DOF_DISTANCE
        float2 sampleUV = uv + offset * .5f;
        float rawDepth=SampleRawDepth(sampleUV);
        half focal=saturate(invlerp(_FocalStart,_FocalEnd,RawToDistance(rawDepth,sampleUV)));
        offset*=focal;
    #endif

    #if _DOF_MASK
        offset *= (1-max(SAMPLE_TEXTURE2D(_CameraFocalMaskTexture,sampler_CameraFocalMaskTexture,uv+offset).r,SAMPLE_TEXTURE2D(_CameraFocalMaskTexture,sampler_CameraFocalMaskTexture,uv).r));
    #endif

    float4 color = SAMPLE_TEXTURE2D(_tex,_sampler,uv+offset);

    #if defined(_FIRSTBLUR) || !defined(_ENCODE)
        return color.rgb;
    #endif
    return DecodeFromRGBM(color);
}

half4 RecordBlurTex(float3 _color)
{
    #if defined(_FINALBLUR) || !defined(_ENCODE)
        return float4(_color,1);
    #endif
    return EncodeToRGBM(_color.rgb);
}
    
half3 SampleMainBlur(float2 _uv,float2 _offset)
{
    return SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),_uv,_offset);
}

half3 DualFilteringDownFilter(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half3 sum =  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv ,0)*4;
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, 1)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, 0)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv ,float2(0, -1)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, 0)*uvDelta);
    return sum*.125h;
}

half3 DualFilteringUpFilter(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half3 sum = 0;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, 2)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(2,0)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, -2)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-2, 0)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, 1)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, -1)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, 1)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, -1)*uvDelta)*2;
    return sum*0.08333h;
}

half3 TentFilter3x3(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half3 sum = 0;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, 1)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, -1)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, 1)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, -1)*uvDelta);
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, 1)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1,0)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, -1)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, 0)*uvDelta)*2;
    sum += SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , 0)*4;
    return sum*0.0625;
}
    