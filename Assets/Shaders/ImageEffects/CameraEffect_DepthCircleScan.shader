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
			CGPROGRAM
			#pragma shader_feature _MASK_TEXTURE
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "../CommonInclude.cginc"
			#include "CameraEffectInclude.cginc"

			float4 _Color;
			float4 _Origin;

			float _MinSqrDistance;
			float _MaxSqrDistance;
			float _FadingPow;

			#if _MASK_TEXTURE
			sampler2D _MaskTexture;
			float _MaskTextureScale;
			#endif

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half2 uv_depth:TEXCOORD1;
				float3 interpolatedRay:TEXCOORD2;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv_depth = GetDepthUV(v.texcoord);
				o.interpolatedRay = GetInterpolatedRay(o.uv);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth));
				float3 worldPos = _WorldSpaceCameraPos + i.interpolatedRay*linearDepth;
				float squareDistance = sqrDistance(_Origin.xyz,worldPos);

				float scan = 1;
				scan *= _Color.a;
				scan *= pow(saturate(invlerp( _MinSqrDistance,_MaxSqrDistance,squareDistance)),_FadingPow)*step(squareDistance, _MaxSqrDistance);

				#if _MASK_TEXTURE
				scan *= tex2D(_MaskTexture, worldPos.xz*_MaskTextureScale).r;
				#endif
				float4 finalCol=tex2D(_MainTex,i.uv);
				return lerp(finalCol,finalCol*_Color, scan);
			}
			ENDCG
		}
	}
}
