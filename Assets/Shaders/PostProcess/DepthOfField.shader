Shader "Hidden/PostProcess/DepthOfField"
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
            #define IDEPTH
            #include "Assets/Shaders/Library/PostProcess.hlsl"

            TEXTURE2D( _BlurTex);SAMPLER(sampler_BlurTex);
            half _FocalStart;
            half _FocalLerp;

            half GetFocalParam(half2 uv)
            {
                half depth=Sample01Depth(uv);

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
