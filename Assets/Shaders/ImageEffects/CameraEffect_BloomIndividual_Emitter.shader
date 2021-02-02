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
			#pragma multi_compile _ _BLOOMINDIVIDUAL_ADDITIVE _BLOOMINDIVIDUAL_ALPHABLEND
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
				#if _BLOOMINDIVIDUAL_ALPHABLEND
					return float4(0,0,0,0);
				#endif
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
			Tags { "RenderType" = "Dissolve"}
			UsePass "Game/Effects/Dissolve/EDGE"
		}

		SubShader
		{
			Tags {"RenderType" = "Disintegrate"}
			UsePass "Game/Effects/Geometry/Disintegrate/DISINTEGRATE"
		}

		SubShader
		{
			Tags{"RenderType"="GeometryAdditive"}
			UsePass "Game/Effects/GeometryAdditive/MAIN"
		}

		SubShader
		{
			Tags{"RenderType"="EnergyShield"}
			UsePass "Game/Effects/EnergyShield/MAIN"
		}

		SubShader
		{
			Tags {"RenderType"="HDREmitter"}
			UsePass "Game/Unlit/Transparent/MAIN"
		}
	}
}
