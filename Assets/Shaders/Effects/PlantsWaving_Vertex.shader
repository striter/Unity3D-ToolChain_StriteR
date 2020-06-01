// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Game/Effect/PlantsWaving_Vertex"
{
	Properties
	{
		_MainTex("Color UV TEX",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_WaveSpeed("Wind Speed",Range(0,5)) = 1
		_WaveDirection("Wave: XYZ|Direction W|Frequency",Vector)=(.1,0,.1,0)
		_WaveParam("Y Param: X|Clip Y|Start Z|Multiply",Vector)=(0,0,1,0)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" "Queue"="AlphaTest+1" }

			CGINCLUDE
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"
			float4 _WaveDirection;
			float _WaveSpeed;
			float4 _WaveParam;

			float3 Wave(float3 worldPos)
			{
				float wave = sin(_Time.y*_WaveSpeed + (worldPos.x + worldPos.y)*_WaveDirection.w) / 100;
				float yMultiple = max(0,worldPos.y%_WaveParam.x- _WaveParam.y)*_WaveParam.z;
				return  _WaveDirection.xyz*wave*yMultiple;
			}
				ENDCG

			Pass		//Base Pass
			{
				Tags{ "LightMode" = "ForwardBase"}
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
					float3 worldPos:TEXCOORD1;
					float diffuse : TEXCOORD2;
					SHADOW_COORDS(3)
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;
				v2f vert(appdata v)
				{
					v2f o;
					o.uv = v.uv;
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.worldPos +=Wave(o.worldPos);
					o.pos = UnityWorldToClipPos(o.worldPos);
					o.diffuse = saturate(dot(v.normal,ObjSpaceLightDir(v.vertex)));
					TRANSFER_SHADOW(o);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float3 albedo = tex2D(_MainTex,i.uv)*_Color;
					UNITY_LIGHT_ATTENUATION(atten, i,i.worldPos)
					fixed3 ambient = albedo * UNITY_LIGHTMODEL_AMBIENT.xyz;
					atten = atten * .8 + .2;		//inspired by half lambert
					float3 diffuse = albedo * _LightColor0.rgb*i.diffuse*atten;
					return fixed4(ambient + diffuse	,1);
				}
				ENDCG
			}
			
			Pass
			{
				Tags{"LightMode" = "ShadowCaster"}
				CGPROGRAM
				#pragma vertex vertshadow
				#pragma fragment fragshadow
				#pragma multi_compile_shadowcaster

				struct v2fs
				{
				V2F_SHADOW_CASTER;
				};

			v2fs vertshadow(appdata_base v)
			{
				v2fs o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				worldPos += Wave(worldPos);
				o.pos = UnityWorldToClipPos(worldPos);
				return o;
			}

			fixed4 fragshadow(v2fs i) :SV_TARGET
			{
				SHADOW_CASTER_FRAGMENT(i);
			}
				ENDCG
			}
		}
}
