Shader "Hidden/CameraEffect_DepthOfField"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma shader_feature _UseBlurDepth
            #include "../CommonInclude.hlsl"
            #include "CameraEffectInclude.hlsl"

            TEXTURE2D( _BlurTex);SAMPLER(sampler_BlurTex);
            half _FocalStart;
            half _FocalLerp;
            #if _UseBlurDepth
            half _BlurSize;
            #endif

            half GetFocalParam(half2 uv)
            {
                half depth=Linear01Depth(uv);
                #if _UseBlurDepth
                depth=max(depth,Linear01Depth(uv+half2(1,0)*_BlurSize*_CameraDepthTexture_TexelSize.x));
                depth=max(depth,Linear01Depth(uv+half2(-1,0)*_BlurSize*_CameraDepthTexture_TexelSize.x));
                depth=max(depth,Linear01Depth(uv+half2(0,1)*_BlurSize*_CameraDepthTexture_TexelSize.y));
                depth=max(depth,Linear01Depth(uv+half2(0,-1)*_BlurSize*_CameraDepthTexture_TexelSize.y));
                #endif

                half focal=step(_FocalStart,depth)*abs((_FocalStart-depth))/_FocalLerp;
                return focal;
            }

            half4 frag (v2f_img i) : SV_Target
            {
                return lerp(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv),SAMPLE_TEXTURE2D(_BlurTex,sampler_BlurTex,i.uv),saturate( GetFocalParam(i.uv)));
            }
            ENDHLSL
        }
    }
}
