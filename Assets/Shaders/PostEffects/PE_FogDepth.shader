Shader "Hidden/PostEffect/PE_FogDepth"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_FogDensity("Fog Density",Float) = 1
		_FogColor("Fog Color",Color) = (1,1,1,1)
		_FogStart("Fog Start",Float) = 0
		_FogEnd("Fog End",Float) = 1
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
#include "PostEffectInclude.cginc"

			half _FogDensity;
			fixed4 _FogColor;
			float _FogStart;
			float _FogEnd;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
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
				float3 worldPos = _WorldSpaceCameraPos+ i.interpolatedRay*linearDepth;
				float fogDensity = saturate( (_FogEnd - worldPos.y)*_FogDensity / (_FogEnd - _FogStart));
				fixed3 col = tex2D(_MainTex, i.uv).rgb;
				col.rgb = lerp(col.rgb, _FogColor.rgb, fogDensity);
				return fixed4( col,1);
			}
			ENDCG
		}
	}
}
