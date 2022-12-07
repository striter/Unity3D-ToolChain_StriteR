Shader "Hidden/PostProcess/AntiAliasing"
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
			NAME "FXAA"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			//FXAA
			#pragma multi_compile_local_fragment _ _FXAA_DEPTH
			#pragma multi_compile_local _ _FXAA_SUBPIXEL
			#pragma multi_compile_local_fragment _FXAA_ADDITIONAL_SAMPLE
			#pragma multi_compile_local _ _FXAA_EDGE
			
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
				#if _FXAA_SUBPIXEL
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
				#endif
				
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
				return half4(FXAASample(i.uv),1);
			}
			ENDHLSL
		}
	}
}
