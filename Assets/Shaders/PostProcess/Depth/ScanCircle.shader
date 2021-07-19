Shader "Hidden/PostProcess/ScanCircle"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma shader_feature_local _MASK_TEXTURE
			#pragma vertex vert_img
			#pragma fragment frag

            #include "../../PostProcessInclude.hlsl"

			float4 _Color;
			float4 _Origin;

			float _MinSqrDistance;
			float _MaxSqrDistance;
			float _FadingPow;

			#if _MASK_TEXTURE
			TEXTURE2D( _MaskTexture);SAMPLER(sampler_MaskTexture);
			float _MaskTextureScale;
			#endif

			float4 frag (v2f_img i) : SV_Target
			{
				float3 worldPos = TransformNDCToWorld(i.uv);
				float squareDistance = sqrDistance(_Origin.xyz,worldPos);

				float scan = 1;
				scan *= _Color.a;
				scan *= pow(saturate(invlerp( _MinSqrDistance,_MaxSqrDistance,squareDistance)),_FadingPow)*step(squareDistance, _MaxSqrDistance);

				#if _MASK_TEXTURE
				scan *= SAMPLE_TEXTURE2D(_MaskTexture,sampler_MaskTexture, worldPos.xz*_MaskTextureScale).r;
				#endif
				float4 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
				return lerp(finalCol,finalCol*_Color, scan);
			}
			ENDHLSL
		}
	}
}
