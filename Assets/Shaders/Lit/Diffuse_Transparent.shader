Shader "Game/Lit/Diffuse_Transparent"
{
	Properties
	{
		_MainTex("Color UV TEX",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_Opacity("Opacity",Range(0,1)) = .7
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite On
		ZTest LEqual
		
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
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			CGPROGRAM

			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_shadowcaster
			ENDCG
		}

		CGINCLUDE
			#include "../CommonLightingInclude.cginc"
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			#pragma multi_compile_instancing

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Opacity;
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
				float3 objNormal:TEXCOORD2;
				float3 objViewDir : TEXCOORD3;
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
				o.objNormal=v.normal;
				o.objViewDir = ObjSpaceLightDir(v.vertex);  
				return o;
			}

			float4 DiffuseFragmentBase(v2fDV i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float diffuse=saturate( GetDiffuse(normalize( i.objNormal),normalize(i.objViewDir)));
				float3 finalCol=tex2D(_MainTex, i.uv)* UNITY_ACCESS_INSTANCED_PROP(Props, _Color)+UNITY_LIGHTMODEL_AMBIENT.xyz;
				finalCol*=diffuse*_LightColor0.rgb.rgb;
				return float4(finalCol,_Opacity);
			}

			
			sampler3D _DitherMaskLOD;
			struct v2fs
			{
				V2F_SHADOW_CASTER;
				float4 screenPos:TEXCOORD1;
			};

			v2fs ShadowVertex(appdata_base v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2fs o;
				o.screenPos = ComputeScreenPos(v.vertex);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
					return o;
			}

			fixed4 ShadowFragment(v2fs i) :SV_TARGET
			{
				float2 vpos = i.screenPos.xy / i.screenPos.w;
				float dither = tex3D(_DitherMaskLOD, float3(vpos * 10,_Opacity * 0.9375)).a;
				clip(dither - 0.01);
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
	}
}
