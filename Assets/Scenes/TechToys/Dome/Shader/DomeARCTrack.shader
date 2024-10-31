Shader "Dome/Lit_ARCTrack"
{
    Properties
    {
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
    	[Vector2]_TrackMovement("Movement",Vector) = (0,0,0,0)
    	
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
//		[Header(Tooness)]
//    	[MinMaxRange]_GeometryShadow("Geometry Shadow",Range(0,1))=0
//    	[HideInInspector]_GeometryShadowEnd("",float)=0.1
//		_IndirectSpecularOffset("Indirect Specular Offset",Range(-7,7))=0
		
		[Header(Detail Tex)]
		[NoScaleOffset]_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
    }
	
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float2,_TrackMovement)
				INSTANCING_PROP(float,_GeometryShadow)
				INSTANCING_PROP(float,_GeometryShadowEnd)
				INSTANCING_PROP(float,_IndirectSpecularOffset)
			INSTANCING_BUFFER_END
            
			#define V2F_ADDITIONAL float lTrack:TEXCOORD8;
			#define V2F_ADDITIONAL_TRANSFER(v,o) o.lTrack = step(v.positionOS,0);
			#define BRDF_SURFACE_INITIALIZE_ADDITIONAL float lTrack;
			#define BRDF_SURFACE_INITIALIZE_ADDITIONAL_TRANSFER(i,input,o) input.lTrack = i.lTrack;
			
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			
			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
			    half sqrRoughness=surface.roughness2;
			    half NDH=lightSurface.NDH;

			    NDH = saturate(NDH);
			    float d = NDH * NDH * (sqrRoughness-1.f) +1.00001f;
							
			    float specular= sqrRoughness / (d * d);
			    specular = clamp(specular,0,100);
			    
			    half steps = max(((1 - surface.smoothness) * 2), 0.01);
			    float toonSpecular = round(specular * steps) / steps;
			    return toonSpecular;
			}

			void SurfaceOverride(BRDFInitializeInput input,inout BRDFSurface surface)
			{
				surface.ao = surface.ao*input.color.a;
			}

			float4 GetAlbedoOverride(inout BRDFInitializeInput i)
			{
				float4 sample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy);

				float isTrack = sample.a;
				float leftTrack = i.lTrack;
				float trackMove = INSTANCE(_TrackMovement).x * leftTrack + (1-leftTrack) * INSTANCE(_TrackMovement).y;

				i.uv.xy += float2(trackMove,0);
				float3 trackSample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy);
				return float4(lerp(sample.rgb,trackSample,isTrack),1);
			}
            
            // float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			// {
			    // return saturate(invlerp(_GeometryShadow,_GeometryShadowEnd, lightSurface.NDL));
			// }
			// #define GET_GEOMETRYSHADOW(surface,lightSurface) GetGeometryShadow(surface,lightSurface)
	        #define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input)
			#define GET_ALBEDO(i) GetAlbedoOverride(i);
			// #define GET_GI(i,(surface) IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,INSTANCE(_IndirectSpecularOffset));
            #define BRDF_SURFACE_ADDITIONAL_TRANSFER(input,surface) SurfaceOverride(input,surface)
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
