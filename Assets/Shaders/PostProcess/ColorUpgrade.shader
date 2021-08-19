Shader "Hidden/PostProcess/ColorUpgrade"
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
			#define IDEPTH
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
					col= lerp(col, SampleLUT(lutCol,TEXTURE2D_ARGS(_LUTTex,sampler_LUTTex),_LUTTex_TexelSize,_LUTCellCount)+offset,_LUTWeight);
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
		    half3 _BloomColor;
			TEXTURE2D(_Bloom_Blur);SAMPLER(sampler_Bloom_Blur);
			#endif
			half3 Bloom(half3 col,float2 uv)
            {
				#if _BLOOM
					col += SAMPLE_TEXTURE2D(_Bloom_Blur,sampler_Bloom_Blur, uv).rgb *_BloomColor;
				#endif
	            return col;
            }

			//FXAA
			#pragma shader_feature_local _FXAA
			#pragma shader_feature_local _FXAA_DEPTH
			// #pragma shader_feature_local _FXAA_SUBPIXEL
			#pragma shader_feature_local _FXAA_ADDITIONAL_SAMPLE
			// #pragma shader_feature_local _FXAA_EDGE
			
			half _FXAAContrastSkip;
			half _FXAARelativeSkip;
			half _FXAABlendStrength;
			half FXAASource(float2 uv)
			{
				#if _FXAA_DEPTH
					return 1.h-Sample01Depth(uv);
				#else
					half3 color=SAMPLE_TEXTURE2D_LOD(_MainTex,sampler_MainTex, uv,0).rgb;
					return RGBtoLuminance(saturate(color));
				#endif
			}
			half3 FXAASample(float2 uv)
			{
				#ifndef _FXAA
					return SampleMainTex(uv).rgb;
				#endif

				half m = FXAASource(uv);
				half n = FXAASource(uv+_MainTex_TexelUp);
				half s = FXAASource(uv-_MainTex_TexelUp);
				half e = FXAASource(uv+_MainTex_TexelRight);
				half w = FXAASource(uv-_MainTex_TexelRight);
				half maxAA=max(m,n,s,e,w);
				half minAA=min(m,n,s,e,w);
				half contrast=maxAA-minAA;
				half contrastThreshold=max(_FXAAContrastSkip,_FXAARelativeSkip*maxAA);
				if(contrast<contrastThreshold)
					return SampleMainTex(uv).rgb;

				#if _FXAA_ADDITIONAL_SAMPLE
					half ne= FXAASource(uv+_MainTex_TexelUpRight);
					half nw= FXAASource(uv+_MainTex_TexelUpLeft);
					half se= FXAASource(uv-_MainTex_TexelUpLeft);
					half sw= FXAASource(uv-_MainTex_TexelUpRight);
				#endif

				half horizontal=abs(n+s-2.h*m);
				half vertical=abs(e+w-2.h*m);
				#if _FXAA_ADDITIONAL_SAMPLE
					horizontal=horizontal*2.h+abs(ne+se-2.h*e)+abs(nw+sw*2.h-w);
					vertical=vertical*2.h+abs(ne+nw-2.h*n)+abs(se+sw-2.h*s);
				#endif

				uint isHorizontal= step(vertical,horizontal);
				half pos=lerp(e,n,isHorizontal);
				half neg=lerp(w,s,isHorizontal);
				half posGradient=abs(pos-m);
				half negGradient=abs(neg-m);
				uint negative=step(posGradient,negGradient);
				uint2 sampleDirection=uint2(1u-isHorizontal,isHorizontal)*lerp(1,-1,negative);
				
				half pixelBlend=0.h;
				half edgeBlend=0.h;
				// #if _FXAA_SUBPIXEL
					uint filterCount=4u;
					half filter=(n+e+s+w);
					#if _FXAA_ADDITIONAL_SAMPLE
						filter*=2.0h;
						filter+=ne+nw+se+sw;
						filterCount+=8u;
					#endif
					filter*=rcp(filterCount);
					filter=abs(filter-m);
					filter=saturate(filter*rcp(contrast));
					pixelBlend= pow2(smoothstep(0.h,1.h,filter))*_FXAABlendStrength;
				// #endif
				
				#if _FXAA_EDGE
					half opposite=lerp(pos,neg,negative);
					half gradient=lerp(posGradient,negGradient,negative);
					half edge=(opposite+m)*.5h;
					half gradientThreshold=gradient*.5h;

					float2 uvEdge=uv+sampleDirection*_MainTex_TexelSize.xy*.5;
					float2 edgeStep=uint2(isHorizontal,1u-isHorizontal)*_MainTex_TexelSize.xy;
					float2 posUV=uvEdge;
					uint posIndex=0u;
					[unroll]
					for(uint i=0;i<10u;i++)
					{
						posUV+=edgeStep;
						if(abs(FXAASource(posUV)-edge)>gradientThreshold)
						{
							posIndex=i;
							break;
						}
					}
					
					float2 negUV=uvEdge;
					uint negIndex=0u;
					[unroll]
					for(uint j=0;j<10u;j++)
					{
						negUV-=edgeStep;
						if(abs(FXAASource(negUV)-edge)>gradientThreshold)
						{
							negIndex=j;
							break;
						}
					}
					edgeBlend=.5-max(posIndex,negIndex)/9.;
				#endif
				
				uv+=sampleDirection*_MainTex_TexelSize.xy*max(pixelBlend,edgeBlend);
				return SampleMainTex(uv).rgb;
			}
			
			
			half4 frag (v2f_img i) : SV_Target
			{
				half3 col=FXAASample(i.uv);
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
		       
		        half _BloomThreshold;
		        float4 frag(v2f_img i) : SV_Target
		        {
		            float4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
		            color*=step(_BloomThreshold+0.01,RGBtoLuminance(color.rgb));
		            return color;
		        }
	        ENDHLSL
        }

		
	}
}
