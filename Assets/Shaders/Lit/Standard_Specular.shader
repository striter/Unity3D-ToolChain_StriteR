Shader "Game/Lit/Standard_Specular"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		_Lambert("Lambert",Range(0,1))=.5
		
		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back

		CGINCLUDE
		#include "../CommonLightingInclude.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#pragma multi_compile_instancing
		#pragma shader_feature _TRANSPARENT
		#pragma shader_feature _SPECULAR

		sampler2D _MainTex;
		float4 _MainTex_ST;
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
			TRANSFER_SHADOW(o);
			return o;
		}


		float4 DiffuseFragmentBase(v2fDV i) :SV_TARGET
		{
			UNITY_SETUP_INSTANCE_ID(i);
			float3 normal=normalize(i.worldNormal);
			float3 lightDir=normalize(i.worldLightDir);
			float3 viewDir=normalize(i.worldViewDir);
			UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
				
			float3 finalCol=tex2D(_MainTex, i.uv)*UNITY_ACCESS_INSTANCED_PROP(Props, _Color)+UNITY_LIGHTMODEL_AMBIENT.xyz;

			float diffuse=saturate( GetDiffuse(normal,lightDir));
			diffuse*=atten;
			diffuse = _Lambert + (1 - _Lambert)*diffuse;
			finalCol*=_LightColor0.rgb*diffuse;
				
			#if _SPECULAR
			float specular = GetSpecular(normal,lightDir,viewDir,_SpecularRange);
			specular*=atten;
			finalCol += _LightColor0.rgb*specular ;
			#endif

			return float4(finalCol,1);
		}

		float4 DiffuseFragmentAdd(v2fDV i) :SV_TARGET
		{
			UNITY_SETUP_INSTANCE_ID(i);
			UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)
			float diffuse=GetDiffuse(normalize(i.worldNormal),normalize(i.worldLightDir));
			return float4( _LightColor0.rgb*diffuse* atten, 1);
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
			ENDCG
		}

		Pass
		{
			Name "ForwardAdd"
			Tags{"LightMode" = "ForwardAdd"}
			Blend One One
			CGPROGRAM
			#pragma vertex DiffuseVertex
			#pragma fragment DiffuseFragmentAdd
			#pragma multi_compile_fwdadd
			ENDCG
		}

		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			CGPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
				
			struct v2fs
			{
				V2F_SHADOW_CASTER;
			};

			v2fs ShadowVertex(appdata_base v)
			{
				v2fs o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 ShadowFragment(v2fs i) :SV_TARGET
			{
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
	}
}