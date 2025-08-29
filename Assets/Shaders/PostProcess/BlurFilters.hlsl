half2 _FocalDistances;
half _Encode;
half _Decode;
float4 _TiltShiftParameters; //focal center , fade start radius , fade end radius
TEXTURE2D(_CameraFocalMaskTexture); SAMPLER(sampler_CameraFocalMaskTexture);
TEXTURE2D(_BlurTex);SAMPLER(sampler_BlurTex);
float4 _BlurTex_TexelSize;
half4 SampleBlurTex(TEXTURE2D_PARAM(_tex,_sampler),float2 uv,float2 offset)
{
    float2 sampleUV = uv + offset * .5f;
    float depth=RawToDistance(SampleRawDepth(sampleUV),sampleUV);
    #if _DOF || _DOF_MASK
        offset *= saturate(invlerp(_FocalDistances.x,_FocalDistances.y,depth));
    #elif _TILT_SHIFT
        offset *=  saturate(invlerp(_TiltShiftParameters.y,_TiltShiftParameters.z, abs(depth - _TiltShiftParameters.x)));
    #endif

    #if _MASK || _DOF_MASK
        offset *= (1-max(SAMPLE_TEXTURE2D(_CameraFocalMaskTexture,sampler_CameraFocalMaskTexture,uv+offset).r,SAMPLE_TEXTURE2D(_CameraFocalMaskTexture,sampler_CameraFocalMaskTexture,uv).r));
    #endif

    
    half4 color = SAMPLE_TEXTURE2D(_tex,_sampler,uv+offset);

    if(_Decode <= .9)
        return color.rgba;
    
    return half4(DecodeFromRGBM(color),1);
}

half4 RecordBlurTex(half4 _color)
{
    if(_Encode <= .9)
        return _color;
    
    return EncodeToRGBM(_color.rgb);
}
    
half4 SampleMainBlur(float2 _uv,float2 _offset)
{
    return SampleBlurTex(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),_uv,_offset);
}

half4 DualFilteringDownFilter(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half4 sum =  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv ,0)*4;
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(0, 1)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(1, 0)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv ,float2(0, -1)*uvDelta);
    sum +=  SampleBlurTex(TEXTURE2D_ARGS(_tex,_sampler),_uv , float2(-1, 0)*uvDelta);
    return sum*.125h;
}

half4 DualFilteringUpFilter(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half4 sum = 0;
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

half4 TentFilter3x3(TEXTURE2D_PARAM(_tex,_sampler),float2 _uv,float4 _texelSize,half _blurSize)
{
    float2 uvDelta=_texelSize.xy *_blurSize;
    half4 sum = 0;
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
    