Shader "Hidden/CameraEffect_DepthCircleScan"
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
			#pragma shader_feature _MASK_TEXTURE
			#pragma vertex vert
			#pragma fragment frag

			#include "../CommonInclude.hlsl"
            #include "../CameraEffectInclude.hlsl"

			float4 _Color;
			float4 _Origin;

			float _MinSqrDistance;
			float _MaxSqrDistance;
			float _FadingPow;

			#if _MASK_TEXTURE
			TEXTURE2D( _MaskTexture);SAMPLER(sampler_MaskTexture);
			float _MaskTextureScale;
			#endif

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
