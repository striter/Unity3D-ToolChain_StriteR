Shader "Hidden/GPUAnimation"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}

        [Header(Instance)]
    	[KeywordEnum(None,Bone,Vertex)]_GPU("GPU Animation Type",float)=0
        [KeywordEnum(None,1Bone,2Bone)]_OPTIMIZE("Bone Animation Optimize",float)=0
        [NoScaleOffset] _InstanceAnimationTex("Animation Texture",2D)="black"{}
        _InstanceFrameBegin("Begin Frame",int)=0
        _InstanceFrameEnd("End Frame",int)=0
        _InstanceFrameInterpolate("Frame Interpolate",Range(0,1))=1
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        HLSLINCLUDE
            #include "Assets/Shaders/Library/CommonInclude.hlsl"
            #include "Assets/Shaders/Library/CommonLightingInclude.hlsl"
            #include "GPUAnimationInclude.hlsl"
            #pragma multi_compile_instancing
			#pragma multi_compile_local _ _GPU_BONE _GPU_VERTEX
			#pragma multi_compile_local _ _OPTIMIZE_1BONE _OPTIMIZE_2BONE
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
				#if _GPU_VERTEX
                uint vertexID:SV_VertexID;
            	#endif
            	#if _GPU_BONE
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
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

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
            	#if _GPU_BONE
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.positionOS, v.normalOS);
				#elif _GPU_VERTEX
            	SampleVertexInstance(v.vertexID,v.positionOS,v.normalOS);
            	#endif
                o.diffuse=dot(v.normalOS,normalize( TransformWorldToObjectNormal(_MainLightPosition.xyz)));
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
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
                float4 boneIndexes:TEXCOORD1;
                float4 boneWeights:TEXCOORD2;
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
                SampleBoneInstance(v.boneIndexes,v.boneWeights, v.positionOS, v.normalOS);
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
