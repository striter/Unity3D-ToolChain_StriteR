Shader "Game/Particles/AlphaBlend"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		[HDR]_Color("Color",Color) = (1,1,1,1)
		
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" "PreviewType" = "Plane"} 
		Blend SrcAlpha OneMinusSrcAlpha
		Cull [_Cull]
		ZTest [_ZTest]
		Lighting Off 
		ZWrite Off
		
		Pass
		{		
			name "Main"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile_instancing
			struct appdata
			{
				float4 vertex : POSITION;
				float4 color:COLOR;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color:TEXCOORD0;
				float2 uv:TEXCOORD1;
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex,i.uv)*_Color*i.color;
				return col;
			}
			ENDCG
		}
	}
}
