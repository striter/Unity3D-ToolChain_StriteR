Shader "Hidden/PostProcess/Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

	
    HLSLINCLUDE
    #include "../PostProcessInclude.hlsl"
    #pragma target 3.5
	float _BlurSize;
	uint _Iteration;
	float _Angle;
	float2 _Vector;
	static const float gaussianWeight3[3] = {0.4026,0.2442,0.0545};

	//Kawase
	float4 fragKawase(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = 0;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, 1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1,0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, -1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1, 0)*uvDelta);
		return sum*.25;
	}

	//Dual VH Kawase
	float4 fragAverageBlurHorizontal(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1, 0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1, 0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(2, 0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-2, 0)*uvDelta);
		return sum*.2;
	}
	float4 fragAverageBlurVertical(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0,1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, -1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, 2)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, -2)*uvDelta);
		return sum*.2;
	}
		
	//Dual VH Gaussian
	float4 fragGaussianBlurHorizontal(v2f_img i) :SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*gaussianWeight3[0];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1,0)*uvDelta)*gaussianWeight3[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1,0)*uvDelta)*gaussianWeight3[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(2,0)*uvDelta)*gaussianWeight3[2];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-2,0)*uvDelta)*gaussianWeight3[2];
		return sum;
	}
	float4 fragGaussianBlurVertical(v2f_img i) :SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*gaussianWeight3[0];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0,1)*uvDelta)*gaussianWeight3[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0,-1)*uvDelta)*gaussianWeight3[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0,2)*uvDelta)*gaussianWeight3[2];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0,-2)*uvDelta)*gaussianWeight3[2];
		return sum;
	}
	
	//Dual Filtering
	float4 fragDualFilteringDownSample(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy *_BlurSize;
		float4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*4;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, 1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1, 0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, -1)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1, 0)*uvDelta);
		return sum*.125;
	}

	float4 fragDualFilteringUpSample(v2f_img i):SV_TARGET
	{
		float2 uvDelta=_MainTex_TexelSize.xy;
		float4 sum =0;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, 2)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(2,0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(0, -2)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-2, 0)*uvDelta);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1, 1)*uvDelta)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(1, -1)*uvDelta)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1, 1)*uvDelta)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv + float2(-1, -1)*uvDelta)*2;
		return sum*.08333;
	}
	
	//Grainy
	float4 fragGrainy(v2f_img i):SV_TARGET
	{
		float2 delta=_MainTex_TexelSize.xy*_BlurSize;
		float2 randomUV=randomUnitCircle(i.uv)*random01(i.uv)*delta;
		float4 sum=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+randomUV);
		return sum;
	}

	//Bokeh
	#define _GOLDENANGLE 2.39996
	float4 fragBokeh(v2f_img i):SV_TARGET
	{
		float2x2 rot=Rotate2x2(_GOLDENANGLE);
		float2 rotate=float2(0,_BlurSize);
		rotate=mul(Rotate2x2(_Angle),rotate);
		float4 sum=0;
		float r=1;
		for(uint j=0;j<_Iteration;j++)
		{
			r+=1.0/r;
			rotate=mul(rot,rotate);
			float4 bokeh=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+(r-1.0)*rotate*_MainTex_TexelSize.xy);
			sum+=bokeh;
		}
		return sum/_Iteration;
	}

	//Hexagon Blur
	float4 HexagonBlurTexture(Texture2D tex,SamplerState samp,float2 uv,float2 direction)
	{
		float4 finalCol=0;
		for(uint i=0;i<_Iteration;i++)
		{
			float4 hexagonBlur=SAMPLE_TEXTURE2D(tex,samp,uv+direction*float2(i+.5,i+.5));
			finalCol+=hexagonBlur;
		}
		return finalCol/_Iteration;
	}
	
	float4 fragHexagonVertical(v2f_img i):SV_TARGET
	{
		float2 dir=float2(cos(_Angle -PI/2),sin(_Angle-PI/2))*_MainTex_TexelSize.xy*_BlurSize;
		return HexagonBlurTexture(_MainTex,sampler_MainTex,i.uv,dir);
	}

	TEXTURE2D(_Hexagon_Vertical);SAMPLER(sampler_Hexagon_Vertical);
	float4 fragHexagonDiagonal(v2f_img i):SV_TARGET
	{
		float2 dir=float2(cos(_Angle+PI/6),sin(_Angle+PI/6))*_MainTex_TexelSize.xy*_BlurSize;
		return (SAMPLE_TEXTURE2D(_Hexagon_Vertical,sampler_Hexagon_Vertical,i.uv)+HexagonBlurTexture(_MainTex,sampler_MainTex,i.uv,dir))/2;
	}

	TEXTURE2D( _Hexagon_Diagonal);SAMPLER(sampler_Hexagon_Diagonal);
	float4 fragHexagonRamboid(v2f_img i):SV_TARGET
	{
		float2 verticalBlurDirection=float2(cos(_Angle+PI/6),sin(_Angle+PI/6))*_MainTex_TexelSize.xy*_BlurSize;
		float4 vertical=HexagonBlurTexture(_Hexagon_Vertical,sampler_Hexagon_Vertical,i.uv,verticalBlurDirection);

		float2 diagonalBlurDirection=float2(cos(_Angle+PI*5/6),sin(_Angle+PI*5/6))*_MainTex_TexelSize.xy*_BlurSize;
		float4 diagonal=HexagonBlurTexture(_Hexagon_Diagonal,sampler_Hexagon_Diagonal,i.uv,diagonalBlurDirection);
		return (vertical+diagonal*2)/3;
	}

	//Radial
	float4 fragRadial(v2f_img i):SV_TARGET
	{
		float2 offset=(_Vector-i.uv)*_BlurSize*_MainTex_TexelSize.xy;
		float4 sum=0;
		for(uint j=0;j<_Iteration;j++)
		{
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
			i.uv+=offset;
		}
		return sum/_Iteration;
	}
	//Directional
	float4 fragDirectional(v2f_img i):SV_TARGET
	{
		float4 sum=0;
		int iteration=max(_Iteration/2,1);
		float2 offset=_Vector*_MainTex_TexelSize.xy*_BlurSize;
		for(int j=-iteration;j<iteration;j++)
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+j*offset);
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