Shader "Game/Lit/PBR/Uber"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="white"{}
		[Fold(_PBRMAP)]_Smoothness("Smoothness",Range(0,1))=.5
		[Fold(_PBRMAP)]_Metallic("Metallic",Range(0,1))=0
		
		[NoScaleOffset]_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
		
		[Header(_Settings)]
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz)]_NDF("Normal Distribution:",float) = 1
		[KeywordEnum(BlinnPhong,GGX)]_VF("Vsibility * Fresnel:",float)=1
	
		[Header(Detail Tex)]
		[ToggleTex(_DETAILNORMALMAP)]_DetailNormalTex("Normal Tex",2D)="white"{}
		[Enum(Linear,0,Overlay,1,PartialDerivative,2,UDN,3,Reoriented,4)]_DetailBlendMode("Normal Blend Mode",int)=0
		[ToggleTex(_MATCAP)] [NoScaleOffset]_Matcap("Mat Cap",2D)="white"{}
		[Foldout(_MATCAP)][HDR]_MatCapColor("MatCap Color",Color)=(1,1,1,1)
		
		[Header(Depth)]
		[ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
		[Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
		[Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(-.5,.5))=0
		[Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",int)=0
		[Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=0
		[Toggle(_PARALLAX)]_Parallax("Parallax",int)=0
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallax Count",int)=16
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaCutoff("Range",Range(0.01,1))=0.01
	}
	
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			
			#pragma multi_compile_instancing

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_Matcap);SAMPLER(sampler_Matcap);
			TEXTURE2D(_DetailNormalTex);SAMPLER(sampler_DetailNormalTex);
			TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4, _EmissionColor)
				INSTANCING_PROP(float,_DetailBlendMode)
				INSTANCING_PROP(float4,_DetailNormalTex_ST)
				INSTANCING_PROP(float,_DepthScale)
				INSTANCING_PROP(float,_DepthOffset)
				INSTANCING_PROP(float,_DepthBufferScale)
				INSTANCING_PROP(int ,_ParallaxCount)
				INSTANCING_PROP(float3,_MatCapColor)
				INSTANCING_PROP(float,_Smoothness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_AlphaCutoff)
			INSTANCING_BUFFER_END

			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Additional/Local/Parallax.hlsl"
			#pragma shader_feature_local_fragment _PARALLAX
			#pragma shader_feature_local_fragment _PBRMAP
			#pragma shader_feature_local_fragment _DEPTHBUFFER
			#pragma shader_feature_local_fragment _DEPTHMAP
			#pragma shader_feature_local_fragment _ALPHACLIP

			#define F2O_ADDITIONAL float depth:SV_DEPTH;
			void GetPBRParameters(float2 uv,inout float smoothness,inout float metallic,inout float ao)
			{
				#if _PBRMAP
					float3 pbr = SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,uv).rgb;
					smoothness = pbr.x;
					metallic = pbr.y;
					ao = pbr.z;
				#else
					smoothness = INSTANCE(_Smoothness);
					metallic = INSTANCE(_Metallic);
					ao = 1;
				#endif
			}
			#define GET_PBRPARAM(i,smoothness,metallic,ao) GetPBRParameters(i.uv,smoothness,metallic,ao)
		ENDHLSL

		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
            #pragma vertex ForwardVertex
            #pragma fragment ForwardFragment
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

			#pragma shader_feature_local _DETAILNORMALMAP
			#pragma shader_feature_local _MATCAP
			
            #pragma multi_compile_fog
            #pragma target 3.5
			#pragma shader_feature_local_fragment _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX
			#pragma shader_feature_local_fragment _VF_BLINNPHONG _VF_GGX
            
			#define V2F_ADDITIONAL float2 detailNormalUV:TEXCOORD8;
            #define V2F_ADDITIONAL_TRANSFER(v,o) o.detailNormalUV = TRANSFORM_TEX_INSTANCE(v.uv,_DetailNormalTex);
			#define BRDF_SURFACE_INITIALIZE_ADDITIONAL float2 detailnormalUV;
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"

			void SurfaceInitialize(v2ff i,inout BRDFInitializeInput _input,inout f2of o)
			{
				_input.detailnormalUV = i.detailNormalUV;
				o.depth=i.positionCS.z;
				ParallaxUVMapping(_input.uv,o.depth,_input.positionWS,_input.TBNWS,_input.viewDirWS);
			}
			#define BRDF_SURFACE_INITIALIZE_ADDITIONAL_TRANSFER(i,input,o) SurfaceInitialize(i,input,o);

			half3 GetNormalTS(float2 uv,float2 uv2)
			{
				half3 normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv));
				#if _DETAILNORMALMAP
					half3 detailNormalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_DetailNormalTex,sampler_DetailNormalTex,uv2));
					normalTS=BlendNormal(normalTS,detailNormalTS,INSTANCE(_DetailBlendMode));
				#endif
				return normalTS;
			}
			#define GET_NORMAL(input) GetNormalTS(input.uv,input.detailnormalUV)
            

			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return max(0., lightSurface.NDL);
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half roughness=surface.roughness;
				half sqrRoughness=surface.roughness2;
				half3 normal=surface.normal;
				half NDV=max(0., surface.NDV);
				half NDH=max(0., lightSurface.NDH);
				half NDL=max(0., lightSurface.NDL);

				half smoothness = surface.smoothness;
				half TDH = lightSurface.TDH;
				half BDH = lightSurface.BDH;

				half normalDistribution=
			#if _NDF_BLINNPHONG
			        NDF_BlinnPhong(NDH, smoothness,max(1, smoothness *40));
			#elif _NDF_COOKTORRANCE
			        NDF_CookTorrance(NDH,sqrRoughness);
			#elif _NDF_BECKMANN
			        NDF_Beckmann(NDH,sqrRoughness);
			#elif _NDF_GAUSSIAN
			        NDF_Gaussian(NDH,sqrRoughness);
			#elif _NDF_GGX
			        NDF_GGX(NDH,roughness,sqrRoughness);
			#elif _NDF_TROWBRIDGEREITZ
			        NDF_TrowbridgeReitz(NDH,sqrRoughness);
			#else
				0;
			#endif
				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}

			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				float LDH=max(0., lightSurface.LDH);
			#if _VF_GGX
			    return InvVF_GGX(LDH,surface.roughness);
			#elif _VF_BLINNPHONG
			    return InvVF_BlinnPhong(LDH);
			#else
			    return 0;
			#endif
			}

			#define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input)
			#define GET_GEOMETRYSHADOW(surface,input) GetGeometryShadow(surface,input)
	        #define GET_NORMALIZATIONTERM(surface,input) GetNormalizationTerm(surface,input)

			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
            
            Light GetMainLight(float3 positionWS,float3 normalWS)
			{
				Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
			#if _MATCAP
				float2 matcapUV=float2(dot(UNITY_MATRIX_V[0].xyz,normalWS),dot(UNITY_MATRIX_V[1].xyz,normalWS));
				matcapUV=matcapUV*.5h+.5h;
				mainLight.color=SAMPLE_TEXTURE2D(_Matcap,sampler_Matcap,matcapUV).rgb*INSTANCE(_MatCapColor).rgb;
				mainLight.distanceAttenuation = 1;
			#endif
				return mainLight;
			}
			#define GET_MAINLIGHT(i) GetMainLight(i.positionWS,i.normalWS) 

			float3 GetGlobalIllumination(v2ff i,BRDFSurface surface,Light mainLight)
			{
				half3 indirectDiffuse = IndirectDiffuse(mainLight,i,surface.normal);
				half3 indirectSpecular = IndirectSpecularWithSSR(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,surface.normalTS);
				return BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
			}
			#define GET_GI(i,surface,mainLight) GetGlobalIllumination(i,surface,mainLight);
            
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			ENDHLSL
		}

		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest [_ZTest]
			Cull [_Cull]
			
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
            #include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			ENDHLSL
		}
		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull [_Cull]
			
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}
		Pass
		{
            Name "META"
            Tags{"LightMode" = "Meta"}
			Cull Off

            HLSLPROGRAM
            #pragma vertex VertexMeta
            #pragma fragment FragmentMeta
            #include "Assets/Shaders/Library/Passes/MetaPBR.hlsl"
            ENDHLSL
		}
		Pass
		{
            Tags{"LightMode" = "SceneSelectionPass"}
            ZWrite On

            HLSLPROGRAM
            #pragma vertex VertexSceneSelection
            #pragma fragment FragmentSceneSelection
            #include "Assets/Shaders/Library/Passes/SceneOutlinePass.hlsl"
            ENDHLSL
		}
	}
	
}