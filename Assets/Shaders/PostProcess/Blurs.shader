﻿Shader "Hidden/PostProcess/Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

    HLSLINCLUDE
    #pragma target 3.5
    #include "Assets/Shaders/Library/PostProcess.hlsl"
    
    #pragma multi_compile_local_fragment _ _DOF _MASK _DOF_MASK _TILT_SHIFT
	#include "BlurFilters.hlsl"
    
	half _BlurSize;
	uint _Iteration;
	half _Angle;

    half4 _Vector;
	half4 _Attenuation;

	static const half gaussianWeight3[3] = {0.4026h,0.2442h,0.0545h};
	half4 HexagonBlurTexture(TEXTURE2D_PARAM(_tex,_samp),float2 uv,float2 direction)
	{
		half4 finalCol=0;
		for(uint i=0u ; i<_Iteration ; i++)
		{
			half4 hexagonBlur = SampleBlurTex(TEXTURE2D_ARGS(_tex,_samp),uv,direction*float2(i+.5,i+.5));
			finalCol+=hexagonBlur;
		}
		return RecordBlurTex(finalCol/_Iteration);
	}
	ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Cull Off
		Pass
		{
			NAME "KAWASE_BLUR"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragKawase
			half4 fragKawase(v2f_img i):SV_TARGET
			{
				float2 uvDelta=_BlurTex_TexelSize.xy *_BlurSize;
				half4 sum = 0;
				sum += SampleMainBlur(i.uv , float2(0, 1)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(1,0)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(0, -1)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
				return RecordBlurTex(sum*.25);
			}
			ENDHLSL
		}

		Pass
		{
			NAME "AVERAGE_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragAverageBlurHorizontal
			
			half4 fragAverageBlurHorizontal(v2f_img i):SV_TARGET
			{
				float2 uvDelta=_BlurTex_TexelSize.xy *_BlurSize;
				half4 sum = SampleMainBlur(i.uv,0);
				sum += SampleMainBlur(i.uv , float2(1, 0)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(2, 0)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(-2, 0)*uvDelta);
				return RecordBlurTex(sum*.2);
			}
			ENDHLSL
		}
		
		Pass
		{
			NAME "AVERAGE_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragAverageBlurVertical
			
			half4 fragAverageBlurVertical(v2f_img i):SV_TARGET
			{
				float2 uvDelta=_BlurTex_TexelSize.xy *_BlurSize;
				half4 sum = SampleMainBlur(i.uv,0);
				sum += SampleMainBlur(i.uv , float2(0,1)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(0, -1)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(0, 2)*uvDelta);
				sum += SampleMainBlur(i.uv , float2(0, -2)*uvDelta);
				return RecordBlurTex(sum*.2);
			}
			ENDHLSL
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragGaussianBlurHorizontal
			half4 fragGaussianBlurHorizontal(v2f_img i) :SV_TARGET
			{
				float2 uvDelta=_BlurTex_TexelSize.xy *_BlurSize;
				half4 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
				sum += SampleMainBlur(i.uv , float2(1,0)*uvDelta)*gaussianWeight3[1];
				sum += SampleMainBlur(i.uv , float2(-1,0)*uvDelta)*gaussianWeight3[1];
				sum += SampleMainBlur(i.uv , float2(2,0)*uvDelta)*gaussianWeight3[2];
				sum += SampleMainBlur(i.uv , float2(-2,0)*uvDelta)*gaussianWeight3[2];
				return RecordBlurTex(sum);
			}
			ENDHLSL
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragGaussianBlurVertical
			half4 fragGaussianBlurVertical(v2f_img i) :SV_TARGET
			{
				float2 uvDelta = _BlurTex_TexelSize.xy *_BlurSize;
				half4 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
				sum += SampleMainBlur(i.uv , float2(0,1)*uvDelta)*gaussianWeight3[1];
				sum += SampleMainBlur(i.uv , float2(0,-1)*uvDelta)*gaussianWeight3[1];
				sum += SampleMainBlur(i.uv , float2(0,2)*uvDelta)*gaussianWeight3[2];
				sum += SampleMainBlur(i.uv , float2(0,-2)*uvDelta)*gaussianWeight3[2];
				return RecordBlurTex(sum);
			}
			ENDHLSL
		}
		
		Pass
		{
			Name "DUALFILTERING_DOWNSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragDualFilteringDownSample
			//Dual Filtering
			half4 fragDualFilteringDownSample(v2f_img i):SV_TARGET
			{
				return RecordBlurTex(DualFilteringDownFilter(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,_BlurTex_TexelSize,_BlurSize));
			}

	
			ENDHLSL
		}

		Pass
		{
			Name "DUALFILTERING_UPSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragDualFilteringUpSample
			half4 fragDualFilteringUpSample(v2f_img i):SV_TARGET
			{
				return RecordBlurTex(DualFilteringUpFilter(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,_BlurTex_TexelSize,_BlurSize));
			}
			ENDHLSL
		}
		
		Pass
		{
			Name "GRAINY"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragGrainy
			half4 fragGrainy(v2f_img i):SV_TARGET
			{
				float2 delta=_BlurTex_TexelSize.xy*_BlurSize;
				float2 randomUV=randomUnitCircle(i.uv)*random(i.uv)*delta;
				return SampleBlurTex(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex), i.uv,randomUV);
			}


			ENDHLSL
		}
		
		Pass
		{
			Name "BOKEH"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragBokeh
			#define _GOLDENANGLE 2.39996h
			half4 fragBokeh(v2f_img i):SV_TARGET
			{
				float2x2 rot=Rotate2x2(_GOLDENANGLE);
				float2 rotate=float2(0,_BlurSize);
				rotate=mul(Rotate2x2(_Angle),rotate);
				float4 sum=0;
				float r=1;
				for(uint j=0u;j<_Iteration;j++)
				{
					r+=1.0/r;
					rotate=mul(rot,rotate);
					half4 bokeh = SampleBlurTex(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,(r-1.0)*rotate*_BlurTex_TexelSize.xy);
					sum += bokeh;
				}
				return sum/_Iteration;
			}
			ENDHLSL
		}

		Pass
		{
			Name "HEXAGON_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragHexagonVertical
			
			half4 fragHexagonVertical(v2f_img i):SV_TARGET
			{
				half angle=_Angle-HALF_PI;
				half sinA,cosA;
				sincos(angle,sinA,cosA);
				half2 dir = half2(cosA,sinA)*_BlurTex_TexelSize.xy*_BlurSize;
				return RecordBlurTex(HexagonBlurTexture(TEXTURE2D_ARGS( _BlurTex,sampler_BlurTex),i.uv,dir));
			}
			ENDHLSL
		}

		Pass
		{
			Name "HEXAGON_DIAGONAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragHexagonDiagonal
			TEXTURE2D(_Hexagon_Vertical);SAMPLER(sampler_Hexagon_Vertical);
			half4 fragHexagonDiagonal(v2f_img i):SV_TARGET
			{
				half angle=_Angle+PI*0.16666h;
				half sinA,cosA;
				sincos(angle,sinA,cosA);
				float2 dir = float2(cosA,sinA)*_BlurTex_TexelSize.xy*_BlurSize;
				half4 combined = (SampleBlurTex(TEXTURE2D_ARGS(_Hexagon_Vertical,sampler_Hexagon_Vertical), i.uv,0)+HexagonBlurTexture(TEXTURE2D_ARGS( _BlurTex,sampler_BlurTex),i.uv,dir))*.5f;
				return RecordBlurTex(combined);
			}

			ENDHLSL
		}

		Pass 
		{
			Name "HEXAGON_RHOMBOID"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragHexagonRamboid
			TEXTURE2D( _Hexagon_Diagonal);SAMPLER(sampler_Hexagon_Diagonal);
			TEXTURE2D(_Hexagon_Vertical);SAMPLER(sampler_Hexagon_Vertical);
			half4 fragHexagonRamboid(v2f_img i):SV_TARGET
			{
				half sinA,cosA;
				sincos(_Angle+PI*0.16666h,sinA,cosA);
				float2 verticalBlurDirection=float2(cosA,sinA)*_BlurTex_TexelSize.xy*_BlurSize;
				half4 vertical = HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Vertical,sampler_Hexagon_Vertical),i.uv,verticalBlurDirection);
				sincos(_Angle+PI*0.833333h,sinA,cosA);
				float2 diagonalBlurDirection=float2(cosA,sinA)*_BlurTex_TexelSize.xy*_BlurSize;
				half4 diagonal = HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Diagonal,sampler_Hexagon_Diagonal),i.uv,diagonalBlurDirection);
				return RecordBlurTex((vertical+diagonal*2.0h)*0.3333h);
			}

			ENDHLSL
		}

		Pass
		{
			Name "BLINKING_RADIAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragBlinkingRadial
			//Radial
			half4 fragBlinkingRadial(v2f_img i):SV_TARGET
			{
				float2 uvDelta = _BlurTex_TexelSize.xy*_Vector.xy;
				
				half4 sum = 0;
				sum += SampleMainBlur(i.uv , 0);
				sum += SampleMainBlur(i.uv , uvDelta)*_Attenuation.y;
				sum += SampleMainBlur(i.uv , uvDelta*2)*_Attenuation.z;
				sum += SampleMainBlur(i.uv , uvDelta*3)*_Attenuation.w;

				float2 uvDelta2 = _BlurTex_TexelSize.xy*_Vector.zw;
				sum += SampleMainBlur(i.uv , 0);
				sum += SampleMainBlur(i.uv , uvDelta2)*_Attenuation.y;
				sum += SampleMainBlur(i.uv , uvDelta2*2)*_Attenuation.z;
				sum += SampleMainBlur(i.uv , uvDelta2*3)*_Attenuation.w;

				return RecordBlurTex(sum*.125);
			}
			ENDHLSL
		}
    	
    	Pass
    	{
    		Name "BLINKING_COMBINE"
    		HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
    		#pragma fragment fragBlinkingCombine
					
			TEXTURE2D(_Blinking_Vertical);SAMPLER(sampler_Blinking_Vertical);
			TEXTURE2D(_Blinking_Horizontal);SAMPLER(sampler_Blinking_Horizontal);
		    half4 fragBlinkingCombine(v2f_img i):SV_TARGET
		    {
    			half4 vertical = SampleBlurTex(TEXTURE2D_ARGS(_Blinking_Vertical,sampler_Blinking_Vertical),i.uv,0);
    			half4 horizontal = SampleBlurTex(TEXTURE2D_ARGS(_Blinking_Horizontal,sampler_Blinking_Horizontal),i.uv,0);
    			return (vertical+horizontal)/2;
		    }
    		ENDHLSL
    	}
    	
    	Pass
		{
			Name "NEXTGEN_DOWNSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragNextGenDownSample
    
			//Dual Filtering
			half4 fragNextGenDownSample(v2f_img i):SV_TARGET
			{
				return RecordBlurTex( DualFilteringDownFilter(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,_BlurTex_TexelSize,_BlurSize));
			}

			ENDHLSL
		}

		Pass
		{
			Name "NEXTGEN_UPSAMPLE"
			HLSLPROGRAM
			#pragma multi_compile_local_fragment _ _BLOOM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragNextGenUpSample
		    TEXTURE2D(_PreDownSample);
		    SAMPLER(sampler_PreDownSample);
		    float4 _PreDownSample_TexelSize;
			half4 fragNextGenUpSample(v2f_img i):SV_TARGET
			{
				half4 srcCol = DualFilteringUpFilter(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,_BlurTex_TexelSize,_BlurSize);
				half4 mipCol = DualFilteringUpFilter(TEXTURE2D_ARGS(_PreDownSample,sampler_PreDownSample),i.uv,_PreDownSample_TexelSize,_BlurSize);

				half4 finalCol = RecordBlurTex(srcCol) + RecordBlurTex(mipCol);
				#ifndef _BLOOM
					finalCol = finalCol * 0.5f;
				#endif
				
				return finalCol;
			}
			ENDHLSL
		}
		
		Pass
		{
			Name "NEXTGEN_UPSAMPLE_FINAL"
			HLSLPROGRAM
			#pragma vertex vert_fullScreenMesh
			#pragma fragment fragNextGenUpSampleFinal
		    half4 fragNextGenUpSampleFinal(v2f_img i):SV_TARGET
			{
				return RecordBlurTex(DualFilteringUpFilter(TEXTURE2D_ARGS(_BlurTex,sampler_BlurTex),i.uv,_BlurTex_TexelSize,_BlurSize));
			}
			ENDHLSL
		}
		
	}
}