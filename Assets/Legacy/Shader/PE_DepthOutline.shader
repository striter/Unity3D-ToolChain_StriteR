Shader "Hidden/PostEffect/PE_DepthOutline"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_EdgeColor("Edge Color",Color) = (1,1,1,1)
		_DepthBias("Depth Bias",Range(0,0.01))=.001
		_SampleDistance("Sample Distance",Float) = 1
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

			sampler2D _CameraDepthNormalsTexture;
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			fixed4 _EdgeColor;
			float _DepthBias;
			fixed _SampleDistance;


			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv[5] : TEXCOORD0;
			};

			half CheckSame(half4 center, half4 sample)
			{
				float diff = 0;

				half2 centerNormal = center.xy;
				half sampleDepth = DecodeFloatRG(sample.zw);


				half2 sampleNormal = sample.xy;
				half centerDepth = DecodeFloatRG(center.zw);

				half2 diffNormal = abs(centerNormal - sampleNormal);
				half diffDepth = abs(centerDepth - sampleDepth);

				bool isSameNormal = (diffNormal.x + diffNormal.y) < .1;

				return  isSameNormal?0:(diffDepth> _DepthBias ?1:0);
			}

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
				float4 origin = tex2D(_CameraDepthNormalsTexture, i.uv[0]);
				for (int j = 1; j <= 4; j++)
					depthDetect += CheckSame(origin, tex2D(_CameraDepthNormalsTexture, i.uv[j]));

				return   lerp(tex2D(_MainTex, i.uv[0]), _EdgeColor , depthDetect );
			}
			ENDCG
		}
	}
}
