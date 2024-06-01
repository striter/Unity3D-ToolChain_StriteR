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
            float4 _SHAr,_SHAg,_SHAb;
            float3 _SHBr,_SHBg,_SHBb,_SHC;
            
            float3 _L00;
            float3 _L10;
            float3 _L11;
            float3 _L12;
            float3 _L20;
            float3 _L21;
            float3 _L22;
            float3 _L23;
            float3 _L24;

            #define kSQRT2 1.4142135623731
            #define kSQRT3 1.7320508075689
            #define kSQRT5 2.2360679774998
            #define kSQRT15 3.8729833462074
            #define kSQRT21 4.5825756949558
            #define kSQRT35 5.3851648071345
            #define kSQRT105 7.0710678118655
            #define kSQRTPi 1.7724538509055

            float3 V00(float3 p){return (1.0f/(2.0 * kSQRTPi)) * _L00;}
            float3 V10(float3 p){return (-kSQRT3*p.y/(2*kSQRTPi)) * _L10;}
            float3 V11(float3 p){return (kSQRT3*p.z/(2*kSQRTPi)) * _L11;}
            float3 V12(float3 p){return (-kSQRT3*p.x/(2*kSQRTPi)) * _L12;}
            float3 V20(float3 p){return (kSQRT15 * p.y * p.x/(2*kSQRTPi)) * _L20;}
            float3 V21(float3 p){return (-kSQRT15 * p.y * p.z /(2*kSQRTPi)) * _L21;}
            float3 V22(float3 p){return (kSQRT5*(3 * p.z * p.z - 1) / (4*kSQRTPi)) * _L22;}
            float3 V23(float3 p){return (-kSQRT15 * p.x * p.z /(2*kSQRTPi)) * _L23;}
            float3 V24(float3 p){return (kSQRT15 * (p.x * p.x - p.y*p.y) / (4*kSQRTPi)) * _L24;}
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
