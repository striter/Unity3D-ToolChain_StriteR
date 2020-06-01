
Shader "Game/Special/Projector" {
	Properties{
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_ColorStrength("Color strength", Float) = 1.0
		_MainTex("Base (RGB) Gloss (A)", 2D) = "black" {}
	}

		Category{

			Tags { "Queue" = "Transparent"  "IgnoreProjector" = "True"  "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			Fog { Mode Off}


			SubShader {
				Pass {
					Name "BASE"
					Tags { "LightMode" = "Always" }

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			float3 normal:NORMAL;
			float2 texcoord: TEXCOORD0;
			fixed4 color : COLOR;
		};

		struct v2f {
			float4 vertex : POSITION;
			float4 uvFalloff : TEXCOORD0;
			float4 uvMainTex : TEXCOORD1;
			float2 texcoord : TEXCOORD2;
		};

		float4x4 unity_Projector;
		float4x4 unity_ProjectorClip;
		sampler2D _MainTex;
		float _ColorStrength;
		fixed4 _TintColor;
		float4 _LightColor0;
		float4 _MainTex_ST;

			v2f vert(appdata_t v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex+v.normal*0.001);
				o.uvMainTex = mul(unity_Projector, v.vertex);
				o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
				o.texcoord = TRANSFORM_TEX(o.uvMainTex.xyz,_MainTex);

				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				fixed2 projCoord = UNITY_PROJ_COORD(i.uvFalloff);
				fixed4 tex;
				tex = tex2D(_MainTex, saturate(i.texcoord.xy));

				fixed4 res = lerp(0, tex * _TintColor * _ColorStrength, projCoord.x<1&&projCoord.x>0?1:0);
				return res;
			}
		ENDCG
				}
			}
	}

}
