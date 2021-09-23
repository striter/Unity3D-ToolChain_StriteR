Shader "Game/Particles/Distort_CenterAbsorb"
{
	Properties
	{
		_DistortStrength("Distort Strength",Range(0,0.1))=.005
		_DistortSpeed("Distort Speed",float)=1
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent-1" "PreviewType"="Plane"}
		Cull Back Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

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
				float4 screenPos:TEXCOORD0;
				float4 centerScreenPos:TEXCOORD1;
			};
			sampler2D _CameraOpaqueTexture;
			float _DistortStrength;
			float _DistortSpeed;
			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				o.centerScreenPos = ComputeScreenPos(UnityObjectToClipPos(float4(0,0,0,0)));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 screenUV = i.screenPos.xy / i.screenPos.w;
				float2 centerScreenUV = i.centerScreenPos.xy / i.centerScreenPos.w;
				float2 offsetDirection = screenUV - centerScreenUV;
				float absorb = normalize(offsetDirection) * (1 - length(offsetDirection));
				float distort = abs( sin( _Time.y* _DistortSpeed %3.14));
				screenUV += absorb * _DistortStrength *distort;

				fixed4 col = tex2D(_CameraOpaqueTexture,screenUV);
				return col;
			}
			ENDCG
		}
	}
}
