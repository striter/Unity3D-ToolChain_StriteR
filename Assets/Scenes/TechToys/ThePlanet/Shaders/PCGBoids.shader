Shader "PCG/Bird"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        [HDR]_Emission("Emission",Color)=(0,0,0,0)
        _Scale("Scale",Range(0.5,2))=1
        _Speed("Speed",Range(0.1,20))=1
        
        _Anim1("Anim1",Range(0,1))=1
        _Anim2("Anim1",Range(0,1))=1
        _Anim3("Anim1",Range(0,1))=1
        _Anim4("Anim1",Range(0,1))=1
    }
    SubShader
    {
        Tags { "Queue"="Geometry" }
		HLSLINCLUDE

	        #include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
	        INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_EmissionColor)
		        INSTANCING_PROP(float,_Scale)
		        INSTANCING_PROP(float,_Speed)
		        INSTANCING_PROP(float,_Anim1)
		        INSTANCING_PROP(float,_Anim2)
		        INSTANCING_PROP(float,_Anim3)
		        INSTANCING_PROP(float,_Anim4)
	        INSTANCING_BUFFER_END
			float3 GetAnimPositionWS(float3 positionOS,float2 uv1,float2 uv2)
			{
	            positionOS.y+=uv1.x*INSTANCE( _Anim1)*INSTANCE(_Scale);
	            positionOS.y+=uv1.y*INSTANCE(_Anim2)*INSTANCE(_Scale);
	            positionOS.y+=uv2.x* INSTANCE(_Anim3)*INSTANCE(_Scale);
	            positionOS.y+=uv2.y*INSTANCE(_Anim4)*INSTANCE(_Scale);
				return TransformObjectToWorld(positionOS);
			}
			#define A2V_ADDITIONAL float2 uv1:TEXCOORD1;float2 uv2:TEXCOORD2;
			#define GET_POSITION_WS(v,o) GetAnimPositionWS(v.positionOS,v.uv1,v.uv2)
		ENDHLSL
    	
    	
        Pass
        {
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "PCGInclude.hlsl"
			
			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			#define _NORMALOFF
			void GetPBRParameters(inout float g,inout float m,inout float a) { g = 0.5; m = 0; a = 1; }
			#define GET_PBRPARAM(glossiness,metallic,ao) GetPBRParameters(glossiness,metallic,ao)
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
        }
        
		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
			ENDHLSL
		}
		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}
    }
}
