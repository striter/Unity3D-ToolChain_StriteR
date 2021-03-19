Shader "Hidden/CameraEffect_DepthSSAO"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "CameraEffectInclude.hlsl"

				sampler2D _NoiseTex;
				float4 _SampleSphere[32];
				int _SampleCount;
				float _Intensity;
				float _DepthBias;
				float _DepthBiasMax;
				float4 _AOColor;
				float _NoiseScale;

				fixed4 frag (v2f_img i) : SV_Target
				{
					float3 normal = ClipSpaceNormalFromDepth( i.uv);
					float3 random = tex2D(_NoiseTex, i.uv* _NoiseScale).rgb;
					float2 uv = i.uv;
					float maxAODistance=100;
					float baseDepth = LinearEyeDepth(uv);
					float occlusion = 0;
					for (int i = 0; i < _SampleCount; i++) {
						float3 sampleOffsetRay =  _SampleSphere[i]*random;
						float2 occ_depth_uv = uv + sign(dot(sampleOffsetRay, normal)) * sampleOffsetRay.xy * _MainTex_TexelSize;
						float depthOffset = baseDepth-LinearEyeDepth(occ_depth_uv);
						occlusion+= step(_DepthBias,depthOffset)*step(depthOffset,_DepthBiasMax);
					}
					occlusion = 1-saturate(occlusion / _SampleCount * _Intensity);
					return lerp(_AOColor,tex2D(_MainTex, uv), occlusion);
				}
				ENDCG
		}
	}
}
