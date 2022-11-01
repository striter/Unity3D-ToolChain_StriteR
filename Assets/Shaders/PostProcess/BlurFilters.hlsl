
    
half3 SampleBlurTex(TEXTURE2D_PARAM(_tex,_sampler),float2 uv,float2 offset)
{
    #if defined(_DOF)||defined(_DOF_CLIPSKY)
    float rawDepth=SampleRawDepth(uv+offset);
    half focal=saturate(invlerp(_FocalStart,_FocalEnd,RawToEyeDepth(rawDepth)));
    offset*=focal;
    #elif _DOF_MASK
    offset *= (1-max(SAMPLE_TEXTURE2D(_CameraMaskTexture,sampler_CameraMaskTexture,uv+offset).r,SAMPLE_TEXTURE2D(_CameraMaskTexture,sampler_CameraMaskTexture,uv).r));
    #endif

    float4 color = SAMPLE_TEXTURE2D(_tex,_sampler,uv+offset);

    #if defined(_FIRSTBLUR)||!defined(_ENCODE)
        return color.rgb;
    #endif
    return DecodeFromRGBM(color);
}

half4 RecordBlurTex(float3 _color)
{
    #if defined(_FINALBLUR)||!defined(_ENCODE)
        return float4(_color,1);
    #endif
    return EncodeToRGBM(_color.rgb);
}
    
half3 SampleMainBlur(float2 uv,float2 offset)
{
    return SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),uv,offset);
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
    