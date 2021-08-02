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

				#include "Assets/Shaders/Library/PostProcessInclude.hlsl"

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
						radius*=remap(dither01(i.uv*_ScreenParams.xy),0.,1.,0.5,1.);
					#endif
					float rcpRadius=rcp(_Radius);
					[unroll(128u)]
					for (uint index = 0u; index < _SampleCount; index++) {
						float3 offsetWS= _SampleSphere[index]*radius;
						offsetWS*=sign(dot(normalWS,offsetWS));
						float3 sampleWS=positionWS+offsetWS;
						float2 sampleUV;
						half sampleDepth;
						TransformHClipToUVDepth(mul(_Matrix_VP, float4(sampleWS,1)),sampleUV,sampleDepth);
						float depthOffset =  TransformWorldToEyeDepth(sampleWS,_Matrix_V)-SampleEyeDepth(sampleUV);
						float depthSample=saturate(depthOffset*rcpRadius)*step(depthOffset,_Bias);
						occlusion+=depthSample;
					}
					occlusion*=rcp(_SampleCount);
					occlusion = saturate(occlusion  * _Intensity);
					occlusion*=step(HALF_MIN,abs(depth-Z_END));		//Clip Skybox
					return lerp(SampleMainTex(i.uv),_AOColor, occlusion);
				}
				ENDHLSL
		}
	}
}
