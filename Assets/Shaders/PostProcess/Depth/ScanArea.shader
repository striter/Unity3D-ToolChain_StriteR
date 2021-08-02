Shader "Hidden/PostProcess/ScanArea"
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
				#pragma vertex vert_img
				#pragma fragment frag

				#include "Assets/Shaders/Library/PostProcessInclude.hlsl"
				
				TEXTURE2D(_FillTexture);SAMPLER(sampler_FillTexture);
				float4 _Origin;
				float4 _FillColor;
				float _TextureScale;
				float4 _TextureFlow;
				float4 _EdgeColor;
				float _SqrEdgeMin;
				float _SqrEdgeMax;


				float4 frag (v2f_img i) : SV_Target
				{
					float3 worldPos =TransformNDCToWorld(i.uv);
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
