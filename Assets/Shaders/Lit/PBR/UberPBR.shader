Shader "Game/Lit/UberPBR"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex(Roughness.Metallic.AO)",2D)="white"{}
		_Glossiness("Glossiness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0
		[NoScaleOffset]_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
		[Header(_Settings)]
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward,Anisotropic_Beckmann,Anisotropic_GGX)]_NDF("Normal Distribution:",float) = 1
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD,_NDF_ANISOTROPIC_GGX,_NDF_ANISOTROPIC_BECKMANN)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
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
		[Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=0
		[Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=0
		[Toggle(_PARALLAX)]_Parallax("Parallax",float)=0
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallax Count",int)=16
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
		
		[Foldout(LIGHTMAP_CUSTOM,LIGHTMAP_INTERPOLATE)]_LightmapST("CLightmap UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_CUSTOM,LIGHTMAP_INTERPOLATE)]_LightmapIndex("CLightmap Index",int)=0
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
			#pragma shader_feature_local _PBRMAP
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _DETAILNORMALMAP
			#pragma shader_feature_local _MATCAP

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
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_DetailBlendMode)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_DetailNormalTex_ST)
				INSTANCING_PROP(float,_DepthScale)
				INSTANCING_PROP(float,_DepthOffset)
				INSTANCING_PROP(float,_DepthBufferScale)
				INSTANCING_PROP(int ,_ParallaxCount)
				INSTANCING_PROP(float3,_MatCapColor)

				INSTANCING_PROP(float,_AlphaClipRange)
				INSTANCING_PROP(float4,_LightmapST)
			    INSTANCING_PROP(float,_LightmapIndex)
			INSTANCING_BUFFER_END

			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Additional/Local/Parallax.hlsl"
			#pragma shader_feature_local _PARALLAX
			#pragma shader_feature_local _DEPTHBUFFER
			#pragma shader_feature_local _DEPTHMAP
			#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
			#pragma shader_feature_local _ALPHACLIP

			struct a2f
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				float2 uv:TEXCOORD0;
				float3 color:COLOR;
				A2V_LIGHTMAP
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float3 color:COLOR;
				float4 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				half3 viewDirWS:TEXCOORD6;
				V2F_FOG(7)
				V2F_LIGHTMAP(8)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct f2o
			{
				float4 result:SV_TARGET;
				float depth:SV_DEPTH;
			};
		
			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = float4( TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),TRANSFORM_TEX_INSTANCE(v.uv,_DetailNormalTex));
				o.positionWS= TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionHCS = o.positionCS;
				o.normalWS = TransformObjectNormalToWorld(v.normalOS);
				o.tangentWS = normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS = GetViewDirectionWS(o.positionWS);
				o.color = v.color;
				LIGHTMAP_TRANSFER(v,o)
				FOG_TRANSFER(o)
				return o;
			}
			float3 CalculateAlbedo(float2 uv,float4 color)
			{
				float4 sample = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv)*INSTANCE(_Color);
				AlphaClip(sample.a);
				return sample.rgb*color.rgb;
			}

			float3 CalculateEmission(float2 uv,float4 color)
			{
				return SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,uv).rgb*INSTANCE(_EmissionColor).rgb;
			}
		
			#define TRANSFER_POSITION_WS(v) TransformObjectToWorld(v.positionOS)
			#define GET_ALBEDO(i) CalculateAlbedo(i.uv,i.color);
			#define GET_EMISSION(i) CalculateEmission(i.uv,i.color);
		
		ENDHLSL
		
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment fragForward
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON LIGHTMAP_CUSTOM LIGHTMAP_INTERPOLATE
			
            #pragma multi_compile_fog
            #pragma target 3.5
			#pragma shader_feature_local_fragment _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD _NDF_ANISOTROPIC_BECKMANN _NDF_ANISOTROPIC_GGX
			#pragma shader_feature_local_fragment _VF_BLINNPHONG _VF_GGX

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

				half smoothness=surface.smoothness;
				half roughnessT=surface.roughnessT;
				half roughnessB=surface.roughnessB;
				half TDH= lightSurface.TDH;
				half BDH= lightSurface.BDH;
				
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
				#elif _NDF_ANISOTROPIC_TROWBRIDGEREITZ
				        NDFA_TrowbridgeReitz(NDH, TDH, BDH, roughnessT, roughnessB);
				#elif _NDF_ANISOTROPIC_WARD
				        NDFA_Ward(NDL, NDV, NDH,TDH, BDH, roughnessT, roughnessB);
				#elif  _NDF_ANISOTROPIC_BECKMANN
						NDFA_Beckmann(NDH,TDH,BDH,roughnessT,roughnessB);
				#elif _NDF_ANISOTROPIC_GGX
						NDFA_GGX(NDH,TDH,BDH,roughnessT,roughnessB);
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
			
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"

			f2o fragForward(v2f i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				f2o o;
				
				o.depth=i.positionCS.z;
				float3 positionWS=i.positionWS;
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				float3 viewDirWS=normalize(i.viewDirWS);
				half3 normalTS=half3(0,0,1);
				float2 baseUV=i.uv.xy;
				ParallaxUVMapping(baseUV,o.depth,positionWS,TBNWS,viewDirWS);
				
				#if _NORMALMAP
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
					#if _DETAILNORMALMAP
						half3 detailNormalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_DetailNormalTex,sampler_DetailNormalTex,i.uv.zw));
						normalTS=BlendNormal(normalTS,detailNormalTS,INSTANCE(_DetailBlendMode));
					#endif
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
				
				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,baseUV)*INSTANCE(_Color);
				AlphaClip(color.a);
				half3 albedo=color.rgb*i.color.rgb;
				half3 emission=SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,baseUV).rgb*INSTANCE(_EmissionColor).rgb;
				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				half anisotropic=INSTANCE(_AnisoTropicValue);
				#if _PBRMAP
					half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
					glossiness*=1.h-mix.r;
					metallic*=mix.g;
					ao*=mix.b;
				#endif

				BRDFSurface surface=BRDFSurface_Ctor(albedo,emission,glossiness,metallic,ao,normalWS,tangentWS,biTangentWS,viewDirWS,anisotropic);

				half3 finalCol=0;
				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
				half3 indirectDiffuse= IndirectDiffuse(mainLight,i,normalWS);
				half3 indirectSpecular=IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
				finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
				
				#if _MATCAP
					float2 matcapUV=float2(dot(UNITY_MATRIX_V[0].xyz,normalWS),dot(UNITY_MATRIX_V[1].xyz,normalWS));
					matcapUV=matcapUV*.5h+.5h;
					mainLight.color=SAMPLE_TEXTURE2D(_Matcap,sampler_Matcap,matcapUV).rgb*INSTANCE(_MatCapColor).rgb;
					mainLight.distanceAttenuation = 1;
				#endif
				
				finalCol+=BRDFLighting(surface,mainLight);
		
				#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
            	#endif
				FOG_MIX(i,finalCol);
				finalCol+=surface.emission;

				o.result=half4(finalCol,color.a);
				return o;
			}
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
			#pragma vertex vert
			#pragma fragment DepthFragment
			f2o DepthFragment(v2f i)
			{
				f2o o;
				o.depth=i.positionCS.z;
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=GetViewDirectionWS(i.positionWS);
            	ParallaxUVMapping(i.uv.xy,o.depth,i.positionWS,TBNWS,viewDirWS);
				AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).a*INSTANCE(_Color.a));
				o.result=i.positionCS.z;
				return o;
			}
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
			#define A2V_SHADOW_DEPTH float2 uv:TEXCOORD0;
			#define V2F_SHADOW_DEPTH float2 uv:TEXCOORD0;
			#define TRANSFER_SHADOW_DEPTH(v,o) o.uv=TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
			#define MIX_SHADOW_DEPTH(i) AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).a*INSTANCE(_Color.a));
			
            #include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}
		Pass
		{
            Name "META"
            Tags{"LightMode" = "Meta"}
            Cull Back
			Blend Off
			ZWrite On
			ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VertexMeta
            #pragma fragment FragmentMeta
            #include "Assets/Shaders/Library/Passes/Meta.hlsl"
            ENDHLSL
		}
	}
	
}