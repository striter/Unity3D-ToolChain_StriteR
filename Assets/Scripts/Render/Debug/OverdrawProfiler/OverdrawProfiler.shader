Shader "Hidden/OverdrawProfiler"
{
    Properties
    {
        _IncrementPerStack("Alpha Per Stack",Range(0,1))=0.1
        
        [PreRenderData]_MainTex("Main Tex",2D)="white"{}
        
		[MinMaxRange]_OverdrawAlpha("Range ",Range(0,1))=0.3
    	[HideInInspector]_OverdrawAlphaEnd("",float)=0.5
        [ColorUsage(false,true)]_OverdrawColor("Overdraw Color",Color) = (1,0,1,1)
    }
    SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Transparent" "LightMode" = "UniversalForward" }
            Name "Pre Render"
            Blend One One
            ZWrite Off
            
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float,_IncrementPerStack)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return INSTANCE(_IncrementPerStack);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Final Blit"
			Cull Off ZWrite Off ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float,_OverdrawAlpha)
                INSTANCING_PROP(float,_OverdrawAlphaEnd)
                INSTANCING_PROP(float4,_OverdrawColor)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float overdrawAmount = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).r;
                float overdrawNormalized = invlerp(INSTANCE(_OverdrawAlpha),INSTANCE(_OverdrawAlphaEnd),overdrawAmount);
                overdrawNormalized = max(0,overdrawNormalized);
                return float4(INSTANCE(_OverdrawColor).rgb * overdrawNormalized , 1 - step(overdrawNormalized,0)) ;
            }
            ENDHLSL
        }
    }
}
