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
				HLSLPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "../CommonInclude.hlsl"
				#include "../CameraEffectInclude.hlsl"

				TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);
				TEXTURE2D(_CameraNormalTexture);SAMPLER(sampler_CameraNormalTexture);
				float3 _SampleSphere[32];
				int _SampleCount;
				float _Intensity;
				float _DepthBias;
				float _DepthBiasMax;
				float4 _AOColor;
				float _NoiseScale;

				float4 frag (v2f_img i) : SV_Target
				{
					float3 positionWS;
					float3 normalWS;
					WorldSpaceNormalFromDepth(i.uv,positionWS,normalWS);
					positionWS+=normalWS*.03;
					return float4(positionWS,1);
					float3 random = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex, i.uv* _NoiseScale).rgb;
					float2 uv = i.uv;
					float baseDepth = LinearEyeDepth(uv);
					float occlusion = 0;
					for (int i = 0; i < _SampleCount; i++) {
						float3 offsetWS= _SampleSphere[i];
						// offsetWS*=sign(dot(normalWS,offsetWS));
						float4 offsetCS=TransformWorldToHClip(offsetWS+positionWS);
						// offsetCS.xyz/=offsetCS.w;
						return offsetCS;

						offsetCS.xyz=offsetCS.xyz*.5+.5;
						float2 uvOffset=offsetCS.xy;
						float depthOffset = baseDepth-LinearEyeDepth(uvOffset);
						depthOffset=abs(depthOffset);
						occlusion+= smoothstep(0.,_DepthBiasMax,depthOffset);
					}
					occlusion = 1-saturate(occlusion / _SampleCount * _Intensity);
					return lerp(_AOColor,SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv), occlusion);
				}
				ENDHLSL
		}
	}
}
