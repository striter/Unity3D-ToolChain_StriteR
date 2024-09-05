Shader "Game/Unfinished/VoxelGeometry"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Pass
        {
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
	        #include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			
			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			
	        INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
			INSTANCING_BUFFER_END
			#define _NORMALOFF
			void GetPBRParameters(inout float g,inout float m,inout float a) { g = 0.5; m = 0; a = 1; }
			float3 GetAlbedo(float2 uv,float4 color) { return color.rgb*color.a;}
			#define GET_PBRPARAM(i,smoothness,metallic,ao) GetPBRParameters(smoothness,metallic,ao)
			#define GET_ALBEDO(i) GetAlbedo(i.uv,i.color);
			#define GET_EMISSION 0;
			
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
