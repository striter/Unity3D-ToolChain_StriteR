Shader "Runtime/Skybox/Cubemap" {
    Properties {
        _Tint ("Tint Color", Color) = (.5,.5,.5,.5)
        _Exposure ("Exposure", Range(0, 8)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset]_Tex ("Cubemap (HDR)", Cube) = "grey" {}
    }
    SubShader {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

        #ifdef UNITY_COLORSPACE_GAMMA
            #define unity_ColorSpaceGrey float4(0.5, 0.5, 0.5, 0.5)
            #define unity_ColorSpaceDouble float4(2.0, 2.0, 2.0, 2.0)
            #define unity_ColorSpaceDielectricSpec half4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)
            #define unity_ColorSpaceLuminance half4(0.22, 0.707, 0.071, 0.0) // Legacy: alpha is set to 0.0 to specify gamma mode
        #else // Linear values
            #define unity_ColorSpaceGrey float4(0.214041144, 0.214041144, 0.214041144, 0.5)
            #define unity_ColorSpaceDouble float4(4.59479380, 4.59479380, 4.59479380, 2.0)
            #define unity_ColorSpaceDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
            #define unity_ColorSpaceLuminance half4(0.0396819152, 0.458021790, 0.00609653955, 1.0) // Legacy: alpha is set to 1.0 to specify linear mode
        #endif

            samplerCUBE _Tex;
            half4 _Tex_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;

            struct appdata {
                float4 vertex : POSITION;
                float3 positionOS : TEXCOORD0;
            };

            struct v2f {
                float4 positionCS : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata v) {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex);
                o.texcoord = mul(Rotate3x3(_Rotation,float3(0,1,0)), v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                half4 tex = texCUBE(_Tex, i.texcoord);
                half3 c = DecodeHDREnvironment(tex, _Tex_HDR);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;
                return half4(c, 1);
            }
            ENDHLSL
        }
    }
}