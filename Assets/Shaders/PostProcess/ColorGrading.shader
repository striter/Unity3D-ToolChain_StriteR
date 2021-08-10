Shader "Hidden/PostProcess/ColorGrading"
{
	Properties
	{
		 [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma shader_feature_local _LUT
			#pragma shader_feature_local _BSC
			#pragma shader_feature_local _CHANNEL_MIXER

			#define ICOLOR
			#include "Assets/Shaders/Library/PostProcess.hlsl"
			
            float _Weight;
			#if _LUT
			TEXTURE2D(_LUTTex);SAMPLER(sampler_LUTTex);
			float4 _LUTTex_TexelSize;
			int _LUTCellCount;
			#endif
			#if _BSC
			half _Saturation;
			half _Brightness;
			half _Contrast;
			#endif
            #if _CHANNEL_MIXER
			half3 _MixRed;
			half3 _MixGreen;
			half3 _MixBlue;
            #endif
            half3 ColorGrading(half3 col)
			{
            	half3 targetCol=col;
				#if _LUT
            		half3 srcCol=targetCol;
					half3 lutCol=saturate(targetCol);		//saturate For HDR
					half3 offset=srcCol-lutCol;
					targetCol= SampleLUT(lutCol,_LUTTex,sampler_LUTTex,_LUTTex_TexelSize,_LUTCellCount)+offset;
				#endif

				#if _BSC
					targetCol *=_Brightness;
					targetCol = lerp(.5h, targetCol, _Contrast);
					targetCol = Saturation(targetCol,_Saturation);
				#endif

				#if _CHANNEL_MIXER
					targetCol = mul(targetCol,half3x3(_MixRed,_MixGreen,_MixBlue));
				#endif
				return lerp(col,targetCol,_Weight);
			}
			half4 frag (v2f_img i) : SV_Target
			{
				return half4(ColorGrading(SampleMainTex(i.uv).rgb),1) ;
			}
			ENDHLSL
		}
	}
}
