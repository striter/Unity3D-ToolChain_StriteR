Shader "Hidden/PostEffect/PE_BloomSpecific_Render_Bloom"
{
	SubShader
	{
		Tags { "RenderType" = "BloomColor"  "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Effect/BloomSpecific/Base/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAdditive" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/Additive/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAlphaBlend" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/AlphaBlend/MAIN"
	}

	SubShader
	{
		Tags { "RenderType" = "BloomParticlesAdditiveNoiseFlow" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Particle/Additive_NoiseFlow/MAIN"
	}

	SubShader
	{
		Tags { "RenderType" = "BloomDissolveEdge" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv:TEXCOORD0;
			};

			float _DissolveAmount;
			float _DissolveWidth;
			float _DissolveScale;
			sampler2D _NoiseTex;
			v2f vert(appdata v)
			{
				v2f o;
				o.uv = float2(v.vertex.x, v.vertex.z) + v.vertex.y*.7;
				o.uv *= _DissolveScale;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed dissolve = tex2D(_NoiseTex,i.uv).r - _DissolveAmount - _DissolveWidth;
				clip(dissolve);

				return float4(0, 0, 0, 1);
			}
			ENDCG
		}

		UsePass "Game/Effect/BloomSpecific/Bloom_Dissolve/EDGE"
	}

	SubShader
	{
		Tags { "RenderType" = "BloomViewDirDraw" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/Effect/BloomSpecific/Color_ViewDirDraw/MAIN"
	}

		//Additional SubShader 
	SubShader
	{
		Tags{"RenderType" = "BloomMask" "IgnoreProjectile" = "true" "Queue" = "Transparent"}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 uv:TEXCOORD0;
			};

			float4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _SubTex1;
			float4 _SubTex1_ST;
			float _Amount1;
			float _Amount2;
			float _Amount3;

			v2f vert(appdata v)
			{
				v2f o;
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _SubTex1);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed dissolve = tex2D(_SubTex1,i.uv.zw).r - _Amount1 - _Amount2;
				clip(dissolve);

				float4 albedo = tex2D(_MainTex, i.uv.xy);
				if (albedo.a == 0)
					return fixed4(albedo.rgb* abs(sin(_Time.y*_Amount3)), 1);
				else if (albedo.a < 1)
					return fixed4(albedo.rgb, 1);

				return fixed4(0,0,0,1);
			}
			ENDCG
		}
	}
}
