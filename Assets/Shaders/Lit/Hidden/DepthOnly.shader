Shader "Hidden/DepthOnly"
{
    SubShader
    {
		Pass
		{
			Blend Off
			ZWrite On
			ZTest LEqual
			
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/CommonInclude.hlsl"
			#include "Assets/Shaders/Library/Additional/HorizonBend.hlsl"
			#pragma shader_feature _HORIZONBEND

			
			struct a2f
			{
				float3 positionOS:POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				positionWS=HorizonBend(positionWS);
				o.positionCS=TransformWorldToHClip(positionWS);
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
