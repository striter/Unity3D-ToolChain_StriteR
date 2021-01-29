Shader "Game/Effects/Dissolve"
{
	Properties
	{
		_DissolveAmount("_Dissolve Amount",Range(0,1)) = 1
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		_DissolveTex("Dissolve Map",2D) = "white"{}
		_DissolveWidth("_Dissolve Width",Range(0,1)) = .1
		[HDR]_DissolveColor("_Dissolve Color",Color) = (1,1,1,1)

	}
	SubShader
	{
		Tags{"RenderType" = "Dissolve"  "Queue" = "Geometry"}
		Cull Off

		CGINCLUDE
		#include "../CommonLightingInclude.cginc"
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"
		#include "Lighting.cginc"
		sampler2D _DissolveTex;
		float4 _DissolveTex_ST;
		float _DissolveAmount;
		float _DissolveWidth;
		float4 _DissolveColor;
		sampler2D _MainTex;
		float4 _MainTex_ST;
		ENDCG

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase


			struct a2fDV
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
				float3 normal:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv:TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float3 objNormal:TEXCOORD2;
				float3 objLightDir:TEXCOORD3;
				SHADOW_COORDS(4)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert (a2fDV v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv.xy =  TRANSFORM_TEX( v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv.zw = TRANSFORM_TEX(v.vertex.xz,_DissolveTex);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.objNormal=v.normal;
				o.objLightDir=ObjSpaceLightDir(v.vertex);
				TRANSFER_SHADOW(o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed dissolve = tex2D(_DissolveTex,i.uv.zw).r - _DissolveAmount-_DissolveWidth;
				clip(dissolve);

				float diffuse=GetDiffuse(normalize(i.objNormal),normalize(i.objLightDir));
				float4 albedo = tex2D(_MainTex,i.uv.xy)* _Color;
				UNITY_LIGHT_ATTENUATION(atten, i,i.worldPos)
				float3 finalCol=tex2D(_MainTex, i.uv)* _Color+(UNITY_LIGHTMODEL_AMBIENT.xyz);
				finalCol*=diffuse*atten;
				finalCol*=_LightColor0.rgb;
				return float4(finalCol,1);
			}
			ENDCG
		}

		Pass
		{
			Tags{"LightMode" = "ShadowCaster"}
			CGPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow

			struct v2fs
			{
				V2F_SHADOW_CASTER;
				float2 uv:TEXCOORD0;
			};
			v2fs vertshadow(appdata_base v)
			{
				v2fs o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				o.uv = TRANSFORM_TEX(v.vertex.xz,_DissolveTex);
				return o;
			}

			fixed4 fragshadow(v2fs i) :SV_TARGET
			{
				fixed dissolve = tex2D(_DissolveTex,i.uv).r - _DissolveAmount-_DissolveWidth;
				clip(dissolve);
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}


		Pass
		{
			NAME "EDGE"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.vertex.xz,_DissolveTex);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed dissolve = tex2D(_DissolveTex,i.uv).r - _DissolveAmount;
				clip(step(0,dissolve)*step( dissolve,_DissolveWidth )-0.01);
				return _DissolveColor;
			}
			ENDCG
		}

	}
}
