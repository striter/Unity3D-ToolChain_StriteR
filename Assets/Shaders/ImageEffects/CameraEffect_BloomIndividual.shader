Shader "Hidden/CameraEffect_BloomIndividual"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	#pragma multi_compile _ _BLOOMINDIVIDUAL_ADDITIVE _BLOOMINDIVIDUAL_ALPHABLEND
	sampler2D _MainTex;
	sampler2D _TargetTex;
	float _Intensity;
	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{		
			name "Main"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			fixed4 frag(v2f_img i) : SV_Target
			{
				float4 baseCol=tex2D(_MainTex,i.uv);
				float4 blendCol=tex2D(_TargetTex,i.uv)*_Intensity;
				#if _BLOOMINDIVIDUAL_ADDITIVE
					return baseCol+blendCol;
				#elif _BLOOMINDIVIDUAL_ALPHABLEND
					return blendCol*blendCol.a +baseCol*(1-blendCol.a);
				#else
					return blendCol;
				#endif
			}
			ENDCG
		}
	}
}
