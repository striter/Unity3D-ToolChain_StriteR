Shader "Hidden/PostProcess/Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

	
    HLSLINCLUDE
    #pragma target 3.5
	half _BlurSize;
	uint _Iteration;
	half _Angle;
	half2 _Vector;
    #pragma multi_compile_local_fragment _ _DOF _DOF_MASK
    #pragma multi_compile _ _FIRSTBLUR
    #pragma multi_compile _ _FINALBLUR
    #pragma multi_compile_local_fragment _ _ENCODE
	#if defined(_DOF)||defined(_DOF_CLIPSKY)
		#define IDEPTH
        half _FocalStart;
        half _FocalEnd;
    #endif
    #include "Assets/Shaders/Library/PostProcess.hlsl"
	#include "BlurFilters.hlsl"
	
    
	//Kawase
	half4 fragKawase(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half3 sum = 0;
		sum += SampleMainBlur(i.uv , float2(0, 1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(1,0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, -1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
		return RecordBlurTex(sum*.25);
	}

	//Dual VH Kawase
	half4 fragAverageBlurHorizontal(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half3 sum = SampleMainBlur(i.uv,0);
		sum += SampleMainBlur(i.uv , float2(1, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(2, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-2, 0)*uvDelta);
		return RecordBlurTex(sum*.2);
	}
	half4 fragAverageBlurVertical(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half3 sum = SampleMainBlur(i.uv,0);
		sum += SampleMainBlur(i.uv , float2(0,1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, -1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, 2)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, -2)*uvDelta);
		return RecordBlurTex(sum*.2);
	}
		
	//Dual VH Gaussian
	static const half gaussianWeight3[3] = {0.4026h,0.2442h,0.0545h};
	half4 fragGaussianBlurHorizontal(v2f_img i) :SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half3 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
		sum += SampleMainBlur(i.uv , float2(1,0)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(-1,0)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(2,0)*uvDelta)*gaussianWeight3[2];
		sum += SampleMainBlur(i.uv , float2(-2,0)*uvDelta)*gaussianWeight3[2];
		return RecordBlurTex(sum);
	}
	half4 fragGaussianBlurVertical(v2f_img i) :SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half3 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
		sum += SampleMainBlur(i.uv , float2(0,1)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(0,-1)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(0,2)*uvDelta)*gaussianWeight3[2];
		sum += SampleMainBlur(i.uv , float2(0,-2)*uvDelta)*gaussianWeight3[2];
		return RecordBlurTex(sum);
	}
	
	//Dual Filtering
	half4 fragDualFilteringDownSample(v2f_img i):SV_TARGET
	{
		return RecordBlurTex(DualFilteringDownFilter(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,_MainTex_TexelSize,_BlurSize));
	}

	half4 fragDualFilteringUpSample(v2f_img i):SV_TARGET
	{
		return RecordBlurTex(DualFilteringUpFilter(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,_MainTex_TexelSize,_BlurSize));
	}
	
	//Grainy
	half4 fragGrainy(v2f_img i):SV_TARGET
	{
		float2 delta=_MainTex_TexelSize.xy*_BlurSize;
		float2 randomUV=randomUnitCircle(i.uv)*random01(i.uv)*delta;
		return float4(SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex), i.uv,randomUV),1);
	}

	//Bokeh
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
			float4 bokeh=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+(r-1.0)*rotate*_MainTex_TexelSize.xy);
			sum+=bokeh;
		}
		return sum/_Iteration;
	}

	//Hexagon Blur
	half3 HexagonBlurTexture(TEXTURE2D_PARAM(_tex,_samp),float2 uv,float2 direction)
	{
		float3 finalCol=0;
		for(uint i=0u;i<_Iteration;i++)
		{
			half3 hexagonBlur = SampleBlurTex(TEXTURE2D_ARGS(_tex,_samp),uv,direction*float2(i+.5,i+.5));
			finalCol+=hexagonBlur;
		}
		return finalCol/_Iteration;
	}
	
	half4 fragHexagonVertical(v2f_img i):SV_TARGET
	{
		half angle=_Angle-HALF_PI;
		half sinA,cosA;
		sincos(angle,sinA,cosA);
		half2 dir=half2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		return RecordBlurTex(HexagonBlurTexture(TEXTURE2D_ARGS( _MainTex,sampler_MainTex),i.uv,dir));
	}

	TEXTURE2D(_Hexagon_Vertical);SAMPLER(sampler_Hexagon_Vertical);
	half4 fragHexagonDiagonal(v2f_img i):SV_TARGET
	{
		half angle=_Angle+PI*0.16666h;
		half sinA,cosA;
		sincos(angle,sinA,cosA);
		float2 dir=float2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		return RecordBlurTex(SampleBlurTex(TEXTURE2D_ARGS(_Hexagon_Vertical,sampler_Hexagon_Vertical), i.uv,0)+HexagonBlurTexture(TEXTURE2D_ARGS( _MainTex,sampler_MainTex),i.uv,dir)/2);
	}

	TEXTURE2D( _Hexagon_Diagonal);SAMPLER(sampler_Hexagon_Diagonal);
	half4 fragHexagonRamboid(v2f_img i):SV_TARGET
	{
		half sinA,cosA;
		sincos(_Angle+PI*0.16666h,sinA,cosA);
		float2 verticalBlurDirection=float2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		half3 vertical=HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Vertical,sampler_Hexagon_Vertical),i.uv,verticalBlurDirection);
		sincos(_Angle+PI*0.833333h,sinA,cosA);
		float2 diagonalBlurDirection=float2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		half3 diagonal=HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Diagonal,sampler_Hexagon_Diagonal),i.uv,diagonalBlurDirection);
		return RecordBlurTex(vertical+diagonal*2.0h)*0.3333h;
	}

	//Radial
	half4 fragRadial(v2f_img i):SV_TARGET
	{
		float2 offset=(_Vector-i.uv)*_BlurSize*_MainTex_TexelSize.xy;
		float4 sum=0;
		float2 sumOffset=0;
		for(uint j=0;j<_Iteration;j++)
		{
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+sumOffset);
			sumOffset+=offset;
		}
		return sum/_Iteration;
	}
	//Directional
	half4 fragDirectional(v2f_img i):SV_TARGET
	{
		half4 sum=0;
		int iteration=max(_Iteration/2,1);
		float2 offset=_Vector*_MainTex_TexelSize.xy*_BlurSize;
		for(int j=-iteration;j<iteration;j++)
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+j*offset);
		return sum/(iteration*2);
	}
    
	//Dual Filtering
	half4 fragNextGenDownSample(v2f_img i):SV_TARGET
	{
		return RecordBlurTex( DualFilteringDownFilter(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,_MainTex_TexelSize,_BlurSize));
	}

    TEXTURE2D(_PreDownSample);
    SAMPLER(sampler_PreDownSample);
    float4 _PreDownSample_TexelSize;
	half4 fragNextGenUpSample(v2f_img i):SV_TARGET
	{
		half3 srcCol = DualFilteringUpFilter(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,_MainTex_TexelSize,_BlurSize);
		half3 mipCol = DualFilteringUpFilter(TEXTURE2D_ARGS(_PreDownSample,sampler_PreDownSample),i.uv,_PreDownSample_TexelSize,_BlurSize);
		
		return RecordBlurTex(srcCol+mipCol);
	}

    half4 fragNextGenUpSampleFinal(v2f_img i):SV_TARGET
	{
		return RecordBlurTex(DualFilteringUpFilter(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,_MainTex_TexelSize,_BlurSize));
	}
	ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Cull Off

		Pass
		{
			NAME "KAWASE_BLUR"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragKawase
			ENDHLSL
		}

		Pass
		{
			NAME "AVERAGE_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragAverageBlurHorizontal
			ENDHLSL
		}
		
		Pass
		{
			NAME "AVERAGE_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragAverageBlurVertical
			ENDHLSL
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGaussianBlurHorizontal
			ENDHLSL
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGaussianBlurVertical
			ENDHLSL
		}
		
		Pass
		{
			Name "DUALFILTERING_DOWNSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragDualFilteringDownSample
			ENDHLSL
		}

		Pass
		{
			Name "DUALFILTERING_UPSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragDualFilteringUpSample
			ENDHLSL
		}
		
		Pass
		{
			Name "GRAINY"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGrainy
			ENDHLSL
		}
		
		Pass
		{
			Name "BOKEH"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragBokeh
			ENDHLSL
		}

		Pass
		{
			Name "HEXAGON_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonVertical
			ENDHLSL
		}
		Pass
		{
			Name "HEXAGON_DIAGONAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonDiagonal
			ENDHLSL
		}

		Pass 
		{
			Name "HEXAGON_RHOMBOID"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonRamboid
			ENDHLSL
		}

		pass
		{
			Name "Radial"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRadial
			ENDHLSL
		}
		Pass
		{
			Name "Directional"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragDirectional
			ENDHLSL
		}
    	
    	Pass
		{
			Name "NEXTGEN_DOWNSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragNextGenDownSample
			ENDHLSL
		}

		Pass
		{
			Name "NEXTGEN_UPSAMPLE"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragNextGenUpSample
			ENDHLSL
		}
		
		Pass
		{
			Name "NEXTGEN_UPSAMPLE_FINAL"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragNextGenUpSampleFinal
			ENDHLSL
		}
		
	}
}