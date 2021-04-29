Shader "Hidden/ImageEffect_ColorGrading"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
		_Weight("Weight",Range(0,1))=1
		[Toggle(_BSC)] _Enable_BSC ("BSC Enable", Float) = 1
		_Brightness("Brightness",Range(0,2))=1
		_Saturation("Saturation",Range(0,2))=1
		_Contrast("Contrast",Range(0,2))=1
		[Toggle(_LUT)] _Enable_LUT ("LUT Enable", Float) = 0
		[NoScaleOffset]_LUTTex("Look Up Table",2D)="white"{}
		[Enum(enum_LUTCellCount)] _LUTCellCount("LUT Cell Count",int)=16
		[Toggle(_CHANNEL_MIXER)] _Enable_CHANNEL_MIXER ("Channel Mixer Enable", Float) = 0
		_MixRed("Mix Red",Vector)=(0,0,0,0)
		_MixGreen("Mix Green",Vector)=(0,0,0,0)
		_MixBlue("Mix Blue",Vector)=(0,0,0,0)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "PreviewType"="Plane"}
		// No culling or depth
		Cull Off ZWrite Off 

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
			sampler2D _LUTTex;
			float4 _LUTTex_TexelSize;
			int _LUTCellCount;

			half3 SampleLUT(half3 sampleCol) {
				half width=_LUTCellCount;
				half minWidth=_LUTCellCount-1;

				int lutCellPixelCount = _LUTTex_TexelSize.z / width;
				int x0CellIndex =  floor(sampleCol.b * width);
				int x1CellIndex = x0CellIndex+1;
				x0CellIndex=min(x0CellIndex,minWidth);
				x1CellIndex=min(x1CellIndex,minWidth);

				half x0PixelCount = x0CellIndex* lutCellPixelCount + (lutCellPixelCount -1)* sampleCol.r;
				half x1PixelCount = x1CellIndex * lutCellPixelCount + (lutCellPixelCount - 1) * sampleCol.r;
				half yPixelCount = sampleCol.g*_LUTTex_TexelSize.w;

				half2 uv0 = float2(x0PixelCount, yPixelCount) * _LUTTex_TexelSize.xy;
				half2 uv1= float2(x1PixelCount, yPixelCount) * _LUTTex_TexelSize.xy;

				half zOffset = fmod(sampleCol.b * width, 1.0h);
				return lerp( tex2D(_LUTTex,uv0),tex2D(_LUTTex,uv1),zOffset) ;
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
					targetCol=SampleLUT(saturate(targetCol));		//saturate For HDR
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
}
