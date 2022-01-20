Shader "Hidden/ShadowCaster"
{
    SubShader
    {
		Pass
		{
			Blend Off
			ZWrite On
			ZTest LEqual
			
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);

			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float,_AlphaClipRange)
			INSTANCING_BUFFER_END
			
			#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
			#pragma shader_feature_local_fragment _ALPHACLIP
			
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float2 uv:TEXCOORD0;
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(v.positionOS.xyz));
				o.uv=v.uv;
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).a*INSTANCE(_Color.a));
				return 0;
			}
			ENDHLSL
		}	
		
    }
}
