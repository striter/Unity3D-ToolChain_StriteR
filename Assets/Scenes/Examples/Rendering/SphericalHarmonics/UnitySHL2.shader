Shader "Game/Unfinished/UnitySHL2"
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

            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float3 v = normalize(i.normalWS);
                
                float3 sh01=0;
                float4 vA=float4(v,1);
                sh01.r=dot(vA,unity_SHAr);
                sh01.g=dot(vA,unity_SHAg);
                sh01.b=dot(vA,unity_SHAb);

                float4 vB = v.xyzz*v.yzzx;
                float3 sh2;
                sh2.r=dot(vB,unity_SHBr);
                sh2.g=dot(vB,unity_SHBg);
                sh2.b=dot(vB,unity_SHBb);
                
                float vC=(v.x*v.x-v.y*v.y);
                sh2 += unity_SHC * vC;
                
                return float4(sh01+sh2,1);
                // return float4(SampleSH(normalize(i.normalWS)),1);
            }
            ENDHLSL
        }
        
    }
}
