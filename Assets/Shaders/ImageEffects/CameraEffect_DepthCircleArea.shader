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
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "../CommonInclude.cginc"
				#include "CameraEffectInclude.cginc"

				float4 _Origin;
				float4 _FillColor;
				sampler2D _FillTexture;
				float _TextureScale;
				float4 _TextureFlow;
				float4 _EdgeColor;
				float _SqrEdgeMin;
				float _SqrEdgeMax;

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
					float squaredDistance = sqrDistance(worldPos,_Origin);

					float fill = step(squaredDistance,_SqrEdgeMin);
					float edge = saturate( invlerp(_SqrEdgeMax,_SqrEdgeMin,squaredDistance))*(1-fill);
					
					float2 uv = (worldPos.xz-_Origin.xz)* _TextureScale + _TextureFlow.xy * _Time.y;
					float fillMask=tex2D(_FillTexture, uv ).r;
					float3 fillColor = fillMask*_FillColor;
					float3 edgeColor = _EdgeColor;

					float3 finalCol=tex2D(_MainTex, i.uv);
					finalCol=lerp(finalCol,fillColor,fill*_FillColor.a);
					finalCol=lerp(finalCol,edgeColor,edge*_EdgeColor.a);

					return float4( finalCol,1);
				}
			ENDCG
		}
	}
}
