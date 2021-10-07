Shader "Hidden/GPUAnimation"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}

        [Header(GPU Animation)]
        [NoScaleOffset] _AnimTex("Animation Texture",2D)="black"{}
        _AnimFrameBegin("Begin Frame",int)=0
        _AnimFrameEnd("End Frame",int)=0
        _AnimFrameInterpolate("Frame Interpolate",Range(0,1))=0        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        HLSLINCLUDE
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "GPUAnimationInclude.hlsl"
            #pragma multi_compile_instancing
			#pragma shader_feature_local _ANIM_TRANSFORM _ANIM_VERTEX
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
				#if _ANIM_VERTEX
                uint vertexID:SV_VertexID;
            	#endif
            	#if _ANIM_TRANSFORM
                float4 transformIndexes:TEXCOORD1;
                float4 transformWeights:TEXCOORD2;
            	#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float diffuse:TEXCOORD1;
            };

            TEXTURE2D( _MainTex);SAMPLER(sampler_MainTex);

            v2f vert (a2v v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
            	#if _ANIM_TRANSFORM
                SampleTransform(v.transformIndexes,v.transformWeights, v.positionOS, v.normalOS);
				#elif _ANIM_VERTEX
            	SampleVertex(v.vertexID,v.positionOS,v.normalOS);
            	#endif
                o.diffuse=saturate(dot(v.normalOS,normalize( TransformWorldToObjectNormal(_MainLightPosition.xyz))));
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
                return col*(i.diffuse*.5+.5);
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
				#if _ANIM_VERTEX
                uint vertexID:SV_VertexID;
            	#endif
            	#if _ANIM_TRANSFORM
                float4 transformIndexes:TEXCOORD1;
                float4 transformWeights:TEXCOORD2;
            	#endif
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
            	#if _ANIM_TRANSFORM
                SampleTransform(v.transformIndexes,v.transformWeights, v.positionOS, v.normalOS);
				#elif _ANIM_VERTEX
            	SampleVertex(v.vertexID,v.positionOS,v.normalOS);
            	#endif
                SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(v.positionOS));
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
