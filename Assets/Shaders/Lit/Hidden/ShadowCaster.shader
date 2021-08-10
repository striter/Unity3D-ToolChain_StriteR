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
			#include "Assets/Shaders/Library/Additional/HorizonBend.hlsl"
			#pragma shader_feature _HORIZONBEND
			
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				float3 positionWS=TransformObjectToWorld(v.positionOS.xyz);
				positionWS=HorizonBend(positionWS);
				SHADOW_CASTER_VERTEX(v,positionWS);
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}	
		
    }
}
