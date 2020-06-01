Shader "Hidden/PostEffect/PE_Blurs"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_BlurSize("Blur Size",Float)=1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

	CGINCLUDE

			#include "UnityCG.cginc"
		sampler2D _MainTex;
		half4 _MainTex_TexelSize;
		fixed _BlurSize;

		struct v2f
		{
			half2 uv[5]: TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		fixed4 fragBlur(v2f i) :SV_TARGET
		{
		float weight[3] = {0.4026,0.2442,0.0545};

		float3 sum = tex2D(_MainTex,i.uv[0]).rgb*weight[0];
		for (int it = 1; it < 3; it++)
		{
			sum += tex2D(_MainTex, i.uv[it * 2 - 1]).rgb*weight[it];
			sum += tex2D(_MainTex, i.uv[it * 2]).rgb*weight[it];
		}
		return fixed4(sum, 1);
		}
			ENDCG

		Pass
		{
			NAME "AVERAGE_BLUR"
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			v2f vert(appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 uv = v.texcoord;
				o.uv[0] = uv;
				o.uv[1] = uv + float2(1, 1)*_MainTex_TexelSize.x *_BlurSize;
				o.uv[2] = uv + float2(1, -1)*_MainTex_TexelSize.x *_BlurSize;
				o.uv[3] = uv + float2(-1, -1)*_MainTex_TexelSize.x *_BlurSize;
				o.uv[4] = uv + float2(-1, 1)*_MainTex_TexelSize.x *_BlurSize;
				return o;
			}

			float4 frag(v2f i):SV_TARGET
			{
				fixed4 sum = tex2D(_MainTex, i.uv[0]);
				sum += tex2D(_MainTex, i.uv[1]);
				sum += tex2D(_MainTex, i.uv[2]);
				sum += tex2D(_MainTex, i.uv[3]);
				sum += tex2D(_MainTex, i.uv[4]);
				return sum/5;
			}
			ENDCG
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_HORIZONTAL"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragBlur
				v2f vert(appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				half2 uv = v.texcoord;
				o.uv[0] = uv;
				o.uv[1] = uv + float2(_MainTex_TexelSize.x * 1, 0)*_BlurSize;
				o.uv[2] = uv - float2(_MainTex_TexelSize.x * 1, 0)*_BlurSize;
				o.uv[3] = uv + float2(_MainTex_TexelSize.x * 2, 0)*_BlurSize;
				o.uv[4] = uv - float2(_MainTex_TexelSize.x * 2, 0)*_BlurSize;
				return o;
			}
			ENDCG
		}

		Pass		//Vert Blur
		{
			NAME "GAUSSIAN_BLUR_VERTICAL"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragBlur

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				half2 uv = v.texcoord;
				o.uv[0]=uv;
				o.uv[1] = uv + float2(0, _MainTex_TexelSize.y * 1)*_BlurSize;
				o.uv[2] = uv - float2(0, _MainTex_TexelSize.y * 1)*_BlurSize;
				o.uv[3] = uv + float2(0, _MainTex_TexelSize.y * 2)*_BlurSize;
				o.uv[4] = uv - float2(0, _MainTex_TexelSize.y * 2)*_BlurSize;
				return o;
			}
	
			ENDCG
		}

	}
}
