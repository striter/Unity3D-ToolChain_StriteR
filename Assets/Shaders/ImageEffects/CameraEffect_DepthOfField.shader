Shader "Hidden/CameraEffect_DepthOfField"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"
        _BlurTex("Blur Tex",2D)="white"
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _UseBlurDepth
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                return o;
            }

            uniform sampler2D _MainTex;
            uniform sampler2D _BlurTex;
            uniform sampler2D _CameraDepthTexture;
            uniform half4 _CameraDepthTexture_TexelSize;
            uniform half _FocalStart;
            uniform half _FocalLerp;
            #if _UseBlurDepth
            uniform half _BlurSize;
            #endif

            half GetFocalParam(half2 uv)
            {
                half depth=Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));
                #if _UseBlurDepth
                depth=max(depth,Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv+half2(1,0)*_BlurSize*_CameraDepthTexture_TexelSize.x)));
                depth=max(depth,Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv+half2(-1,0)*_BlurSize*_CameraDepthTexture_TexelSize.x)));
                depth=max(depth,Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv+half2(0,1)*_BlurSize*_CameraDepthTexture_TexelSize.y)));
                depth=max(depth,Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv+half2(0,-1)*_BlurSize*_CameraDepthTexture_TexelSize.y)));
                #endif

                half focal=step(_FocalStart,depth)*abs((_FocalStart-depth))/_FocalLerp;
                return focal;
            }

            half4 frag (v2f i) : SV_Target
            {
                return lerp(tex2D(_MainTex, i.uv),tex2D(_BlurTex,i.uv),saturate( GetFocalParam(i.uv)));
            }
            ENDCG
        }
    }
}
