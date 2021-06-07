Shader "Game/Lit/UberPBR"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		[Header(PBR)]
		[Fold(_ROUGHNESSMAP)]_Glossiness("Glossiness",Range(0,1))=1
		[ToggleTex(_ROUGHNESSMAP)][NoScaleOffset]_RoughnessTex("Roughness Tex",2D)="white"{}
        [Fold(_METALLICMAP)]_Metallic("Metalness",Range(0,1))=0
		[ToggleTex(_METALLICMAP)][NoScaleOffset]_MetallicTex("Metallic Tex",2D)="white"{}

		[Header(_Functions)]
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution Function:",float) = 2
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD)]_AnisoTropicValue("Anisotropic Value",Range(0,1))=1
        [KeywordEnum(Implicit,AshikhminShirley,AshikhminPremoze,Duer,Neumann,Kelemen,CookTorrance,Ward,R_Kelemen_Modified,R_Kurt,R_WalterEtAl,R_SmithBeckmann,R_GGX,R_Schlick,R_Schlick_Beckmann,R_Schlick_GGX)]_GSF("Geometry Shadow Function:",float)=0
        [KeywordEnum(Schlick,Schlick_IOR,SphericalGaussian)]_F("Fresnel Function",float)=0
		[Foldout(_F_SCHLICK_IOR)]_Ior("Ior",  Range(1,4)) = 1.5

		[Header(Additional Mapping)]
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[ToggleTex(_AOMAP)][NoScaleOffset]_AOTex("AO Tex",2D)="white"{}
		[ToggleTex(_PARALLEXMAP)][NoScaleOffset]_ParallexTex("Parallex Tex",2D)="white"{}
		[Foldout(_PARALLEXMAP)]_ParallexScale("Parallex Scale",Range(0.001,.2))=1
		[Foldout(_PARALLEXMAP)]_ParallexOffset("Parallex Offset",Range(0,1))=.42
		[Toggle(_PARALLEX_STEEP)]_SteepParallex("Steep Parallex",float)=0
		[Enum(_8,8,_16,16,_32,32,_64,64,_128,128)]_SteepCount("Steep Count",int)=16
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back
		Blend Off

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
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma shader_feature_local _ROUGHNESSMAP
			#pragma shader_feature_local _METALLICMAP
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _PARALLEXMAP
			#pragma shader_feature_local _PARALLEX_STEEP
			#pragma shader_feature_local _AOMAP

			#pragma multi_compile_local _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
            #pragma multi_compile_local _GSF_IMPLICIT _GSF_ASHIKHMINSHIRLEY _GSF_ASHIKHMINPREMOZE _GSF_DUER _GSF_NEUMANN _GSF_KELEMEN _GSF_COOKTORRANCE _GSF_WARD _GSF_R_KELEMEN_MODIFIED _GSF_R_KURT _GSF_R_WALTERETAL _GSF_R_SMITHBECKMANN _GSF_R_GGX _GSF_R_SCHLICK _GSF_R_SCHLICK_BECKMANN _GSF_R_SCHLICK_GGX
            #pragma multi_compile_local _F_SCHLICK _F_SCHLICK_IOR _F_SPHERICALGAUSSIAN
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_RoughnessTex);SAMPLER(sampler_RoughnessTex);
			TEXTURE2D(_MetallicTex);SAMPLER(sampler_MetallicTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_ParallexTex);SAMPLER(sampler_ParallexTex);
			TEXTURE2D(_AOTex);SAMPLER(sampler_AOTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			INSTANCING_PROP(float,_Glossiness)
			INSTANCING_PROP(float,_Metallic)
			INSTANCING_PROP(float,_AnisoTropicValue)
			INSTANCING_PROP(float,_Ior)
			INSTANCING_PROP(int ,_SteepCount)
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float4, _Color)
			INSTANCING_PROP(float,_ParallexScale)
			INSTANCING_PROP(float,_ParallexOffset)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				float3 positionOS : POSITION;
				float2 uv:TEXCOORD0;
				float2 lightmapUV:TEXCOORD1;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 normalWS:TEXCOORD1;
				float3 tangentWS:TEXCOORD2;
				float3 biTangentWS:TEXCOORD3;
				float3 viewDirWS:TEXCOORD4;
				float4 shadowCoordWS:TEXCOORD5;
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
				return o;
			}
			#if _PARALLEXMAP
			float GetParallex(float2 uv)
			{
				return 1.0-SAMPLE_TEXTURE2D(_ParallexTex,sampler_ParallexTex,uv);
			}
			float2 ParallexMap(float2 uv,float3 viewDirTS)
			{
				float3 viewDir=normalize(viewDirTS);
				viewDir.z+=INSTANCE(_ParallexOffset);
				float2 uvOffset=viewDir.xy/viewDir.z*INSTANCE(_ParallexScale);
				#if _PARALLEX_STEEP
				int marchCount=lerp(INSTANCE(_SteepCount),INSTANCE(_SteepCount)/4,saturate(dot(float3(0,0,1),viewDirTS)));
				marchCount=min(marchCount,128);
				float deltaDepth=1.0/marchCount;
				float2 deltaUV=uvOffset/marchCount;
				float depthLayer=0;
				float2 curUV=uv;
				float curDepth;
				for(int i=0;i<marchCount;i++)
				{
					curDepth=GetParallex(curUV).r;
					if(curDepth<=depthLayer)
						break;
					curUV-=deltaUV;
					depthLayer+=deltaDepth;
				}
				float2 preUV=curUV+deltaUV;
				float beforeDepth=GetParallex(preUV)-depthLayer+deltaDepth;
				float afterDepth=curDepth-depthLayer;
				float weight=afterDepth/(afterDepth-beforeDepth);
				curUV=preUV*weight+curUV*(1-weight);
				return curUV;
				#else
				float2 offset=uvOffset*GetParallex(uv).r;
				return uv-offset;
				#endif
			}
			#endif
			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				float3 biTangentWS=normalize(i.biTangentWS);
				float3 tangentWS=normalize(i.tangentWS);
				float3 viewDirWS=normalize(i.viewDirWS);
				float3 lightDirWS=normalize(_MainLightPosition.xyz);
				float3x3 TBNWS=float3x3(tangentWS,biTangentWS,normalWS);
				#if _PARALLEXMAP
				i.uv=ParallexMap(i.uv,mul(TBNWS, viewDirWS));
				#endif
				#if _NORMALMAP
				float3 normalTS=normalize( DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv).xyz));
				normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif

				float4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_Color;
				float3 albedo=color.rgb;
				
				float alpha=color.a;
				float glossiness=
				#if _ROUGHNESSMAP
				(1-SAMPLE_TEXTURE2D(_RoughnessTex,sampler_RoughnessTex,i.uv));
				#else
				_Glossiness;
				#endif

				float metallic=
				#if _METALLICMAP
				SAMPLE_TEXTURE2D(_MetallicTex,sampler_MetallicTex,i.uv);
				#else
				_Metallic;
				#endif
				
				float ao=1;
				#if _AOMAP
				ao*=SAMPLE_TEXTURE2D(_AOTex,sampler_AOTex,i.uv);
				#endif

				Light mainlight=GetMainLight(i.shadowCoordWS);
				half3 bakcedGI=0;

                float3 normal=normalize(normalWS);
                float3 tangent = normalize(tangentWS);
                float3 viewDir=normalize(viewDirWS);
				BRDFSurface surface=InitializeBRDFSurface(albedo,glossiness,metallic,ao,_Ior,normal,tangent,viewDir);
				
                float3 lightDir=normalize(lightDirWS);
				float3 lightCol=_MainLightColor.rgb;
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);

				float3 brdfColor=0;
				
				brdfColor+=BRDFGlobalIllumination(surface);

				BRDFLight light=InitializeBRDFLight(surface,lightDir,lightCol,atten,_AnisoTropicValue);
				brdfColor+=BRDFLighting(surface,light);
				return float4(brdfColor,1);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}