Shader "Game/Unfinished/SphericalHarmonicsL2"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            float3 _L00;
            float3 _L10;
            float3 _L11;
            float3 _L12;
            float3 _L20;
            float3 _L21;
            float3 _L22;
            float3 _L23;
            float3 _L24;


            float4 _SHAr,_SHAg,_SHAb;
            float3 _SHBr,_SHBg,_SHBb,_SHC;
            
            float3 V00(float3 _v){return 0.5f * sqrt(1.0 / PI) * _L00;}
            float3 V10(float3 _v){return sqrt(3.0f / (4.0f*PI)) * _L10 * _v.z;}
            float3 V11(float3 _v){return sqrt(3.0f / (4.0f*PI)) * _L11 * _v.y;}
            float3 V12(float3 _v){return sqrt(3.0f / (4.0f*PI)) * _L12 * _v.x;}
            float3 V20(float3 _v){return .5f * sqrt(15.0f / PI ) * _L20 *_v.x*_v.y;}
            float3 V21(float3 _v){return .5f * sqrt(15.0f / PI ) * _L21 *_v.y*_v.z;}
            float3 V22(float3 _v){return .25f * sqrt(5.0f / PI ) * _L22 * _v.z * _v.z;}
            float3 V23(float3 _v){return .5f * sqrt(15.0f / PI ) * _L23 *_v.z*_v.x;}
            float3 V24(float3 _v){return .25f * sqrt(15.0f / PI ) * _L24 * (_v.x*_v.x - _v.y*_v.y);}
            
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 v = normalize(i.normalWS);
                //
                // float3 sh = SampleSHL2(v,
                //     unity_SHAr,unity_SHAg,unity_SHAb,
                //     unity_SHBr,unity_SHBg,unity_SHBb,
                //     unity_SHC
                // );
                //
                // return float4(sh,1);

                
                float3 sh = V00(v) + V10(v) + V11(v) + V12(v) + V20(v) + V21(v) + V22(v) + V23(v) + V24(v);
                return float4(sh,1);
                
                float3 sh01=0;
                float4 vA=float4(v,1);
                sh01.r=dot(vA,_SHAr);
                sh01.g=dot(vA,_SHAg);
                sh01.b=dot(vA,_SHAb);

                float4 vB = v.xyzz*v.yzzx;
                float3 sh2;
                sh2.r=dot(vB,_SHBr);
                sh2.g=dot(vB,_SHBg);
                sh2.b=dot(vB,_SHBb);

                float vC=(v.x*v.x-v.y*v.y);
                float3 sh3= _SHC * vC;
                
                return float4(sh01+sh2+sh3,1);
            }
            ENDHLSL
        }
    }
}
