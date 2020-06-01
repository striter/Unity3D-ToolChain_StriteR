Shader "Hidden/PostEffect/PE_BSC"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_Brightness("Brightness",Range(0,3)) = 1
		_Saturation("Saturation", Range(0, 3)) = 1
		_Contrast("Contrast", Range(0, 3)) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "../CommonInclude.cginc"
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed _Brightness;
			fixed _Saturation;
			fixed _Contrast;

			
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed3 col = tex2D(_MainTex, i.uv).rgb*_Brightness;
				
				fixed lum = luminance(col);
				fixed3 lumCol = fixed3(lum, lum, lum);
				col = lerp(lumCol, col, _Saturation);
			
				fixed3 avgCol = fixed3(.5, .5, .5);
				col = lerp(avgCol, col, _Contrast);

				return fixed4( col,1);
			}
			ENDCG
		}
	}
}
