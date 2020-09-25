Shader "Hidden/PostEffect/PE_DepthSSAO"
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
			float _Strength;
			float _FallOff;
			float4 _AOColor;

			float Get01Depth(float2 uv)
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

				float depth = Get01Depth(texcoords);
				float depth1 = Get01Depth(texcoords + offset1);
				float depth2 = Get01Depth(texcoords + offset2);
				
				float3 p1 = float3(offset1, depth1 - depth);
				float3 p2 = float3(offset2, depth2 - depth);
				return normalize(cross(p1, p2));
			}

			fixed4 frag (v2f_img i) : SV_Target
			{
				float3 normal = normal_from_depth( i.uv);
				float3 random = tex2D(_NoiseTex, i.uv*10).rgb;
				float2 uv = i.uv;
				float baseDepth = Get01Depth(uv);
				float occlusion = 0;
				for (int i = 0; i < _SampleCount; i++) {
					float3 sampleOffsetRay =  _SampleSphere[i]*random;
					float2 occ_depth_uv = saturate(uv + sign(dot(sampleOffsetRay, normal)) * sampleOffsetRay.xy * _MainTex_TexelSize);
					float depthOffset = baseDepth -Get01Depth(occ_depth_uv) ;
					occlusion +=step(depthOffset, _FallOff) *lerp(-1,1,  smoothstep(-_FallOff, _FallOff, depthOffset));
				}
				occlusion = saturate(occlusion / _SampleCount * _Strength);
				return lerp(tex2D(_MainTex, uv), _AOColor, occlusion);
			}
			ENDCG
		}
	}
}
