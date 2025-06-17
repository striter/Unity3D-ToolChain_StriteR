﻿Shader "Hidden/PostProcess/ColorGrading"
{
	Properties
	{
		 [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		HLSLINCLUDE
			#include "Assets/Shaders/Library/PostProcess.hlsl"
		ENDHLSL
		Pass
		{
			NAME "UberColorProcess"
			HLSLPROGRAM
			#pragma vertex vert_blit
			#pragma fragment frag
			#pragma multi_compile_local_fragment _ _LUT
			#pragma multi_compile_local_fragment _ _BSC
			#pragma multi_compile_local_fragment _ _CHANNEL_MIXER

			//Color Grading
			TEXTURE2D(_LUTTex);SAMPLER(sampler_LUTTex);
			half _LUTWeight;
			float4 _LUTTex_TexelSize;
			uint4 _LUTCellCount;
			half _Saturation;
			half _Brightness;
			half _Contrast;
			half3 _MixRed;
			half3 _MixGreen;
			half3 _MixBlue;
			
            half3 ColorGrading(half3 col,float2 uv)
			{
            	half3 srcCol=col;
            	
				#if _LUT
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
		
			#pragma multi_compile_local_fragment _ _TONEMAP_ACES
			#include "Library/ToneMapping.hlsl"
			half3 ToneMapping(half3 col)
			{
				#if _TONEMAP_ACES
            		col = unity_to_ACES(col);
            		col = AcesTonemap(col);
				#endif
				return col;
			}
			
			//Motion Blur
			#pragma multi_compile_local_fragment _ _MOTIONBLUR
			int _Iteration;
			float _Intensity;
			
			half3 GatherMotionBlurSample(int index,float invSampleCount,float2 center,float random,float2 velocity)
            {
	            float offset = (index + .5f) * (random - .5);
            	float2 sampleUV = center + (offset * invSampleCount) * velocity;
            	return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,sampleUV).rgb;
            }
			
			half3 MotionBlur(float2 uv)
            {
				#if _MOTIONBLUR
            		float randomValue = random(uv);
		            float2 velocity = SampleMotionVector(uv) * _Intensity;
					float invSampleCount = rcp(_Iteration*2);

            		half3 color = 0;
            		for(int i=0;i<_Iteration;i++)
            		{
            			color += GatherMotionBlurSample(i,invSampleCount,uv,randomValue,velocity);
            			color += GatherMotionBlurSample(i,invSampleCount,uv,-randomValue,-velocity);
            		}
            		return half3(color * invSampleCount);
            	#endif
            	return SampleMainTex(uv);
            }
				
			//Bloom
			#pragma multi_compile_local_fragment _ _BLOOM
		    half4 _BloomColor;
			TEXTURE2D(_Bloom_Blur);SAMPLER(sampler_Bloom_Blur);
			half3 Bloom(half3 col,float2 uv)
            {
				#if _BLOOM
					col += SAMPLE_TEXTURE2D(_Bloom_Blur,sampler_Bloom_Blur, uv).rgb *_BloomColor.rgb;
				#endif
	            return col;
            }
			
			half4 frag (v2f_img i) : SV_Target
			{
				half3 col = MotionBlur(i.uv);
				col = ToneMapping(col);
				col = Bloom(col,i.uv);
				col = ColorGrading(col,i.uv);
				return half4(col,1);
			}
			ENDHLSL
		}
		
        Pass{
	        HLSLPROGRAM
	            #pragma vertex vert_blit
	            #pragma fragment frag
		       
		        half4 _BloomThreshold;
		        float4 frag(v2f_img i) : SV_Target
		        {
		            float4 color = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);

		        	half Threshold = _BloomThreshold.r;
		            half ThresholdKnee = _BloomThreshold.g;
					half brightness = max(color.r, color.g, color.b);
		            half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
		            softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
		            half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
		            color *= multiplier;
		            return max(0,color);
		        }
	        ENDHLSL
        }

		
	}
}
