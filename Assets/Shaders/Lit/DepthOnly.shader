Shader "Hidden/DepthOnly"
{
	Properties
	{
		[Header(Misc)]
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
		[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("Color Mask",int)=15
		[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=1
	}
	
    SubShader
    {
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
				return i.positionCS.z;
			}
    	ENDHLSL
		Pass
		{
			ZWrite On
			Blend Off
			ZTest [_ZTest]
			ColorMask [_ColorMask]
			Cull [_Cull]
			
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}		
    	
    	Pass
		{
			ZWrite On
			Blend Off
			ZTest [_ZTest]
			ColorMask [_ColorMask]
			Cull [_Cull]
			
			NAME "Forward"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}
    }
}
