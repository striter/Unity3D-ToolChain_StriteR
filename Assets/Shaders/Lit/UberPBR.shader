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
		[Fold(_PBRMAP)]_Glossiness("Glossiness",Range(0,1))=1
        [Fold(_PBRMAP)]_Metallic("Metalness",Range(0,1))=0
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution:",float) = 2
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		[KeywordEnum(BlinnPhong,GGX)]_VF("Vsibility * Fresnel:",float)=1
	
		[Header(Detail Tex)]
		[ToggleTex(_DETAILNORMALMAP)]_DetailNormalTex("Normal Tex",2D)="white"{}
		[Enum(Linear,0,Overlay,1,PartialDerivative,2,UDN,3,Reoriented,4)]_DetailBlendMode("Normal Blend Mode",int)=0
		[ToggleTex(_MATCAP)] [NoScaleOffset]_Matcap("Mat Cap",2D)="white"{}		
		
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
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
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
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _DETAILNORMALMAP
			#pragma shader_feature_local _MATCAP

			#pragma shader_feature_local _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
			#pragma shader_feature_local _VF_BLINNPHONG _VF_GGX
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_Matcap);SAMPLER(sampler_Matcap);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_DetailNormalTex);SAMPLER(sampler_DetailNormalTex);
			TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_DetailBlendMode)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4,_DetailNormalTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float,_DepthScale)
				INSTANCING_PROP(float,_DepthOffset)
				INSTANCING_PROP(float,_DepthBufferScale)
				INSTANCING_PROP(int ,_ParallaxCount)
				INSTANCING_PROP(float,_AlphaClipRange)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
			
			#include "Assets/Shaders/Library/Additional/HorizonBend.hlsl"
			#pragma multi_compile _ _HORIZONBEND
			
			#include "Assets/Shaders/Library/Additional/Local/Parallax.hlsl"
			#pragma shader_feature_local _PARALLAX
			#pragma shader_feature_local _DEPTHBUFFER
			#pragma shader_feature_local _DEPTHMAP
			#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
			#pragma shader_feature_local _ALPHACLIP

			struct a2f
			{
				half3 positionOS : POSITION;
				half3 normalOS:NORMAL;
				half4 tangentOS:TANGENT;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				half3 viewDirWS:TEXCOORD6;
				FOG_COORD(7)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = float4( TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),TRANSFORM_TEX_INSTANCE(v.uv,_DetailNormalTex));
				o.positionWS=TransformObjectToWorld(v.positionOS);
				o.positionWS=HorizonBend(o.positionWS);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.positionHCS=o.positionCS;
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS=TransformWorldToViewDir(o.positionWS,UNITY_MATRIX_V);
				FOG_TRANSFER(o);
				return o;
			}
		ENDHLSL
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define IBRDF
			#define IGI
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #pragma target 3.5
			
			half4 frag(v2f i,out float depth:SV_DEPTH) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				
				float3 positionWS=i.positionWS;
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				half3 normalTS=half3(0,0,1);
				float2 baseUV=i.uv.xy;
				depth=i.positionCS.z;
				ParallaxUVMapping(baseUV,depth,positionWS,TBNWS,viewDirWS);
				
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
				half3 albedo=color.rgb;

				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				half anisotropic=INSTANCE(_AnisoTropicValue);
				#if _PBRMAP
					half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
					glossiness=1.h-mix.r;
					metallic=mix.g;
					ao=mix.b;
				#endif
				BRDFSurface surface=BRDFSurface_Ctor(albedo,glossiness,metallic,ao,normalWS,tangentWS,viewDirWS);
				
				half3 brdfColor=0;
				half3 indirectDiffuse=IndirectBRDFDiffuse(surface.normal);
				half3 indirectSpecular=IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
				brdfColor+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS));
				#if _MATCAP
					float2 matcapUV=float2(dot(UNITY_MATRIX_V[0].xyz,normalWS),dot(UNITY_MATRIX_V[1].xyz,normalWS));
					matcapUV=matcapUV*.5h+.5h;
					mainLight.color=SAMPLE_TEXTURE2D(_Matcap,sampler_Matcap,matcapUV).rgb;
				#endif
				BRDFLight brdfMainLight=BRDFLight_Ctor(surface,mainLight.direction,mainLight.color,mainLight.shadowAttenuation,anisotropic);
				brdfColor+=BRDFLighting(surface,brdfMainLight);

				#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
			    {
			    	BRDFLight light=BRDFLight_Ctor(surface, GetAdditionalLight(lightIndex,i.positionWS),anisotropic);
					brdfColor+=BRDFLighting(surface,light);
			    }
            	#endif
				FOG_MIX(i,brdfColor);
				return half4(brdfColor,1.h);
			}
			ENDHLSL
		}

		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment ShadowFragment
				
			float4 ShadowFragment(v2f i,out float depth:SV_DEPTH) :SV_TARGET
			{
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(TransformWorldToViewDir(i.positionWS,UNITY_MATRIX_V));
				depth=i.positionCS.z;
            	ParallaxUVMapping(i.uv.xy,depth,i.positionWS,TBNWS,viewDirWS);
				AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).a*INSTANCE(_Color.a));
				return 0;
			}
			ENDHLSL
		}
		
		USEPASS "Hidden/ShadowCaster/MAIN"
	}
}