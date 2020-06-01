Shader "Game/Effect/BloomSpecific/Color_ViewDirDraw"
{
	Properties
	{
	    _MainTex("Texture", 2D) = "white" {}
	    _Color("_Color",Color)=(1,1,1,1)
			_Amount1("_DrawAmount",Range(0,1))=.5
	}

	SubShader
	{ 
		Tags {"RenderType" = "BloomViewDirDraw" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		Cull Back Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
		Blend SrcAlpha One

		Cull Back
		Pass
		{
			name "MAIN"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile_instancing
			struct appdata
			{
				float4 vertex : POSITION;
				float4 color    : COLOR;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color    : TEXCOORD0;
				float2 uv:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			sampler2D _MainTex;
			float4 _MainTex_ST;
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
				UNITY_INSTANCING_BUFFER_END(Props)
			float _Amount1;
			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.vertex = UnityObjectToClipPos(v.vertex+viewDir*_Amount1);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 col = tex2D(_MainTex,i.uv)*UNITY_ACCESS_INSTANCED_PROP(Props, _Color)*i.color;
				return col;
			}
			ENDCG
		}
	}
}
