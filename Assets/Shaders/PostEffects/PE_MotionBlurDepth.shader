Shader "Hidden/PostEffect/PE_MotionBlurDepth"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_BlurSize("Blur Size",Range(0,1))=1
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

			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			sampler2D _CameraDepthTexture;
			float4x4 _CurrentVPMatrixInverse;
			float4x4 _PreviousVPMatrix;
			fixed _BlurSize;
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_depth:TEXCOORD1;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv_depth = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv_depth.y = 1 - o.uv_depth.y;
#endif

				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth);
				float4 currentPos = float4(i.uv.x*2-1,i.uv.y*2-1,d*2-1,1);
				float4 worldPos = mul(_CurrentVPMatrixInverse, currentPos);
			    worldPos /= worldPos.w;
				float4 previousPos = mul(_PreviousVPMatrix, worldPos);
				previousPos /= previousPos.w;

				float2 velocity = (currentPos.xy - previousPos.xy) / 2;
				float2 uv = i.uv;
				fixed4 col=fixed4(0,0,0,0);
				for (int it = 0; it < 3; it++)
				{
					col += tex2D(_MainTex, uv+ velocity * _BlurSize*it);
				}
				col /= 3;

				return col;
			}
			ENDCG
		}
	}
}
