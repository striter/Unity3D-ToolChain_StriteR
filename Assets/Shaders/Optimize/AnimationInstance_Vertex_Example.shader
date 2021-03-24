Shader "Hidden/AnimationInstance_Vertex_Example"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _InstanceAnimationTex("Animation Texture",2D)="black"{}
        _InstanceFrameBegin("Begin Frame",int)=0
        _InstanceFrameEnd("End Frame",int)=0
        _InstanceFrameInterpolate("Frame Interpolate",Range(0,1))=1
        [KeywordEnum(None,1Bone,2Bone)]_OPTIMIZE("Optimize",float)=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        HLSLINCLUDE
            #pragma multi_compile_instancing
            #include "../CommonInclude.hlsl"
            #include "../CommonLightingInclude.hlsl"
            #include "AnimationInstanceInclude.hlsl"
            #pragma target 3.5
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            struct appdata
            {
                #if INSTANCING_ON
                uint vertexID:SV_VertexID;
                #endif
                float3 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float diffuse:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                #if INSTANCING_ON
                SampleVertexInstance(v.vertexID, v.positionOS, v.normalOS);
                #endif
                o.diffuse=dot(v.normalOS,TransformWorldToObjectNormal(_MainLightPosition.xyz));
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col*i.diffuse;
            }
            ENDHLSL
        }

        Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
            struct a2fs
            {
                A2V_SHADOW_CASTER;
                uint vertexID:SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
				
			struct v2fs
			{
				V2F_SHADOW_CASTER;
			};

			v2fs ShadowVertex(a2fs v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2fs o;
                #if INSTANCING_ON
                SampleVertexInstance(v.vertexID, v.positionOS,v.normalOS);
                #endif
                SHADOW_CASTER_VERTEX(v,o);
				return o;
			}

			float4 ShadowFragment(v2fs i) :SV_TARGET
			{
                return 1;
			}
			ENDHLSL
		}
    }
}
