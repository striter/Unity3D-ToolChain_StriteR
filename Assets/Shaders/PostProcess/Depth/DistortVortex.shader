Shader "Hidden/PostProcess/DistortVortex"
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
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

            #include "../../PostProcessInclude.hlsl"

			TEXTURE2D( _NoiseTex);SAMPLER(sampler_NoiseTex);
			float _NoiseStrength;
			float4 _DistortParam;		//xy ViewPort z OffsetFactor;

			float4 frag (v2f_img i) : SV_Target
			{
				float2 dir = i.uv - _DistortParam.xy;
				float2 distort = normalize(dir)*(1 - length(dir))*_DistortParam.z;
				float noise = SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,i.uv).r*_NoiseStrength;
				distort *= noise;
				return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv + distort);
			}
			ENDHLSL
		}
	}
}
