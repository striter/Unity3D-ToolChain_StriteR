Shader "Hidden/DepthOnly"
{
    SubShader
    {
		Blend Off
		ZWrite On
		ZTest LEqual
			
    	HLSLINCLUDE
			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Additional/HorizonBend.hlsl"
			#pragma multi_compile_ _ _HORIZONBEND

			
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
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment

			ENDHLSL
		}		
    	
    	Pass
		{
			NAME "Forward"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment

			ENDHLSL
		}
    }
}
