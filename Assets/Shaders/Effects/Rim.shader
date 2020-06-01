Shader "Game/Effect/Rim"
{
	Properties
	{
	   _InnerColor("Inner Color", Color) = (1,1,1,1)
	   _RimColor("Rim Color", Color) =(1,1,1,1)
	   _RimWidth("Rim Width", Range(0.2,20.0)) = 3.0
	   _RimGlow("Rim Glow Multiplier", Range(0.0,9.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend",Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend",Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		ZWrite Off
		Cull Back
		Blend [_SrcBlend] [_DstBlend]
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

	   float4 _InnerColor;
	   float4 _RimColor;
	   float _RimWidth;
	   float _RimGlow;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float rim : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.rim = pow(1-saturate(dot(normalize(ObjSpaceViewDir(v.vertex)),v.normal)),_RimWidth)*_RimGlow;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return lerp(_InnerColor,_RimColor, i.rim);
			}
			ENDCG
		}
	}
}
