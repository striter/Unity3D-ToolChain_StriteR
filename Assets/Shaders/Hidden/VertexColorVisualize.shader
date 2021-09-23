Shader "Hidden/VertexColorVisualize"
{
    Properties
    {
		[KeywordEnum(UV0,UV1,UV2,UV3,UV4,UV5,UV6,UV7,Color,Normal,Tangent)]_SAMPLE("Sample Source",float)=0
        [KeywordEnum(R,G,B,A)]_VISUALIZE("Visualize",float)=0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _SAMPLE_UV0 _SAMPLE_UV1 _SAMPLE_UV2 _SAMPLE_UV3 _SAMPLE_UV4 _SAMPLE_UV5  _SAMPLE_UV6  _SAMPLE_UV7 _SAMPLE_COLOR _SAMPLE_NORMAL _SAMPLE_TANGENT 
            #pragma multi_compile_local _ _VISUALIZE_R _VISUALIZE_G _VISUALIZE_B _VISUALIZE_A
			#include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                #if  _SAMPLE_COLOR
                float4 color:COLOR;
                #elif _SAMPLE_NORMAL
                float4 normalOS:NORMAL;
                #elif _SAMPLE_TANGENT
                float4 tangentOS:TANGENT;
                #elif _SAMPLE_UV0
                float4 uv0:TEXCOORD0;
                #elif _SAMPLE_UV1
				float4 uv1:TEXCOORD1;
				#elif _SAMPLE_UV2
				float4 uv2:TEXCOORD2;
				#elif _SAMPLE_UV3
				float4 uv3:TEXCOORD3;
				#elif _SAMPLE_UV4
				float4 uv4:TEXCOORD4;
				#elif _SAMPLE_UV5
				float4 uv5:TEXCOORD5;
				#elif _SAMPLE_UV6
				float4 uv6:TEXCOORD6;
				#elif _SAMPLE_UV7
				float4 uv7:TEXCOORD7;
				#endif
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color:COLOR;
            };


            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if _SAMPLE_COLOR
                o.color=v.color;
                #elif _SAMPLE_NORMAL
                o.color=v.normalOS;
                #elif _SAMPLE_TANGENT
                o.color=v.tangentOS;
                #elif _SAMPLE_UV0
                o.color=v.uv0;
                #elif _SAMPLE_UV1
                o.color=v.uv1;
                #elif _SAMPLE_UV2
                o.color=v.uv2;
                #elif _SAMPLE_UV3
                o.color=v.uv3;
                #elif _SAMPLE_UV4
                o.color=v.uv4;
                #elif _SAMPLE_UV5
                o.color=v.uv5;
                #elif _SAMPLE_UV6
                o.color=v.uv6;
                #elif _SAMPLE_UV7
                o.color=v.uv7;
                #endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
            #if _VISUALIZE_R
                return float4(i.color.r,i.color.r,i.color.r,1);
            #elif _VISUALIZE_G
                return float4(i.color.g,i.color.g,i.color.g,1);
            #elif _VISUALIZE_B
                return float4(i.color.b,i.color.b,i.color.b,1);
            #elif _VISUALIZE_A
                return float4(i.color.a,i.color.a,i.color.a,1);
            #else
                return i.color;
            #endif
            }
            ENDHLSL
        }
    }
}
