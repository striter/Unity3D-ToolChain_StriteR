Shader "Hidden/Shadowmap"
{
    Properties
    {
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="black"{}
		
		[Header(Detail Tex)]
		_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
    	
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
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
			
			#pragma multi_compile_fragment _ SHADOWS_VOLUME

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_EmissionColor)
			INSTANCING_BUFFER_END
            #include "Assets/Shaders/Library/Geometry.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"

			float4x4 _WorldToShadow;
			TEXTURE2D(_ShadowmapTexture);SAMPLER(sampler_ShadowmapTexture);
			float4 _ShadowParams;
			Light GetMainLight(v2ff i)
			{
			    Light light = GetMainLight();
				float3 positionWS = i.positionWS;
				float4 shadowCoord = mul(_WorldToShadow, float4(positionWS, 1.0));
		        ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
		        half4 shadowParams = GetMainLightShadowParams();

				float shadowMap = SampleShadowmap(TEXTURE2D_ARGS(_ShadowmapTexture, sampler_LinearClampCompare), shadowCoord, shadowSamplingData, shadowParams, false);
			    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
			    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

				float fade = saturate(distanceCamToPixel2 * float(_ShadowParams.z) + float(_ShadowParams.w));
				light.shadowAttenuation = shadowMap;
				return light;
			}
			
			#define GET_MAINLIGHT(i) GetMainLight(i)
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}
        USEPASS "Game/Additive/DepthOnly/MAIN"
        UsePass "Game/Additive/ShadowCaster/MAIN"
    }
}
