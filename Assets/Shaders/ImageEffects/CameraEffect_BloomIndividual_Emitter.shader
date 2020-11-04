Shader "Hidden/CameraEffect_BloomReceiver_Emitter"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(0,0,0,1);
			}
			ENDCG
		}
	}
	Category
	{
		Cull Off Lighting Off Fog { Mode Off }
		Tags{"Ignore Projector"="True"}
		SubShader
		{
			Tags { "RenderType" = "BloomColor" "Queue" = "Geometry" }
			UsePass "Game/Effects/BloomEmitter/Color/MAIN"
		}

		SubShader
		{
			Tags{ "RenderType" = "BloomParticlesAdditive" "Queue" = "Transparent" }
			UsePass "Game/Effects/BloomEmitter/Particles/Additive/MAIN"
		}

		SubShader
		{
			Tags{ "RenderType" = "BloomParticlesAlphaBlend" "Queue" = "Transparent" }
			UsePass "Game/Effects/BloomEmitter/Particles/AlphaBlend/MAIN"
		}

		SubShader
		{
			Tags { "RenderType" = "BloomDissolveEdge" "Queue" = "Transparent" }
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
					o.uv = float2(v.vertex.x, v.vertex.z) + v.vertex.y * .7;
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

			UsePass "Game/Effects/BloomEmitter/Bloom_Dissolve/EDGE"
		}
	}
}
