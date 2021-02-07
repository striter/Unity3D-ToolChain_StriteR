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
			#include "../CommonInclude.cginc"
			#pragma shader_feature REPLACECOLOR

			sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			half4 _MainTex_TexelSize;
			half4 _OutlineColor;
			float _DepthBias;
			fixed _SampleDistance;

			#if REPLACECOLOR
			half4 _ReplaceColor;
			#endif

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
				o.uv[1] = uv + half2(1, 1)*_MainTex_TexelSize.xy*_SampleDistance;
				o.uv[2] = uv + half2(-1,-1)*_MainTex_TexelSize.xy*_SampleDistance;
				o.uv[3] = uv + half2(-1, 1)*_MainTex_TexelSize.xy*_SampleDistance;
				o.uv[4] = uv + half2(1, -1)*_MainTex_TexelSize.xy*_SampleDistance;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				half depthDetect =0;
				half originDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv[0]));
				for (int j = 1; j <= 4; j++)
				{
					half4 sampleDepth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv[j]));
					float depthOffset= (originDepth-sampleDepth);
					depthDetect+=step(_DepthBias,depthOffset);
				}
				depthDetect=saturate(depthDetect)*.25;
				#if REPLACECOLOR
				return lerp(_ReplaceColor,_OutlineColor,depthDetect);
				#else
				return lerp(tex2D(_MainTex,i.uv[0]),_OutlineColor,depthDetect);
				#endif
			}
			ENDCG
		}
	}
}
