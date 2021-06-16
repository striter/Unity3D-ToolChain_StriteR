Shader "Game/Lit/UberPBR"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(Detail Tex)]
		[ToggleTex(_DETAILNORMALMAP)]_DetailNormalTex("Normal Tex",2D)="white"{}
		[Enum(Linear,0,Overlay,1,PartialDerivative,2,UDN,3,Reoriented,4)]_DetailBlendMode("Normal Blend Mode",int)=0
		
		[Header(PBR)]
		[Fold(_PBRMAP)]_Glossiness("Glossiness",Range(0,1))=1
        [Fold(_PBRMAP)]_Metallic("Metalness",Range(0,1))=0
		[Header(Roughness.Metallic.AO)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex",2D)="white"{}
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution:",float) = 2
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		[KeywordEnum(BlinnPhong,GGX)]_VF("Vsibility * Fresnel:",float)=1

		[Header(_Height)]
		[ToggleTex(_PARALLEXMAP)][NoScaleOffset]_ParallexTex("Parallex Tex",2D)="white"{}
		[Foldout(_PARALLEXMAP)]_ParallexScale("Parallex Scale",Range(0.001,.2))=1
		[Foldout(_PARALLEXMAP)]_ParallexOffset("Parallex Offset",Range(0,1))=.42
		[Toggle(_PARALLEX_STEEP)]_SteepParallex("Steep Parallex",float)=0
		[Enum(_8,8,_16,16,_32,32,_64,64,_128,128)]_SteepCount("Steep Count",int)=16

		//[Header(Misc)]
  //      [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=0
  //      [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
  //      [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
  //      [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
  //      [Enum(Off,0,Back,1,Front,2)]_Cull("Cull",int)=1
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		//Blend [_SrcBlend] [_DstBlend]
		//ZWrite [_ZWrite]
		//ZTest [_ZTest]
		//Cull [_Cull]

		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "../CommonInclude.hlsl"
			#include "../CommonLightingInclude.hlsl"
			#include "../BRDFInclude.hlsl"
			#include "../GlobalIlluminationInclude.hlsl"
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma shader_feature_local _PBRMAP
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _DETAILNORMALMAP
			#pragma shader_feature_local _PARALLEXMAP
			#pragma shader_feature_local _PARALLEX_STEEP
            
			#pragma multi_compile_local _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
			#pragma multi_compile_local _VF_BLINNPHONG _VF_GGX
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_DetailNormalTex);SAMPLER(sampler_DetailNormalTex);
			TEXTURE2D(_ParallexTex);SAMPLER(sampler_ParallexTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			INSTANCING_PROP(float,_Glossiness)
			INSTANCING_PROP(float,_Metallic)
			INSTANCING_PROP(float,_DetailBlendMode)
			INSTANCING_PROP(float,_AnisoTropicValue)
			INSTANCING_PROP(int ,_SteepCount)
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float4, _Color)
			INSTANCING_PROP(float,_ParallexScale)
			INSTANCING_PROP(float,_ParallexOffset)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				half3 positionOS : POSITION;
				half2 uv:TEXCOORD0;
				half3 normalOS:NORMAL;
				half4 tangentOS:TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 positionCS : SV_POSITION;
				half2 uv:TEXCOORD0;
				half3 normalWS:TEXCOORD1;
				half3 tangentWS:TEXCOORD2;
				half3 biTangentWS:TEXCOORD3;
				half3 viewDirWS:TEXCOORD4;
				float4 shadowCoordWS:TEXCOORD5;
				half4 screenPos:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				float3 positionWS =  TransformObjectToWorld(v.positionOS);
				o.shadowCoordWS=TransformWorldToShadowCoord(positionWS);
				o.normalWS=mul((float3x3)unity_ObjectToWorld,v.normalOS);
				o.tangentWS=mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz);
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS=GetCameraPositionWS()-positionWS;
				o.screenPos=ComputeScreenPos(o.positionCS);
				return o;
			}
			#if _PARALLEXMAP
			half GetParallex(half2 uv)
			{
				return 1.h-SAMPLE_TEXTURE2D(_ParallexTex,sampler_ParallexTex,uv).r;
			}
			half2 ParallexMap(half2 uv,half3 viewDirTS)
			{
				half3 viewDir=normalize(viewDirTS);
				viewDir.z+=INSTANCE(_ParallexOffset);
				half2 uvOffset=viewDir.xy/viewDir.z*INSTANCE(_ParallexScale);
				#if _PARALLEX_STEEP
				int marchCount=lerp(INSTANCE(_SteepCount),INSTANCE(_SteepCount)/4,saturate(dot(float3(0,0,1),viewDirTS)));
				marchCount=min(marchCount,128);
				half deltaDepth=1.0/marchCount;
				half2 deltaUV=uvOffset/marchCount;
				half depthLayer=0;
				half2 curUV=uv;
				half curDepth = 0;
				for(int i=0;i<marchCount;i++)
				{
					curDepth=GetParallex(curUV).r;
					if(curDepth<=depthLayer)
						break;
					curUV-=deltaUV;
					depthLayer+=deltaDepth;
				}
				half2 preUV=curUV+deltaUV;
				half beforeDepth=GetParallex(preUV)-depthLayer+deltaDepth;
				half afterDepth=curDepth-depthLayer;
				half weight=afterDepth/(afterDepth-beforeDepth);
				curUV=preUV*weight+curUV*(1-weight);
				return curUV;
				#else
				half2 offset=uvOffset*GetParallex(uv).r;
				return uv-offset;
				#endif
			}
			#endif
			half4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				half3 normalWS=normalize(i.normalWS);
				half3 normalTS=half3(0,0,1);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				#if _PARALLEXMAP
				i.uv=ParallexMap(i.uv,mul(TBNWS, viewDirWS));
				#endif
				#if _NORMALMAP
				normalTS= SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv).xyz;
				#if _DETAILNORMALMAP
				half3 detailNormalTS=SAMPLE_TEXTURE2D(_DetailNormalTex,sampler_DetailNormalTex,i.uv).xyz;
				normalTS=BlendNormal(normalTS,detailNormalTS,INSTANCE(_DetailBlendMode));
				#else
				normalTS=DecodeNormalMap(normalTS);
				#endif
				normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_Color;
				half3 albedo=color.rgb;

				half glossiness=_Glossiness;
				half metallic=_Metallic;
				half ao=1;
				#if _PBRMAP
				half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,i.uv).rgb;
				glossiness=1.-mix.r;
				metallic=mix.g;
				ao=mix.b;
				#endif

                half3 normal=normalize(normalWS);
                half3 tangent = normalize(tangentWS);
                half3 viewDir=normalize(viewDirWS);
				BRDFSurface surface=InitializeBRDFSurface(albedo,glossiness,metallic,ao,normal,tangent,viewDir);
				
                half3 lightDir=normalize(lightDirWS);
				half3 lightCol=_MainLightColor.rgb;
				half atten=MainLightRealtimeShadow(i.shadowCoordWS);

				half3 brdfColor=0;
				half3 indirectDiffuse=IndirectBRDFDiffuse(surface.normal);
				half3 indirectSpecular=IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness,i.screenPos,normalTS);
				brdfColor+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

				BRDFLight light=InitializeBRDFLight(surface,lightDir,lightCol,atten,_AnisoTropicValue);
				brdfColor+=BRDFLighting(surface,light);
				return half4(brdfColor,1.h);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}