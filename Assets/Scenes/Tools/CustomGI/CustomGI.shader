Shader "Hidden/CustomGI"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Toggle(_WORLDUV)]_WorldUV("World UV",float) = 1
		
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
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaCutoff("Range",Range(0.01,1))=0.01
    }
    SubShader
    {
    	HLSLINCLUDE

			#pragma shader_feature_local_fragment _ALPHACLIP
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
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
				INSTANCING_PROP(float,_AlphaCutoff)
			INSTANCING_BUFFER_END
			
    	ENDHLSL
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM

            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_CUSTOM

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_vertex _ _WORLDUV
            #pragma multi_compile_fog

			#define NFOG
			#define V2F_FOG(index) half fogFactor:TEXCOORDindex;
	        #define FOG_TRANSFER(o) o.fogFactor=FogFactor(o.positionCS.z);
	        #define FOG_MIX(i,col) col=FogInterpolateCustom(col,i.fogFactor,i.positionWS);
			
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"

			SHL2Input()
			
			half3 FogInterpolateCustom(half3 srcColor,half fogFactor,half3 positionWS)
			{
				half3 sh = SHL2Sample(GetCameraRealDirectionWS(positionWS),);
			    half density = FogDesnity(fogFactor);
			    return lerp(srcColor,sh,density);
			}

			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			float3 _IrradianceParameters;
			float3 OverrideGlobalIllumination(v2ff i,BRDFSurface surface,Light mainLight)
			{
				half3 indirectDiffuse;
				half3 indirectSpecular;
				half3 normal = normalize(surface.normal);
				indirectDiffuse = SHL2Sample(normal,);
				#ifndef LIGHTMAP_CUSTOM
					indirectDiffuse *= IndirectDiffuse(mainLight,i,normal) * _IrradianceParameters.y;
					indirectSpecular = indirectDiffuse;
					return BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
				#endif

				//Custom Lightmap Goes here
				indirectDiffuse = SHL2Sample(normal,);
				indirectSpecular = indirectDiffuse;//IndirectSpecular(surface.reflectDir,surface.perceptualRoughness,1000);
			#if LIGHTMAP_ON
				#if LIGHTMAP_LOCAL
					half3 lightmap = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap,sampler_Lightmap), i.lightmapUV);
				#else
					half3 lightmap = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap), i.lightmapUV);
				#endif

				half3 skylightContribution = lightmap * _IrradianceParameters.x;
				indirectDiffuse *= skylightContribution;
				indirectSpecular *= skylightContribution;
				float mainLightIrradianceState = _IrradianceParameters.y;
				float mainLightIrradianceIntensity = _IrradianceParameters.z;
				float4 shadowMaskSample = SAMPLE_TEXTURE2D_LIGHTMAP(unity_ShadowMask,samplerunity_Lightmap,i.lightmapUV);
				int irradianceStart = floor(mainLightIrradianceState);
				int irradianceEnd = (irradianceStart + 1) ;
				float irradianceInterpolation = mainLightIrradianceState - irradianceStart;
				float mainLightIndirectIntensity = lerp( shadowMaskSample[irradianceStart%4],shadowMaskSample[irradianceEnd%4],irradianceInterpolation) * mainLightIrradianceIntensity;
		
				indirectDiffuse += mainLightIndirectIntensity * _MainLightColor.rgb ;
			#endif
				return BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
			}

				void Transfer(a2vf v,inout v2ff i)
				{
			#if _WORLDUV
					i.uv = i.positionWS.xz + i.positionWS.yz + i.positionWS.xy;
					i.uv /= 3;
			#endif
				}
				
			#define V2F_ADDITIONAL_TRANSFER(v,o) Transfer(v,o);
			
			#define GET_GI(i,surface,mainLight) OverrideGlobalIllumination(i,surface,mainLight);
			
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
			ZTest [_ZTest]
			Cull [_Cull]
			
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
			Cull Off
			
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
    }
}
