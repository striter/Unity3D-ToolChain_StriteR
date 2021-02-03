Shader "Hidden/ImageEffect_Bloom"
{
   Properties
   {
      _MainTex("Base (RGB)", 2D) = "white" {}
      _Bloom("Bloom (RGB)", 2D) = "black" {}
   }

   CGINCLUDE

       #include "UnityCG.cginc"

       sampler2D _MainTex;
       uniform half4 _MainTex_TexelSize;
       sampler2D _Bloom;

       uniform half _Intensity;
       uniform half _Threshold;
       
        //SAMPLE LIGHT
        struct v2f_tap
        {
            half4 pos : SV_POSITION;
            half2 uv20 : TEXCOORD0;
            half2 uv21 : TEXCOORD1;
            half2 uv22 : TEXCOORD2;
            half2 uv23 : TEXCOORD3;
        };
        
        v2f_tap vert4Tap(appdata_img v)
        {
            v2f_tap o;

            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv20 = v.texcoord + _MainTex_TexelSize.xy;
            o.uv21 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h, -0.5h);
            o.uv22 = v.texcoord + _MainTex_TexelSize.xy * half2(0.5h, -0.5h);
            o.uv23 = v.texcoord + _MainTex_TexelSize.xy * half2(-0.5h, 0.5h);

            return o;
        }

        fixed4 fragSampleLight(v2f_tap i) : SV_Target
        {
            fixed4 color = tex2D(_MainTex, i.uv20);
            color += tex2D(_MainTex, i.uv21);
            color += tex2D(_MainTex, i.uv22);
            color += tex2D(_MainTex, i.uv23);
            return max(color / 4 - _Threshold, 0) * _Intensity;
        }

       //ADD BLOOM TEXTURE
        struct v2f_simple
        {
            half4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
        };

        v2f_simple vertAddBloomTex(appdata_img v)
        {
            v2f_simple o;

            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        fixed4 fragAddBloomTex(v2f_simple i) : SV_Target
        {
            fixed4 color;
            color = tex2D(_MainTex, i.uv);
            color += tex2D(_Bloom, i.uv);
            return color;
        }

     ENDCG

    SubShader {
        ZTest Off Cull Off ZWrite Off Blend Off
        // 0 SampleLight
        Pass{
        CGPROGRAM
            #pragma vertex vert4Tap
            #pragma fragment fragSampleLight
        ENDCG
        }
        
        // 1 AddBloomTex
        Pass{
        CGPROGRAM
            #pragma vertex vertAddBloomTex
            #pragma fragment fragAddBloomTex
        ENDCG
        }

    }
    FallBack Off
}
