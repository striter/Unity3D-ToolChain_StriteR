Shader "Hidden/PostProcess/Bloom"
{
   Properties
   {
      _MainTex("Base (RGB)", 2D) = "white" {}
   }

   HLSLINCLUDE
        #include "../PostProcessInclude.hlsl"
       TEXTURE2D(_Bloom_Blur);SAMPLER(sampler_Bloom_Blur);

       half _Intensity;
       half _Threshold;
       
        float4 fragSampleLight(v2f_img i) : SV_Target
        {
            float4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
            color*=step(_Threshold+0.01,(color.r+color.g+color.b)/3);
            return color;
        }

        float4 fragAddBloomTex(v2f_img i) : SV_Target
        {
            float4 color;
            color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
            color += SAMPLE_TEXTURE2D(_Bloom_Blur,sampler_Bloom_Blur, i.uv)*_Intensity;
            return color;
        }

     ENDHLSL

    SubShader {
        ZTest Off Cull Off ZWrite Off Blend Off
        // 0 SampleLight
        Pass{
        HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragSampleLight
        ENDHLSL
        }
        
        // 1 AddBloomTex
        Pass{
        HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment fragAddBloomTex
        ENDHLSL
        }

    }
    FallBack Off
}
