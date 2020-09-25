Shader "Hidden/PostEffect/PE_FogDepthNoise"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_FogDensity("Fog Density",Float) = 1
		_FogColor("Fog Color",Color) = (1,1,1,1)
		_FogStart("Fog Start",Float) = 0
		_FogEnd("Fog End",Float) = 1
		_NoiseTex("Noise Tex",2D) = "white"{}
		_NoisePow("Noise Pow",Float) = 1
		_NoiseLambert("Noise Lambert",Range(0,1)) = 0
		_FogSpeedX("Fog Speed Horizontal",Range(-.5,.5)) = .5
		_FogSpeedY("Fog Speed Vertical",Range(-.5,.5)) = .5
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "PostEffectInclude.cginc"
				half _FogDensity;
				float _FogPow;
				fixed4 _FogColor;
				float _FogStart;
				float _FogEnd;
				sampler2D _NoiseTex;
				float _NoisePow;
				float _NoiseLambert;
				float _FogSpeedX;
				float _FogSpeedY;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float2 uv_depth:TEXCOORD1;
				float4 interpolatedRay:TEXCOORD2;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv_depth = GetDepthUV(v.texcoord); 
				o.interpolatedRay = GetInterpolatedRay(o.uv);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth));
				float3 worldPos = _WorldSpaceCameraPos+ i.interpolatedRay.xyz*linearDepth;
				float2 worldUV = (worldPos.xz + worldPos.y);
				float2 noiseUV1 = worldUV / 15 + _Time.y*float2(_FogSpeedX, 0);
				float2 noiseUV2 = worldUV / 15 + _Time.y*float2(0, _FogSpeedY);
				float noise = (tex2D(_NoiseTex, noiseUV1).r)*(tex2D(_NoiseTex, noiseUV2).r);
				float fogDensity = linearDepth == 1 ? 0:(_FogEnd - worldPos.y) /(_FogEnd - _FogStart)*_FogDensity*noise;
				return fixed4(tex2D(_MainTex, i.uv).rgb + _FogColor.rgb*fogDensity,1);
			}
			ENDCG
		}
	}
}
