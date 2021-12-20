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
    #pragma multi_compile_local _ _DOF _DOF_CLIPSKY
    #pragma multi_compile _ _FIRSTBLUR
    #pragma multi_compile _ _FINALBLUR
    #pragma multi_compile_local _ _ENCODE
	#if defined(_DOF)||defined(_DOF_CLIPSKY)
		#define IDEPTH
        half _FocalStart;
        half _FocalEnd;
    #endif
    #define ICOLOR
    #include "Assets/Shaders/Library/PostProcess.hlsl"
	half4 SampleBlurTex(TEXTURE2D_PARAM(_tex,_sampler),float2 uv,float2 offset)
	{
		#if defined(_DOF)||defined(_DOF_CLIPSKY)
			float rawDepth=SampleRawDepth(uv+offset);
    		half focal=saturate(invlerp(_FocalStart,_FocalEnd,RawToEyeDepth(rawDepth)));
			#if _DOF_CLIPSKY
			focal*=DepthLesser(rawDepth,Z_END)?1:0;
			#endif
    		offset*=focal;
    	#endif

	    return SAMPLE_TEXTURE2D(_tex,_sampler,uv+offset);
	}
    
    half4 SampleMainBlur(float2 uv,float2 offset)
    {
		float4 color=SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),uv,offset);
		#if defined(_FIRSTBLUR)||!defined(_ENCODE)
			return color;
		#endif
		color.rgb=DecodeRGBM(color);
		return color;
    }

    half4 RecordBlurTex(float4 _color)
	{
		#if defined(_FINALBLUR)||!defined(_ENCODE)
			return _color;
		#endif
		return EncodeRGBM(_color.rgb);
	}
	
    
	//Kawase
	half4 fragKawase(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half4 sum = 0;
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
		half4 sum = SampleMainBlur(i.uv,0);
		sum += SampleMainBlur(i.uv , float2(1, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(2, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-2, 0)*uvDelta);
		return RecordBlurTex(sum*.2);
	}
	half4 fragAverageBlurVertical(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half4 sum = SampleMainBlur(i.uv,0);
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
		half4 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
		sum += SampleMainBlur(i.uv , float2(1,0)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(-1,0)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(2,0)*uvDelta)*gaussianWeight3[2];
		sum += SampleMainBlur(i.uv , float2(-2,0)*uvDelta)*gaussianWeight3[2];
		return RecordBlurTex(sum);
	}
	half4 fragGaussianBlurVertical(v2f_img i) :SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half4 sum = SampleMainBlur(i.uv,0)*gaussianWeight3[0];
		sum += SampleMainBlur(i.uv , float2(0,1)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(0,-1)*uvDelta)*gaussianWeight3[1];
		sum += SampleMainBlur(i.uv , float2(0,2)*uvDelta)*gaussianWeight3[2];
		sum += SampleMainBlur(i.uv , float2(0,-2)*uvDelta)*gaussianWeight3[2];
		return RecordBlurTex(sum);
	}
	
	//Dual Filtering
	half4 fragDualFilteringDownSample(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half4 sum = SampleMainBlur(i.uv,0)*4;
		sum += SampleMainBlur(i.uv , float2(0, 1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(1, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, -1)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-1, 0)*uvDelta);
		return RecordBlurTex(sum*.125h);
	}

	half4 fragDualFilteringUpSample(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		half4 sum =0;
		sum += SampleMainBlur(i.uv , float2(0, 2)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(2,0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(0, -2)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(-2, 0)*uvDelta);
		sum += SampleMainBlur(i.uv , float2(1, 1)*uvDelta)*2;
		sum += SampleMainBlur(i.uv , float2(1, -1)*uvDelta)*2;
		sum += SampleMainBlur(i.uv , float2(-1, 1)*uvDelta)*2;
		sum += SampleMainBlur(i.uv , float2(-1, -1)*uvDelta)*2;
		return RecordBlurTex(sum*.08333h);
	}
	
	//Grainy
	half4 fragGrainy(v2f_img i):SV_TARGET
	{
		float2 delta=_MainTex_TexelSize.xy*_BlurSize;
		float2 randomUV=randomUnitCircle(i.uv)*random01(i.uv)*delta;
		return SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex), i.uv,randomUV);
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
			float4 bokeh=SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,(r-1.0)*rotate*_MainTex_TexelSize.xy);
			sum+=bokeh;
		}
		return sum/_Iteration;
	}

	//Hexagon Blur
	half4 HexagonBlurTexture(TEXTURE2D_PARAM(_tex,_samp),float2 uv,float2 direction)
	{
		float4 finalCol=0;
		for(uint i=0u;i<_Iteration;i++)
		{
			half4 hexagonBlur= SampleBlurTex(TEXTURE2D_ARGS(_tex,_samp),uv,direction*float2(i+.5,i+.5));
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
		return HexagonBlurTexture(TEXTURE2D_ARGS( _MainTex,sampler_MainTex),i.uv,dir);
	}

	TEXTURE2D(_Hexagon_Vertical);SAMPLER(sampler_Hexagon_Vertical);
	half4 fragHexagonDiagonal(v2f_img i):SV_TARGET
	{
		half angle=_Angle+PI*0.16666h;
		half sinA,cosA;
		sincos(angle,sinA,cosA);
		float2 dir=float2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		return (SAMPLE_TEXTURE2D(_Hexagon_Vertical,sampler_Hexagon_Vertical,i.uv)+HexagonBlurTexture(TEXTURE2D_ARGS( _MainTex,sampler_MainTex),i.uv,dir))/2;
	}

	TEXTURE2D( _Hexagon_Diagonal);SAMPLER(sampler_Hexagon_Diagonal);
	half4 fragHexagonRamboid(v2f_img i):SV_TARGET
	{
		half angle=_Angle+PI*0.16666h;
		half sinA,cosA;
		sincos(angle,sinA,cosA);
		float2 verticalBlurDirection=float2(cos(_Angle+PI/6),sin(_Angle+PI/6))*_MainTex_TexelSize.xy*_BlurSize;
		half4 vertical=HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Vertical,sampler_Hexagon_Vertical),i.uv,verticalBlurDirection);

		angle=_Angle+PI*0.833333h;
		sincos(angle,sinA,cosA);
		float2 diagonalBlurDirection=float2(cosA,sinA)*_MainTex_TexelSize.xy*_BlurSize;
		half4 diagonal=HexagonBlurTexture(TEXTURE2D_ARGS( _Hexagon_Diagonal,sampler_Hexagon_Diagonal),i.uv,diagonalBlurDirection);
		return (vertical+diagonal*2.0h)*0.3333h;
	}

	//Radial
	half4 fragRadial(v2f_img i):SV_TARGET
	{
		float2 offset=(_Vector-i.uv)*_BlurSize*_MainTex_TexelSize.xy;
		float4 sum=0;
		float2 sumOffset=0;
		for(uint j=0;j<_Iteration;j++)
		{
			sum+=SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,sumOffset);
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
			sum+=SampleBlurTex(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.uv,j*offset);
		return sum/(iteration*2);
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
	}
}