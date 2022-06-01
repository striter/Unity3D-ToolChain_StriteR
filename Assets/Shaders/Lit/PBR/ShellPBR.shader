Shader "Game/Lit/ShellPBR"
{
	Properties
	{
		_ShellDelta("Shell Delta",float)=0
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_RootColor("Root Color",Color)=(0,0,0,0)
		_EdgeColor("Edge Color",Color)=(1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Toggle(_ANISOTROPIC)]_Anisotropic("Anisotropic",int)=0
		[Foldout(_ANISOTROPIC)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		
		[Header(Fur)]
		_FurTex("Texure",2D)="white"{}
		_FurLength("Length",Range(0,1))=0.1
		_FurAlphaClip("Alpha Clip",Range(0,1))=0.5
		_FurShadow("Inner Shadow",Range(0,1))=0.5
		_FURUVDelta("UV Delta",Range(0,5))=0.1
		_FurGravity("Gravity",Range(0,1))=.1
		
		[Header(PBR)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex(Roughness.Metallic.AO)",2D)="white"{}
		_Glossiness("Glossiness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0
	
		[Header(Render Options)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend Off
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#pragma multi_compile_instancing

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_FurTex);SAMPLER(sampler_FurTex);
		
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_ShellDelta)
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4,_FurTex_ST)
				INSTANCING_PROP(float4, _RootColor)
				INSTANCING_PROP(float4, _EdgeColor)
				INSTANCING_PROP(float,_FurAlphaClip)
				INSTANCING_PROP(float,_FurLength)
				INSTANCING_PROP(float,_FurShadow)
				INSTANCING_PROP(float,_FURUVDelta)
				INSTANCING_PROP(float,_FurGravity)
			INSTANCING_BUFFER_END
			
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"

			#pragma shader_feature_local_fragment _ANISOTROPIC
		
			#pragma shader_feature_local_fragment _PBRMAP
			#pragma shader_feature_local_fragment _NORMALMAP
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
			
            #pragma multi_compile_fog
            #pragma target 3.5


		ENDHLSL
		Pass
		{
			NAME "Shell"
			Tags{"LightMode" = "UniversalForward"}
			Cull Off
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return max(0.,lightSurface.NDL);
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half sqrRoughness=surface.roughness2;
				half NDH=max(0., lightSurface.NDH);
				half normalDistribution
				#if _ANISOTROPIC
					= NDFA_TrowbridgeReitz(NDH,lightSurface.TDH,lightSurface.BDH,surface.roughnessT,surface.roughnessB);
				#else
					= NDF_CookTorrance(NDH,sqrRoughness);
				#endif

				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}
						
			float GetNormalizationTerm(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				return InvVF_GGX(max(0., lightSurface.LDH),surface.roughness);
			}
						
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"

			struct a2f
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				float3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				V2F_FOG(6)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.positionWS=TransformObjectToWorld(v.positionOS);
				o.uv = float4( TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),TRANSFORM_TEX_INSTANCE(v.uv,_FurTex));
				float delta=INSTANCE(_ShellDelta);
				float amount=delta+=(delta- delta*delta);
				
				o.positionWS+=o.normalWS*INSTANCE(_FurLength)*amount;
				// o.positionWS+=dot(o.normalWS,float3(0,1,0))*amount*float3(0,INSTANCE(_FurGravity),0);
				o.uv.w+=amount*INSTANCE(_FURUVDelta);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.positionHCS = o.positionCS;
				FOG_TRANSFER(o)
				return o;
			}
			

			float4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				
				float3 positionWS=i.positionWS;
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				float3 viewDirWS=-GetCameraRealDirectionWS(positionWS);
				half3 normalTS=half3(0,0,1);
				float2 baseUV=i.uv.xy;

				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,baseUV);
				half3 albedo=color.rgb;
				
				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				#if _PBRMAP
					half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
					glossiness=1.h-mix.r;
					metallic=mix.g;
					ao=mix.b;
				#endif

				float delta= INSTANCE(_ShellDelta);
				albedo*=lerp(INSTANCE(_RootColor).rgb,INSTANCE(_EdgeColor).rgb,delta);
				ao=saturate(ao-(1-delta)*INSTANCE(_FurShadow));
				float furSample=SAMPLE_TEXTURE2D(_FurTex,sampler_FurTex,i.uv.zw).r;
				clip(furSample-delta*delta*INSTANCE(_FurAlphaClip));
				
				#if _NORMALMAP
					float3x3 TBNWS=transpose(half3x3(tangentWS,biTangentWS,normalWS));
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
					normalWS=normalize(mul(TBNWS), normalTS));
				#endif
				
				BRDFSurface surface=BRDFSurface_Ctor(albedo,0,glossiness,metallic,ao,normalWS,tangentWS,biTangentWS,viewDirWS,INSTANCE(_AnisoTropicValue));
				
				half3 finalCol=0;
				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS),positionWS,unity_ProbesOcclusion);
				half3 indirectDiffuse= IndirectDiffuse(mainLight,i,normalWS);
				half3 indirectSpecular=IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
				finalCol+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
				
				finalCol+=BRDFLighting(surface,mainLight);
		
				#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
					finalCol+=BRDFLighting(surface, GetAdditionalLight(lightIndex,i.positionWS));
            	#endif
				FOG_MIX(i,finalCol);
				finalCol+=surface.emission;
				return half4(finalCol,1.h);
			}
			ENDHLSL
		}
		USEPASS "Hidden/ShadowCaster/MAIN"
	}
}