Shader "Hidden/PostProcess/VerticalFog"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_FogDensity("Fog Density",Float) = 1
		_FogColor("Fog Color",Color) = (1,1,1,1)
		_FogVerticalStart("Fog Start",Float) = 0
		_FogVerticalOffset("Fog Offset",Float) = 1
		[NoScaleOffset]_NoiseTex("Noise Tex",2D) = "white"{}
		_NoiseScale("Noise Scale",float)=.5
		_NoiseSpeedX("Fog Speed Horizontal",Range(-.5,.5)) = .5
		_NoiseSpeedY("Fog Speed Vertical",Range(-.5,.5)) = .5
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				HLSLPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma shader_feature_local _NOISE
				#include "../../PostProcessInclude.hlsl"
				half _FogDensity;
				float _FogPow;
				float4 _FogColor;
				float _FogVerticalStart;
				float _FogVerticalOffset;
				#if _NOISE
				sampler2D _NoiseTex;
				float _NoiseScale;
				float _NoiseSpeedX;
				float _NoiseSpeedY;
				#endif

				float4 frag (v2f_img i) : SV_Target
				{
					float linearDepth = LinearEyeDepthUV(i.uv);
					float3 worldPos = GetPositionWS(i.uv);
					float2 worldUV = (worldPos.xz + worldPos.yz);
					float fog =  (( _FogVerticalStart+_FogVerticalOffset)-worldPos.y)  /_FogVerticalOffset*_FogDensity;
					#if _NOISE
					float2 noiseUV = worldUV / _NoiseScale + _Time.y*float2(_NoiseSpeedX,_NoiseSpeedY);
					float noise = tex2D(_NoiseTex, noiseUV).r;
					fog*=noise;
					#endif
					return lerp(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv) , _FogColor, saturate(fog));
				}
				ENDHLSL
		}
	}
}
