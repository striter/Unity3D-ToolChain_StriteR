Shader "Hidden/ImageEffect_Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

	
    HLSLINCLUDE
	#include "../CommonInclude.hlsl"
	#include "CameraEffectInclude.hlsl"
	half _BlurSize;
	int _Iteration;
	float _Angle;
	float2 _Vector;

	struct v2fc
	{
		half4 vertex : SV_POSITION;
		half2 uv: TEXCOORD0;
		half4 uvOffsetA:TEXCOORD1;
		half4 uvOffsetB:TEXCOORD2;
	};

	//Kawase
	v2fc vertKawase(a2v_img v)
	{
		v2fc o;
		o.vertex = TransformObjectToHClip(v.positionOS);
		half2 uv = v.uv;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(1,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, -1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-1, 0)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
	
	half4 fragKawase(v2fc i):SV_TARGET
	{
		half4 sum = 0;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.zw);
		return sum*.25;
	}

	//Dual 
	v2fc vertDualPassHorizontal(a2v_img v)
	{
		v2fc o;
		o.vertex = TransformObjectToHClip(v.positionOS);
		half2 uv = v.uv;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(1, 0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(-1,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(2, 0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-2, 0)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
		
	v2fc vertDualPassVertical(a2v_img v)
	{
		v2fc o;
		o.vertex = TransformObjectToHClip(v.positionOS);
		half2 uv = v.uv;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(0, -1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, 2)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(0, -2)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}

	half4 fragAverageBlur(v2fc i):SV_TARGET
	{
		half4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.zw);
		return sum*.2;
	}
		
	static const half gaussianWeight[3] = {0.4026h,0.2442h,0.0545h};
	half4 fragGaussianBlur(v2fc i) :SV_TARGET
	{
		half4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*gaussianWeight[0];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.xy)*gaussianWeight[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.zw)*gaussianWeight[1];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.xy)*gaussianWeight[2];
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.zw)*gaussianWeight[2];
		return sum;
	}
	
	//Dual Filtering
	struct v2fdfd
	{
		half4 vertex : SV_POSITION;
		half2 uv: TEXCOORD0;
		half4 uvOffsetA:TEXCOORD1;
		half4 uvOffsetB:TEXCOORD2;
	};
	
	
	v2fdfd vertDualFilteringDownSample(a2v_img v)
	{
		v2fdfd o;
		o.vertex = TransformObjectToHClip(v.positionOS);
		half2 uv = v.uv;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(1,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, -1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-1, 0)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
	
	float4 fragDualFilteringDownSample(v2fdfd i):SV_TARGET
	{
	
		half4 sum = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*4;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.zw);
		return sum*.125;
	}

	struct v2fdfu
	{
		half4 vertex : SV_POSITION;
		half4 uvOffsetA:TEXCOORD0;
		half4 uvOffsetB:TEXCOORD1;
		half4 uvOffsetC:TEXCOORD2;
		half4 uvOffsetD:TEXCOORD3;
	};

	v2fdfu vertDualFilteringUpSample(a2v_img v)
	{
		v2fdfu o;
		o.vertex = TransformObjectToHClip(v.positionOS);
		half2 uv = v.uv;
		o.uvOffsetA.xy = uv + half2(0, 2)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(2,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, -2)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-2, 0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetC.xy = uv + half2(1, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetC.zw = uv + half2(1,-1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetD.xy = uv + half2(-1, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetD.zw = uv + half2(-1, -1)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
	
	float4 fragDualFilteringUpSample(v2fdfu i):SV_TARGET
	{
		half4 sum =0;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetA.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetB.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetC.xy)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetC.zw)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetD.xy)*2;
		sum += SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uvOffsetD.zw)*2;
		return sum*.08333;
	}
	
	//Grainy
	half4 fragGrainy(v2f_img i):SV_TARGET
	{
		half random=random2(i.uv);
		half4 sum=0;
		float randomSum=1.0/_Iteration;
		for(int index=0;index<_Iteration;index++)
		{
			float2 randomUV=float2(random2(random*randomSum*index),random2(random*randomSum*(_Iteration-index)))-.5;
			randomUV*=_MainTex_TexelSize.xy*_BlurSize;
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+randomUV);
		}
		return sum/_Iteration;
	}
	
	//Bokeh
	#define _GOLDENANGLE 2.39996
	float4 fragBokeh(v2f_img i):SV_TARGET
	{
		half2x2 rot=Rotate2x2(_GOLDENANGLE);
		half2 rotate=float2(0,_BlurSize);
		rotate=mul(Rotate2x2(_Angle),rotate);
		half4 sum=0;
		half r=1;
		for(int j=0;j<_Iteration;j++)
		{
			r+=1.0/r;
			rotate=mul(rot,rotate);
			half4 bokeh=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+(r-1.0)*rotate*_MainTex_TexelSize.xy);
			sum+=bokeh;
		}
		return sum/_Iteration;
	}

	//Hexagon Blur
	float4 HexagonBlurTexture(Texture2D tex,sampler samp,float2 uv,float2 direction)
	{
		float4 finalCol=0;
		for(int i=0;i<_Iteration;i++)
		{
			half4 hexagonBlur=SAMPLE_TEXTURE2D(tex,samp,uv+direction*float2(i+.5,i+.5));
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
		for(int j=0;j<_Iteration;j++)
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
		int iteration=_Iteration/2u;
		float2 offset=_Vector*_MainTex_TexelSize.xy*_BlurSize;
		for(int j=-iteration;j<iteration;j++)
		{
			sum+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+j*offset);
		}
		return sum/(_Iteration);
	}
	ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Cull Off
		Pass
		{
			NAME "KAWASE_BLUR"
			HLSLPROGRAM
			#pragma vertex vertKawase
			#pragma fragment fragKawase
			ENDHLSL
		}

		Pass
		{
			NAME "AVERAGE_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vertDualPassHorizontal
			#pragma fragment fragAverageBlur
			ENDHLSL
		}
		
		Pass
		{
			NAME "AVERAGE_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vertDualPassVertical
			#pragma fragment fragAverageBlur
			ENDHLSL
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_HORIZONTAL"
			HLSLPROGRAM
			#pragma vertex vertDualPassHorizontal
			#pragma fragment fragGaussianBlur
			ENDHLSL
		}

		Pass		//Vert Blur
		{
			NAME "GAUSSIAN_BLUR_VERTICAL"
			HLSLPROGRAM
			#pragma vertex vertDualPassVertical
			#pragma fragment fragGaussianBlur
			ENDHLSL
		}
		
		Pass
		{
			Name "DUALFILTERING_DOWNSAMPLE"
			HLSLPROGRAM
			#pragma vertex vertDualFilteringDownSample
			#pragma fragment fragDualFilteringDownSample
			ENDHLSL
		}

		Pass
		{
			Name "DUALFILTERING_UPSAMPLE"
			HLSLPROGRAM
			#pragma vertex vertDualFilteringUpSample
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
			Name "Bokeh"
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