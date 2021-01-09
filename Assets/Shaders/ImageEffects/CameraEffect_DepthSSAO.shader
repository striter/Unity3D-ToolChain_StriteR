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

				sampler2D _MainTex;
				float4 _MainTex_TexelSize;
				sampler2D _NoiseTex;
				sampler2D _CameraDepthTexture;
				float4 _SampleSphere[32];
				int _SampleCount;
				float _Intensity;
				float _DepthBias;
				float4 _AOColor;
				float _NoiseScale;

				float GetDepth(float2 uv)		//0-1
				{
					float depth= SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
	#ifdef UNITY_REVERSED_Z 
					depth = 1 - depth;
	#endif
					return depth;
				}

				float3 normal_from_depth(float2 texcoords) {

					float2 offset1 = float2(0, 1)*_MainTex_TexelSize.xy;
					float2 offset2 = float2(1,0)*_MainTex_TexelSize.xy;

					float depth = GetDepth(texcoords);
					float depth1 = GetDepth(texcoords + offset1);
					float depth2 = GetDepth(texcoords + offset2);
				
					float3 p1 = float3(offset1, depth1 - depth);
					float3 p2 = float3(offset2, depth2 - depth);
					return normalize(cross(p1, p2));
				}

				fixed4 frag (v2f_img i) : SV_Target
				{
					float3 normal = normal_from_depth( i.uv);
					float3 random = tex2D(_NoiseTex, i.uv* _NoiseScale).rgb;
					float2 uv = i.uv;
					float baseDepth = GetDepth(uv);
					float occlusion = 0;

					float depthParam= saturate(LinearEyeDepth(1-baseDepth)/30);
					float depthBias=_DepthBias*_ProjectionParams.w*lerp(1,0.1, depthParam);
					float distance=1-depthParam;
					for (int i = 0; i < _SampleCount; i++) {
						float3 sampleOffsetRay =  _SampleSphere[i]*random*distance;
						float2 occ_depth_uv = uv + sign(dot(sampleOffsetRay, normal)) * sampleOffsetRay.xy * _MainTex_TexelSize;
						float depthOffset = baseDepth -GetDepth(occ_depth_uv) ;
						occlusion +=step(depthOffset, depthBias) * lerp(-1,1,  smoothstep(-depthBias, depthBias, depthOffset));
					}
					occlusion = saturate(occlusion / _SampleCount * _Intensity);
					return lerp(tex2D(_MainTex, uv), _AOColor, occlusion);
				}
				ENDCG
		}
	}
}
