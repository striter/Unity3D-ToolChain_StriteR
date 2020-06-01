Shader "Game/Effect/Holograph"
{
	Properties
	{
		_HolographTex("Holograph Tex",2D) = "white"{}
		_Color("Holograph Color",Color) = (1,1,1,1)
		_RGBCutout("RGB Cutout",Range(0,1)) = .2
		_VerticalFlowSpeed("Vertical Speed",Range(0,5)) = 1
	}
		SubShader
		{
			 Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
			 Blend SrcAlpha OneMinusSrcAlpha
			 ZWrite On
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _HolographTex;
			float4 _HolographTex_ST;
			float4 _Color;
			float _VerticalFlowSpeed;
			float _RGBCutout;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _HolographTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float alpha =tex2D(_HolographTex, i.uv+float2(0,_Time.x*_VerticalFlowSpeed)).g;
				fixed4 col = _Color;
				col.a = alpha>_RGBCutout?alpha:0;
				return col;
			}
			ENDCG
		}
	}
}
