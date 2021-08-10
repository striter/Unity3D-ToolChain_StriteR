Shader "Hidden/Test/ToonSpecular"
{
	Properties
	{
		[Header(Base Tex)]
		_AlbedoMask("Albedo Mask",2D)="white"{}
		_ColorR("Color R",Color) = (1,0,0,1)
		_ColorG("Color G",Color)=(0,1,0,1)
		_ColorB("Color B",Color)=(0,0,1,1)
		_ColorA("Color A",Color)=(0,0,0,1)
		
		[Header(Lighting)]
		[Toggle(_FLATNORMAL)]_Interpolate("Interpolate",int)=0
		_DiffuseBegin("Diffuse Begin",Range(-1,1))=0
		_DiffuseEnd("Diffuse End",Range(0,1))=0.5
		_Lambert("Lambert",Range(0,1))=0.5
		
		[Header(_Specular)]
		[Toggle(_SPECULAR)]_Specular("Specular",int)=0
		_SpecularRange("Specular Range",Range(0,50))=1
		[HDR]_SpecularCol("Specular Col",Color)=(1,1,1,1)
		
		[Header(_Fresnel)]
		[Toggle(_FRESNEL)]_Fresnel("Fresnel",int)=0
		_FresnelStrength("Fresnel",Range(0,2))=0.5
		_FresnelCol("FresnelCol",Color)=(1,1,1,1)
		
		[Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
		
		[Header(Stencil)]
		_Stencil("Stencil ID", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Comparison", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
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
			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 3.5

			#define BDRF
			#define GI
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma shader_feature_local _FLATNORMAL
			#pragma shader_feature_local _FRESNEL
			#pragma shader_feature_local _SPECULAR
			TEXTURE2D( _AlbedoMask); SAMPLER(sampler_AlbedoMask);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
				INSTANCING_PROP(float3,_ColorR)
				INSTANCING_PROP(float3,_ColorG)
				INSTANCING_PROP(float3,_ColorB)
				INSTANCING_PROP(float3,_ColorA)
				INSTANCING_PROP(float,_DiffuseBegin)
				INSTANCING_PROP(float,_DiffuseEnd)
				INSTANCING_PROP(float,_Lambert)
				INSTANCING_PROP(float,_FresnelStrength)
				INSTANCING_PROP(float4,_FresnelCol)
				INSTANCING_PROP(float,_SpecularRange)
				INSTANCING_PROP(float4,_SpecularCol)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				half3 positionOS : POSITION;
				half3 normalOS:NORMAL;
				half2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 positionCS : SV_POSITION;
				half2 uv:TEXCOORD0;
				#ifdef _FLATNORMAL
				nointerpolation
				#endif
				half3 normalWS:TEXCOORD1;
				float3 positionWS:TEXCOORD4;
				half4 positionHCS:TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = v.uv;
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS=  TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.positionHCS=o.positionCS;
				return o;
			}
			
			half4 frag(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 positionWS=i.positionWS;
				half3 normalWS=normalize(i.normalWS);
				half3 viewDirWS=normalize(TransformWorldToViewDir(positionWS,UNITY_MATRIX_V));
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
				half2 baseUV=i.uv.xy;
				half4 colorMask=SAMPLE_TEXTURE2D(_AlbedoMask,sampler_AlbedoMask,baseUV);
				half3 albedo= colorMask.r*_ColorR;
				albedo=lerp(albedo,_ColorB,colorMask.b);
				albedo=lerp(albedo,_ColorA,colorMask.a);
				albedo=lerp(albedo,_ColorG,colorMask.g);

				float ndl=dot(normalWS,lightDirWS);
				float ndv=dot(normalWS,viewDirWS);
				float geometryShadow=saturate(ndl);
				float diffuse=smoothstep(_DiffuseBegin,_DiffuseBegin+_DiffuseEnd,ndl);
				diffuse=_Lambert*diffuse+(1-_Lambert);

				float3 diffuseCol=diffuse*albedo;
				float3 finalCol=diffuseCol;
				#if _SPECULAR
					float3 halfDirWS=normalize(viewDirWS+lightDirWS);
					float specular=pow(saturate(dot(halfDirWS,normalWS)),_SpecularRange)*geometryShadow;
					finalCol=lerp(finalCol,_SpecularCol.rgb,specular*_SpecularCol.a);
				#endif
				
				#if _FRESNEL
					float fresnel=pow3(1-ndv)*geometryShadow*_FresnelStrength;
					finalCol=lerp(finalCol,_FresnelCol.rgb,fresnel*_FresnelCol.a);
				#endif
				
				return half4(finalCol,1.h);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}