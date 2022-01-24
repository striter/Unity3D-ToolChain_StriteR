Shader "Game/Particles/Distort"
{
	Properties
	{
		_DistortTex("DistortTex",2D) = "white"{}
		_DistortStrength("Distort Strength",Range(0,0.1))=.005
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent-1" "PreviewType"="Plane"}
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

		Pass
		{		
			name "Main"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos:TEXCOORD2;
			};
			sampler2D _CameraOpaqueTexture;
			sampler2D _DistortTex;
			float _DistortStrength;
			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.screenPos.xy / i.screenPos.w;
				fixed4 col = tex2D(_CameraOpaqueTexture,uv + tex2D(_DistortTex,uv) *_DistortStrength);
				return col;
			}
			ENDCG
		}
	}
}
