Shader "Hidden/CameraEffect_DepthOutline"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			half4 _OutlineColor;
			float _DepthBias;
			fixed _SampleDistance;


			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv[5] : TEXCOORD0;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				half2 uv = v.texcoord;
				o.uv[0] = uv;
				#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
						uv.y = 1 - uv.y;
				#endif
				o.uv[1] = uv + _MainTex_TexelSize.xy*half2(1, 1)*_SampleDistance;
				o.uv[2] = uv + _MainTex_TexelSize.xy*half2(-1,-1)*_SampleDistance;
				o.uv[3] = uv + _MainTex_TexelSize.xy*half2(-1, 1)*_SampleDistance;
				o.uv[4] = uv + _MainTex_TexelSize.xy*half2(1, -1)*_SampleDistance;

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				half depthDetect =0;
				half originDepth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[0]));
				for (int j = 1; j <= 4; j++)
				{
					half4 sampleDepth=Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv[j]));
					depthDetect += (originDepth-sampleDepth)>_DepthBias?1:0;
				}

				return   lerp(tex2D(_MainTex, i.uv[0]), _OutlineColor , depthDetect );
			}
			ENDCG
		}
	}
}
