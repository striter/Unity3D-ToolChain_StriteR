Shader "Hidden/PostEffect/PE_Bloom"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_Bloom("Bloom",2D)="white"{}
		_LuminanceThreshold("Luminance Threshold",Range(0,1)) = .5
		_LuminanceMultiple("Luminance Multiple",Range(1,20)) = 10
		_BlurSize("BlurSize",Float)=1
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		CGINCLUDE

	#include "UnityCG.cginc"

	fixed luminance(fixed4 color)
	{
	return 0.2125*color.r + 0.7154*color.g + 0.0721*color.b;
	}

	sampler2D _MainTex;
	half4 _MainTex_TexelSize;
	sampler2D _Bloom;
	half _BlurSize;
	half _LuminanceMultiple;
	half _LuminanceThreshold;


			v2f_img vertExtractBright(appdata_img v)
			{
				v2f_img o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			fixed4  fragExtractBright(v2f_img i):SV_TARGET
			{
				fixed4 c = tex2D(_MainTex,i.uv);
				fixed val = clamp((luminance(c) - _LuminanceThreshold)*_LuminanceMultiple, 0, 1);
				return c * val;
			}


			struct v2fBloom
			{
				float4 vertex:SV_POSITION;
				half4 uv:TEXCOORD0;
			};
			v2fBloom vertBloom(appdata_img v)
			{
				v2fBloom o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = v.texcoord;
				o.uv.zw = v.texcoord;

#if UNITY_UV_STARTS_AT_TOP
				if(_MainTex_TexelSize.y<0)
				o.uv.w = 1 - o.uv.w;
#endif
				return o;
			}

			fixed4 fragBloom(v2fBloom i):SV_TARGET
			{
			return tex2D(_MainTex,i.uv.xy)+tex2D(_Bloom,i.uv.zw);
			}
		ENDCG

		Pass
		{
			CGPROGRAM
			#pragma vertex vertExtractBright
			#pragma fragment fragExtractBright
			ENDCG
		}

			UsePass "Hidden/PostEffect/PE_Blurs/GAUSSIAN_BLUR_VERTICAL"
			UsePass "Hidden/PostEffect/PE_Blurs/GAUSSIAN_BLUR_HORIZONTAL"


			Pass
			{
			CGPROGRAM
#pragma vertex vertBloom
#pragma fragment fragBloom
			ENDCG
			}
	}
}
