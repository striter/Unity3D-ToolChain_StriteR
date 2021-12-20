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
			
			struct a2f
			{
				float3 positionOS:POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
			};

			v2f DepthVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				o.positionCS=TransformWorldToHClip(positionWS);
				return o;
			}

			float4 DepthFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
    	ENDHLSL
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}		
    	
    	Pass
		{
			NAME "Forward"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}
    }
}
