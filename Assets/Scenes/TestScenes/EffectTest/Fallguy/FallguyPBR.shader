Shader "Hidden/Test/FallguyPBR"
{
	Properties
	{
		[Header(Base Tex)]
		_AlbedoMask("Albedo Mask",2D)="white"{}
		_ColorR("Color R",Color) = (1,0,0,1)
		_ColorG("Color G",Color)=(0,1,0,1)
		_ColorB("Color B",Color)=(0,0,1,1)
		_ColorA("Color A",Color)=(0,0,0,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(PBR)]
		_Glossiness("Glossiness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0
		_Fresnel("Fresnel",Range(0.5,10))=2
		[Header(Roughness.Metallic.AO)]
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution:",float) = 2
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		[KeywordEnum(BlinnPhong,GGX)]_VF("Vsibility * Fresnel:",float)=1
	
		[Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Cull [_Cull]
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 3.5
			
			#include "Assets/Shaders/Library/CommonInclude.hlsl"
			#include "Assets/Shaders/Library/CommonLightingInclude.hlsl"
			#include "Assets/Shaders/Library/BRDFInclude.hlsl"
			#include "Assets/Shaders/Library/GlobalIlluminationInclude.hlsl"
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
            
			#pragma multi_compile_local _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
			#pragma multi_compile_local _VF_BLINNPHONG _VF_GGX
		
			TEXTURE2D( _AlbedoMask); SAMPLER(sampler_AlbedoMask);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
				INSTANCING_PROP(float3,_ColorR)
				INSTANCING_PROP(float3,_ColorG)
				INSTANCING_PROP(float3,_ColorB)
				INSTANCING_PROP(float3,_ColorA)
				INSTANCING_PROP(float,_Fresnel)
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_DetailBlendMode)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_DetailNormalTex_ST)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				half3 positionOS : POSITION;
				half3 normalOS:NORMAL;
				half4 tangentOS:TANGENT;
				half2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 positionCS : SV_POSITION;
				half4 uv:TEXCOORD0;
				half3 normalWS:TEXCOORD1;
				half3 tangentWS:TEXCOORD2;
				half3 biTangentWS:TEXCOORD3;
				float3 positionWS:TEXCOORD4;
				half4 positionHCS:TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = half4(v.uv,TRANSFORM_TEX_INSTANCE(v.uv,_DetailNormalTex));
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS=  TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.positionHCS=o.positionCS;
				return o;
			}
			
			half4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 positionWS=i.positionWS;
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(TransformWorldToViewDir(positionWS,UNITY_MATRIX_V));
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
				half3 normalTS=half3(0,0,1);
				half2 baseUV=i.uv.xy;
				#if _NORMALMAP
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
				half4 colorMask=SAMPLE_TEXTURE2D(_AlbedoMask,sampler_AlbedoMask,baseUV);
				half3 albedo= colorMask.r*_ColorR;
				albedo=lerp(albedo,_ColorB,colorMask.b);
				albedo=lerp(albedo,_ColorA,colorMask.a);
				albedo=lerp(albedo,_ColorG,colorMask.g);
				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				BRDFSurface surface=InitializeBRDFSurface(albedo,glossiness,metallic,ao,normalWS,tangentWS,viewDirWS);
				surface.fresnelTerm*=INSTANCE(_Fresnel);
                half3 lightDir=normalize(lightDirWS);
				half3 lightCol=_MainLightColor.rgb;
				half attenuation=MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWS));
				
				half3 brdfColor=0;
				half3 indirectDiffuse=IndirectBRDFDiffuse(surface.normal);
				half3 indirectSpecular=IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
				brdfColor+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

				BRDFLight light=InitializeBRDFLight(surface,lightDir,lightCol,attenuation,INSTANCE(_AnisoTropicValue));
				brdfColor+=BRDFLighting(surface,light);

				return half4(brdfColor,1.h);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}