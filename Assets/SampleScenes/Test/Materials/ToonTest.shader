Shader "ToonTest"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_AOTex("AO Tex",2D)="black"{}
		_Color("Add Color",Color)=(1,1,1,1)
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=1
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		_NormalStrength("Nomral Strength",Range(0,2))=.5
		[Header(Diffuse Setting)]
		_DiffuseCenter("_Diffuse Center",Range(0,1))=.5
		_DiffuseLightIntensity("_Diffuse Light Intensity",Range(0,1))=1
		_DiffuseDarkIntensity("_Diffuse Dark Intensity",Range(0,1))=.2
		[Header(Scatter Setting)]
		[Toggle(_SCATTER)]_EnableScatter("Enable Scatter",float)=1
		[NoScaleOffset]_ScatterTex("Scatter Tex",2D)="white"{}
		_ScatterOffset("Scatter Offset",Range(0,1))=.5
		[Toggle(_SCATTER_DISTORT)]_EnableScatterDistort("Enable Scatter Distort",float)=0
		_ScatterDistort("Scatter Vertex Distort",Range(0,1))=.2
		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularColor("Specular Color",Color)=(1,1,1,1)
		_SpecularRange("Specular Range",Range(.9,1))=.98
	}
	SubShader
	{
		Tags {"RenderType"="Opaque" "Queue" = "Geometry"}
		Cull Back
		ZWrite On
		ZTest LEqual
		
		Stencil
		{
			Ref 255
			Comp Always
			Pass Replace
		}
		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
			Blend Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragBase
			#pragma multi_compile_fwdbase
			ENDCG
		}


		USEPASS "Hidden/ShadowCaster/MAIN"

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#include "LightingInclude.cginc"
		#pragma shader_feature _SPECULAR
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _SCATTER
		#pragma shader_feature _SCATTER_DISTORT
		
		float4 _Color;
		sampler2D _Ramp;
		sampler2D _MainTex;
		sampler2D _AOTex;
		#if _NORMALMAP
		sampler2D _NormalTex;
		float _NormalStrength;
		#endif
		half4 _MainTex_ST;
		float4 _SpecularColor;
		float _SpecularRange;

		float _DiffuseCenter;
		float _DiffuseLightIntensity;
		float _DiffuseDarkIntensity;

		sampler2D _ScatterTex;
		float _ScatterOffset;
		float _ScatterDistort;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal:NORMAL;
			#if _NORMALMAP
			float4 tangent:TANGENT;
			#endif
			float2 uv : TEXCOORD0;
			float4 color:COLOR;
		};

		struct v2f
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 worldNormal:TEXCOORD1;
			float3 lightDir:TEXCOORD2;
			float3 viewDir:TEXCOORD3;
			float3 worldPos:TEXCOORD4;
			float4 color:TEXCOORD5;
			SHADOW_COORDS(6)
			#if _NORMALMAP
			float3x3 worldToTangent:TEXCOORD7;
			#endif
		};
		
		v2f vert (appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX( v.uv, _MainTex);
			o.worldNormal=normalize( mul((float3x3)unity_ObjectToWorld, v.normal));
			o.lightDir=WorldSpaceLightDir(v.vertex);
			o.viewDir=WorldSpaceViewDir(v.vertex);
			o.worldPos=mul(unity_ObjectToWorld,v.vertex);
			o.color=v.color;
			#if _NORMALMAP
			float3 worldTangent=mul(unity_ObjectToWorld,v.tangent);
			float3 worldBitangent=cross(worldTangent,o.worldNormal);
			o.worldToTangent=float3x3(normalize(worldTangent),normalize(worldBitangent),o.worldNormal);
			#endif
			TRANSFER_SHADOW(o);
			return o;
		}
		

		float4 fragBase(v2f i):SV_TARGET
		{
			float3 normal=normalize(i.worldNormal);
			float3 lightDir=normalize(i.lightDir);
			float3 viewDir = normalize(i.viewDir);
			float3 lightNormal=normal;
			UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos)
			#if _NORMALMAP
			float3 tangentSpaceNormal=tex2D(_NormalTex,i.uv)*2-1;
			lightNormal= lerp(normal, normalize( mul(tangentSpaceNormal,i.worldToTangent)),_NormalStrength);
			#endif
			float3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
			float3 lightMask=tex2D(_AOTex,i.uv).rgb;
			float4 vertexMask=i.color;
			atten= max(atten,vertexMask.a);
			float NDL =dot(lightNormal,lightDir);
			float diffuse=saturate(NDL)*atten;
			diffuse= diffuse>_DiffuseCenter?_DiffuseLightIntensity:_DiffuseDarkIntensity;
			diffuse*=lightMask.g;
			float3 diffuseCol = _LightColor0.rgb*diffuse+ambient;
			float3	startCol=tex2D(_MainTex, i.uv).rgb+_Color.rgb;
			startCol*=diffuseCol;
			#if _SPECULAR
			float specular =GetSpecular(lightNormal,lightDir,viewDir,_SpecularRange)*atten;
			specular+=lightMask.r*diffuse*atten;
			startCol += specular*_LightColor0.rgb*lightMask.r;
			#endif
			#if _SCATTER
			float normalizedNDL=NDL/2+_ScatterOffset;
			#if _SCATTER_DISTORT
			normalizedNDL-=_ScatterDistort*(1-vertexMask.b);
			#endif
			startCol= lerp(startCol, startCol*tex2D(_ScatterTex, normalizedNDL),diffuse);
			#endif

			return float4(startCol ,1);
		}
		ENDCG
	}
}
