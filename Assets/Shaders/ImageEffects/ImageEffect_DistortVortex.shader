Shader "Hidden/ImageEffect_DistortVortex"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex("Noise Tex",2D)="white"{}
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
			sampler2D _NoiseTex;
			float _NoiseStrength;
			float4 _DistortParam;		//xy ViewPort z OffsetFactor;

			fixed4 frag (v2f_img i) : SV_Target
			{
				float2 dir = i.uv - _DistortParam.xy;
				float2 distort = normalize(dir)*(1 - length(dir))*_DistortParam.z;
				float noise = tex2D(_NoiseTex,i.uv).r*_NoiseStrength;
				distort *= noise;
				return  tex2D(_MainTex, i.uv + distort);
			}
			ENDCG
		}
	}
}
