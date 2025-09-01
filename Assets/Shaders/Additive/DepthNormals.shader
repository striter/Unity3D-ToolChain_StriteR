Shader "Runtime/Additive/DepthNormals"
{
    SubShader
    {
		Blend One Zero
		ZWrite On
		ZTest LEqual
        Cull Back
		
		NAME "MAIN"
		Tags{"LightMode" = "DepthNormals"}
		HLSLINCLUDE
		#pragma multi_compile_instancing
		#include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"

		struct a2f
		{
			float3 positionOS:POSITION;
			float3 normalOS:NORMAL;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 positionCS:SV_POSITION;
			float3 normalWS:NORMAL;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		v2f vert(a2f v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			o.positionCS = TransformObjectToHClip(v.positionOS);
			o.normalWS = TransformObjectToWorldNormal(v.normalOS);
			return o;
		}

		float4 frag(v2f i) :SV_TARGET
		{
			return float4(i.normalWS * 0.5 + 0.5,i.positionCS.z/i.positionCS.w);
		}
		ENDHLSL

		Pass
		{
			NAME "MAIN"
			Tags{ "LightMode" = "DepthNormals" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}		
    	
		Pass
		{
			NAME "DepthOnly"
			Tags{ "LightMode" = "DepthOnly" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}		
    	
    	Pass
		{
			NAME "Forward"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
		
    }
}
