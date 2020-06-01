Shader "Game/Particle/Additive_NoiseFlow"
{
	Properties
	{
	    _MainTex("Main Texture", 2D) = "white" {}
	    _Color("_Color",Color)=(1,1,1,1)
		_SubTex1("Detail Tex",2D) = "white"{}
		_FlowSpeed("Flow Speed(XY Main,ZW Noise)",Vector)=(0,0,0,0)
		_Emmission("Emission",Range(0,5))=1
	}

	SubShader
	{ 
		Tags {"RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }

		Blend SrcAlpha One
		Pass
		{
			name "Main"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv:TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color    : TEXCOORD0;
				float4 uv:TEXCOORD1;
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _SubTex1;
			float4 _SubTex1_ST;
			float4 _FlowSpeed;
			float _Emmission;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _SubTex1);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex,i.uv.xy+ _FlowSpeed.xy*_Time.y)*_Color*i.color*_Emmission;
				fixed4 colDetail = tex2D(_SubTex1, i.uv.zw+ _FlowSpeed.xw*_Time.y);
				return col*colDetail;
			}
			ENDCG
		}
	}
}
