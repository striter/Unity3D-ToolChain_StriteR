Shader "Hidden/CameraEffect_DepthCircleArea"
{
	Properties
	{
		[PreRenderData]_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "../CommonInclude.hlsl"
				#include "CameraEffectInclude.hlsl"
				
				TEXTURE2D(_FillTexture);SAMPLER(sampler_FillTexture);
				float4 _Origin;
				float4 _FillColor;
				float _TextureScale;
				float4 _TextureFlow;
				float4 _EdgeColor;
				float _SqrEdgeMin;
				float _SqrEdgeMax;

				struct v2f
				{
					float4 positionCS : SV_POSITION;
					float2 uv : TEXCOORD0;
					half2 uv_depth:TEXCOORD1;
					float3 interpolatedRay:TEXCOORD2;
				};

				v2f vert (a2v_img v)
				{
					v2f o;
					o.positionCS = TransformObjectToHClip(v.positionOS);
					o.uv = v.uv;
					o.uv_depth = GetDepthUV(v.uv);
					o.interpolatedRay = GetInterpolatedRay(o.uv);
					return o;
				}

				float4 frag (v2f i) : SV_Target
				{
					float linearDepth = LinearEyeDepth(i.uv_depth);
					float3 worldPos = _WorldSpaceCameraPos + i.interpolatedRay*linearDepth;
					float squaredDistance = sqrDistance(worldPos,_Origin.xyz);

					float fill = step(squaredDistance,_SqrEdgeMin);
					float edge = saturate( invlerp(_SqrEdgeMax,_SqrEdgeMin,squaredDistance))*(1-fill);
					
					float2 uv = (worldPos.xz-_Origin.xz)* _TextureScale + _TextureFlow.xy * _Time.y;
					float fillMask=SAMPLE_TEXTURE2D(_FillTexture,sampler_FillTexture, uv ).r;
					float3 fillColor = fillMask*_FillColor.rgb;
					float3 edgeColor = _EdgeColor.rgb;

					float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;
					finalCol=lerp(finalCol,fillColor,fill*_FillColor.a);
					finalCol=lerp(finalCol,edgeColor,edge*_EdgeColor.a);

					return float4( finalCol,1);
				}
			ENDHLSL
		}
	}
}
