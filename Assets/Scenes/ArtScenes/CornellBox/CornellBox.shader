Shader "Hidden/CornellBox"
{
    Properties
    {
        [NoScaleOffset] _MainTex("Main Tex",2D)="white"{}
    }
    SubShader
    {
        ZWrite On
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 positionHCS : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS = o.positionCS;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	float2 screenUV=TransformHClipToNDC(i.positionHCS);
                float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,screenUV).rgb;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
