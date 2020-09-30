Shader "Hidden/CameraEffect_BloomReceiver_Emitter"
{
	SubShader
	{
		Tags { "RenderType" = "BloomColor"  "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/BloomEmitter/Base/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAdditive" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/BloomEmmitter/Particle/Additive/MAIN"
	}

	SubShader
	{
		Tags{ "RenderType" = "BloomParticlesAlphaBlend" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		UsePass "Game/BloomEmmitter/Particle/AlphaBlend/MAIN"
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

		UsePass "Game/BloomEmitter/Bloom_Dissolve/EDGE"
	}
}
