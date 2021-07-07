Shader "Hidden/PostProcess/SSAO"
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
				#pragma shader_feature_local _DITHER

				#include "../../PostProcessInclude.hlsl"

				TEXTURE2D(_NoiseTex);SAMPLER(sampler_NoiseTex);
				TEXTURE2D(_CameraNormalTexture);SAMPLER(sampler_CameraNormalTexture);
				float3 _SampleSphere[32];
				uint _SampleCount;
				float _Intensity;
				float _Radius;
				float _Bias;
				float4 _AOColor;
				float _NoiseScale;
				
				float4 frag (v2f_img i) : SV_Target
				{
					float depth = 0;
					float3 positionWS;
					float3 normalWS=WorldSpaceNormalFromDepth(i.uv,positionWS,depth);
					float occlusion = 0;
					float radius=_Radius;
					#if _DITHER
					radius*=remap(random01(i.uv),0.,1.,0.5,1.);
					#endif
					[unroll(128u)]
					for (uint index = 0u; index < _SampleCount; index++) {
						float3 offsetWS= _SampleSphere[index]*radius;
						offsetWS*=sign(dot(normalWS,offsetWS));
						float3 sampleWS=positionWS+offsetWS;
						half2 sampleUV;
						half sampleDepth;
						TransformHClipToUVDepth(mul(_Matrix_VP, float4(sampleWS,1)),sampleUV,sampleDepth);
						float depthOffset =  TransformWorldToLinearEyeDepth(sampleWS,_Matrix_V)-LinearEyeDepthUV(sampleUV);
						float depthSample=saturate(depthOffset/_Radius)*step(depthOffset,_Bias);
						occlusion+=depthSample;
					}
					occlusion/=_SampleCount;
					occlusion = saturate(occlusion  * _Intensity);
					occlusion*=step(HALF_MIN,abs(depth-Z_END));		//Clip Skybox
					return lerp(Sample_MainTex(i.uv),_AOColor, occlusion);
				}
				ENDHLSL
		}
	}
}
