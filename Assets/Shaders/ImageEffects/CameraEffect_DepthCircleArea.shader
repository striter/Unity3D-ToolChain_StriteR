Shader "Hidden/CameraEffect_DepthCircleArea"
{
	Properties
	{
		[PreRenderData]_MainTex("Texture", 2D) = "white" {}
		_FillColor("_Fill Color",Color)=(1,1,1,1)
		_FillTexture("_Fill Texture",2D)="White"{}
		_TextureScale("_Texture Scale",float)=1
		_TextureFlow("_Texture Flow",Vector)=(1,1,0,0)
		_SqrEdgeMin("Square Edge Min",float)=100
		_SqrEdgeMax("Square Edge Max",float)=144
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
					float2 uv = worldPos.xz + worldPos.yy;
					float squaredDistance = sqrdistance(worldPos,_Origin);

					float fill = step(squaredDistance,_SqrEdgeMax);
					float edge = fill * step(_SqrEdgeMin,squaredDistance);

					fill *= _FillColor.a;
					edge *= _EdgeColor.a;

					float4 fillColor = tex2D(_FillTexture, uv * _TextureScale + _TextureFlow.xy * (_Time.y))*_FillColor;
					float4 edgeColor = _EdgeColor;

					return  lerp( tex2D(_MainTex, i.uv),lerp(fillColor, edgeColor,edge), fill);
				}
			ENDCG
		}
	}
}
