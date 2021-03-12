Shader "Game/Lit/Standard_Specular"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		
		[Header(Normal Map)]
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=0
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(Diffuse Setting)]
		_Lambert("Lambert",Range(0,1))=.5

		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back
		Blend Off
		ZWrite On
		ZTest LEqual

		CGINCLUDE
		#include "../CommonLightingInclude.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#pragma multi_compile_instancing
		#pragma shader_feature _SPECULAR
		#pragma shader_feature _NORMALMAP

		sampler2D _MainTex;
		float4 _MainTex_ST;
		#if _NORMALMAP
		sampler2D _NormalTex;
		#endif
		float _Lambert;
		float _SpecularRange;
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
		UNITY_INSTANCING_BUFFER_END(Props)

		struct a2fDV
		{
			float4 vertex : POSITION;
			float2 uv:TEXCOORD0;
			float3 normal:NORMAL;
			#if _NORMALMAP
			float4 tangent:TANGENT;
			#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2fDV
		{
			float4 pos : SV_POSITION;
			float2 uv:TEXCOORD0;
			float3 worldPos:TEXCOORD1;
			float3 worldLightDir:TEXCOORD2;
			float3 worldNormal:TEXCOORD3;
			float3 worldViewDir:TEXCOORD4;
			SHADOW_COORDS(5)
			#if _NORMALMAP
			float3x3 worldToTangent:TEXCOORD6;
			#endif
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		v2fDV DiffuseVertex(a2fDV v)
		{
			v2fDV o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);
			o.uv = TRANSFORM_TEX( v.uv,_MainTex);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			o.worldLightDir=WorldSpaceLightDir( v.vertex);
			o.worldNormal=mul(unity_ObjectToWorld,v.normal);
			o.worldViewDir=WorldSpaceViewDir( v.vertex);
			#if _NORMALMAP
			float3 worldTangent=mul(unity_ObjectToWorld,v.tangent);
			float3 worldBitangent=cross(worldTangent,o.worldNormal);
			o.worldToTangent=float3x3(normalize(worldTangent),normalize(worldBitangent),normalize(o.worldNormal));
			#endif
			TRANSFER_SHADOW(o);
			return o;
		}

		ENDCG
		Pass
		{
			NAME "FORWARDBASE"
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			CGPROGRAM
			#pragma vertex DiffuseVertex
			#pragma fragment DiffuseFragmentBase
			#pragma multi_compile_fwdbase

			float4 DiffuseFragmentBase(v2fDV i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normal=normalize(i.worldNormal);
				#if _NORMALMAP
				float3 tangentSpaceNormal= DecodeNormalMap(tex2D(_NormalTex,i.uv));
				normal= mul(tangentSpaceNormal,i.worldToTangent);
				#endif
				float3 lightDir=normalize(i.worldLightDir);
				float3 viewDir=normalize(i.worldViewDir);

				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
				float3 finalCol=tex2D(_MainTex, i.uv)*UNITY_ACCESS_INSTANCED_PROP(Props, _Color)+UNITY_LIGHTMODEL_AMBIENT.xyz;
				float diffuse=saturate( GetDiffuse(normal,lightDir,_Lambert,atten));
				finalCol*=_LightColor0.rgb*diffuse;
				#if _SPECULAR
				float specular = GetSpecular(normal,lightDir,viewDir,_SpecularRange);
				specular*=atten;
				finalCol += _LightColor0.rgb*specular;
				#endif
				return float4(finalCol,1);
			}

			ENDCG
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
	}
}