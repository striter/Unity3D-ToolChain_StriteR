Shader "Hidden/ImageEffect_ColorGrading"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LUTTex("Color Correction Table",2D)="white"{}
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
			#pragma shader_feature _LUT
			#pragma shader_feature _BSC
			#pragma shader_feature _CHANNEL_MIXER
			#include "UnityCG.cginc"
			
		#if _LUT
		uniform sampler2D _LUTTex;
		int _LUTCellCount;
		half3 SampleLUT(half3 sampleCol) {
			half width=_LUTCellCount;
			half sliceSize = 1.0h / width;              // space of 1 slice
			half slicePixelSize = sliceSize / width;           // space of 1 pixel
			half sliceInnerSize = slicePixelSize * (width - 1.0h);  // space of width pixels
			half zSlice0 = min(floor(sampleCol.b * width), width - 1.0h);
			half zSlice1 = min(zSlice0 + 1.0h, width - 1.0h);
			half xOffset = slicePixelSize * 0.5h + sampleCol.r * sliceInnerSize;
			half s0 = xOffset + (zSlice0 * sliceSize);
			half s1 = xOffset + (zSlice1 * sliceSize);
			half3 slice0Color = tex2D(_LUTTex, half2(s0, sampleCol.g));
			half3 slice1Color = tex2D(_LUTTex, half2(s1, sampleCol.g));
			half zOffset = fmod(sampleCol.b * width, 1.0h);
			half3 result = lerp(slice0Color, slice1Color, zOffset);
			return result;
		}
		#endif

		#if _BSC
		uniform half _Saturation;
		float3 Saturation(float3 c)
		{
			float luma =  dot(c, float3(0.2126729, 0.7151522, 0.0721750));
			return luma.xxx + _Saturation.xxx * (c - luma.xxx);
		}

		uniform half _Brightness;
		uniform half _Contrast;
		half3 GetBSCCol(half3 col)
		{
			half3 avgCol = half3(.5h, .5h, .5h);
			col = lerp(avgCol, col, _Contrast);
				
			half rgbMax=max(max(col.r,col.g),col.b);
			half rgbMin=min(min(col.r,col.g),col.b);

			col*=_Brightness;
				
			return Saturation(col);
		}
		#endif

		#if _CHANNEL_MIXER
		uniform half4 _MixRed;
		uniform half4 _MixGreen;
		uniform half4 _MixBlue;
		half GetMixerAmount(half3 col,half3 mix)
		{
			return col.r*mix.x+col.g*mix.y+col.b*mix.z;
		}

		half3 ChannelMixing(half3 col)
		{
			half redMixAmount=GetMixerAmount(col,_MixRed);
			half greenMixAmount=GetMixerAmount(col,_MixGreen);
			half blueMixAmount=GetMixerAmount(col,_MixBlue);
			col.r+=redMixAmount;
			col.g+=greenMixAmount;
			col.b+=blueMixAmount;
			return col;
		}
		#endif

			uniform sampler2D _MainTex;
			uniform half _Weight;
			half4 frag (v2f_img i) : SV_Target
			{
				half3 baseCol=tex2D(_MainTex, i.uv).rgb;
				half3 targetCol=baseCol;

				#if _LUT
					targetCol=SampleLUT(targetCol);
				#endif
				#if _BSC
					targetCol=GetBSCCol(targetCol);
				#endif
				#if _CHANNEL_MIXER
					targetCol=ChannelMixing(targetCol);
				#endif
				return half4(lerp(baseCol,targetCol,_Weight),1) ;
			}
			ENDCG
		}
	}
    FallBack Off
}
