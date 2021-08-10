Shader "Hidden/PostProcess/ColorGrading"
{
	Properties
	{
		 [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		HLSLINCLUDE
			#define ICOLOR
			#include "Assets/Shaders/Library/PostProcess.hlsl"
		ENDHLSL
		Pass
		{
			NAME "UberColorProcess"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma shader_feature_local _LUT
			#pragma shader_feature_local _BSC
			#pragma shader_feature_local _CHANNEL_MIXER

			//Color Grading
			#if _LUT
			TEXTURE2D(_LUTTex);SAMPLER(sampler_LUTTex);
			half _LUTWeight;
			float4 _LUTTex_TexelSize;
			uint _LUTCellCount;
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
				#if _LUT
            		half3 srcCol=col;
					half3 lutCol=saturate(col);		//saturate For HDR
					half3 offset=srcCol-lutCol;
					col= lerp(col, SampleLUT(lutCol,_LUTTex,sampler_LUTTex,_LUTTex_TexelSize,_LUTCellCount)+offset,_LUTWeight);
				#endif

				#if _BSC
					col *=_Brightness;
					col = lerp(.5h, col, _Contrast);
					col = Saturation(col,_Saturation);
				#endif

				#if _CHANNEL_MIXER
					col = mul(col,half3x3(_MixRed,_MixGreen,_MixBlue));
				#endif
				return col;
			}

			//Bloom
			#pragma shader_feature_local _BLOOM
			#if _BLOOM
		    half _Intensity;
			TEXTURE2D(_Bloom_Blur);SAMPLER(sampler_Bloom_Blur);
			#endif
			half3 Bloom(half3 col,float2 uv)
            {
				#if _BLOOM
					col += SAMPLE_TEXTURE2D(_Bloom_Blur,sampler_Bloom_Blur, uv).rgb *_Intensity;
				#endif
	            return col;
            }

			//FXAA
			#pragma multi_compile_local _ _FXAA_6 _FXAA_10
			#ifndef _FXAA_6
				#ifndef _FXAA_10
					#define _FXAA_NONE
				#endif
			#endif
			half _FXAAContrastSkip;
			half _FXAARelativeSkip;
			half _FXAABlendStrength;
			half FXAALuminance(float2 uv) { return Luminance( SampleMainTex(uv).rgb); }
			half3 FXAA(float2 uv)
			{
				#ifdef _FXAA_NONE
					return SampleMainTex(uv).rgb;
				#endif
				half m= FXAALuminance(uv);
				half n = FXAALuminance(uv+_MainTex_TexelUp);
				half s = FXAALuminance(uv-_MainTex_TexelUp);
				half e = FXAALuminance(uv+_MainTex_TexelRight);
				half w = FXAALuminance(uv-_MainTex_TexelRight);
				half maxLum=max(m,n,s,e,w);
				half minLum=min(m,n,s,e,w);
				half contrast=maxLum-minLum;

				half threshold=max(_FXAAContrastSkip,_FXAARelativeSkip*maxLum);
				if(contrast<threshold)
					return SampleMainTex(uv).rgb;

				#if _FXAA_10
				half ne= FXAALuminance(uv+_MainTex_TexelUpRight);
				half nw= FXAALuminance(uv+_MainTex_TexelUpLeft);
				half se= FXAALuminance(uv-_MainTex_TexelUpLeft);
				half sw= FXAALuminance(uv-_MainTex_TexelUpRight);
				#endif
				
				uint filterCount=4u;
				half filter=(n+e+s+w);
				#if _FXAA_10
					filter*=2.0h;
					filter+=ne+nw+se+sw;
					filterCount+=8u;
				#endif
				filter*=rcp(filterCount);
				filter=abs(filter-m);
				filter=saturate(filter*rcp(contrast));
				half blend=pow2(smoothstep(0.h,1.h,filter))*_FXAABlendStrength;

				half horizontal=abs(n+s-2.h*m);
				half vertical=abs(e+w-2.h*m);
				#if _FXAA_10
					horizontal=horizontal*2.h+abs(ne+se-2.h*e)+abs(nw+sw*2.h-w);
					vertical=vertical*2.h+abs(ne+nw-2.h*n)+abs(se+sw-2.h*s);
				#endif
				
				half isHorizontal= step(vertical,horizontal);
				half pLuminance=lerp(e,n,isHorizontal);
				half nLuminance=lerp(w,s,isHorizontal);
				half pGradient=abs(pLuminance-m);
				half nGradient=abs(nLuminance-m);
				half negative=lerp(-1.h,1.h,step(nGradient,pGradient));
				half2 direction=half2(1.0h-isHorizontal,isHorizontal);
				half2 uvOffset=direction*_MainTex_TexelSize.xy*negative*blend;
				uv+=uvOffset;
				return SampleMainTex(uv).rgb;
			}
			
			
			half4 frag (v2f_img i) : SV_Target
			{
				half3 col=FXAA(i.uv);
				col=Bloom(col,i.uv);
				col=ColorGrading(col);
				return half4(col,1) ;
			}
			ENDHLSL
		}
		
        Pass{
	        HLSLPROGRAM
	            #pragma vertex vert_img
	            #pragma fragment frag
		       
		        half _Threshold;
		        float4 frag(v2f_img i) : SV_Target
		        {
		            float4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
		            color*=step(_Threshold+0.01,(color.r+color.g+color.b)/3);
		            return color;
		        }
	        ENDHLSL
        }

		
	}
}
