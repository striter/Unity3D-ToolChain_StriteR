Shader "Game/Additive/SceneSelectionPass"
{
    SubShader
    {
		Pass
		{
			Blend Off
			ZWrite On
			ZTest LEqual
            Cull Off
			
			NAME "MAIN"
			Tags{"LightMode" = "SceneSelectionPass"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
#if defined(GET_POSITION_WS)
	float3 positionWS = GET_POSITION_WS(v,o);
#else
	float3 positionWS=TransformObjectToWorld(v.positionOS);
#endif
				SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(v.positionOS.xyz));
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 1;
			}
			ENDHLSL
		}	
		
    }
}
