Shader "Hidden/ImageEffect_Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

	
    CGINCLUDE
    #include "UnityCG.cginc"
	#include "../CommonInclude.hlsl"
    uniform sampler2D _MainTex;
    uniform half4 _MainTex_TexelSize;
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
	v2fc vertKawase(appdata_img v)
	{
		v2fc o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
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
		sum += tex2D(_MainTex,i.uvOffsetA.xy);
		sum += tex2D(_MainTex,i.uvOffsetA.zw);
		sum += tex2D(_MainTex,i.uvOffsetB.xy);
		sum += tex2D(_MainTex,i.uvOffsetB.zw);
		return sum*.25;
	}

	//Dual 
	v2fc vertDualPassHorizontal(appdata_img v)
	{
		v2fc o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(1, 0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(-1,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(2, 0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-2, 0)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
		
	v2fc vertDualPassVertical(appdata_img v)
	{
		v2fc o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(0, -1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, 2)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(0, -2)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}

	half4 fragAverageBlur(v2fc i):SV_TARGET
	{
		half4 sum = tex2D(_MainTex,i.uv);
		sum += tex2D(_MainTex,i.uvOffsetA.xy);
		sum += tex2D(_MainTex,i.uvOffsetA.zw);
		sum += tex2D(_MainTex,i.uvOffsetB.xy);
		sum += tex2D(_MainTex,i.uvOffsetB.zw);
		return sum*.2;
	}
		
	static const half gaussianWeight[3] = {0.4026h,0.2442h,0.0545h};
	half4 fragGaussianBlur(v2fc i) :SV_TARGET
	{
		half4 sum = tex2D(_MainTex,i.uv)*gaussianWeight[0];
		sum += tex2D(_MainTex,i.uvOffsetA.xy)*gaussianWeight[1];
		sum += tex2D(_MainTex,i.uvOffsetA.zw)*gaussianWeight[1];
		sum += tex2D(_MainTex,i.uvOffsetB.xy)*gaussianWeight[2];
		sum += tex2D(_MainTex,i.uvOffsetB.zw)*gaussianWeight[2];
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
	
	
	v2fdfd vertDualFilteringDownSample(appdata_img v)
	{
		v2fdfd o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetA.zw = uv + half2(1,0)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, -1)*_MainTex_TexelSize.xy *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-1, 0)*_MainTex_TexelSize.xy *_BlurSize;
		return o;
	}
	
	float4 fragDualFilteringDownSample(v2fdfd i):SV_TARGET
	{
	
		half4 sum = tex2D(_MainTex,i.uv)*4;
		sum += tex2D(_MainTex,i.uvOffsetA.xy);
		sum += tex2D(_MainTex,i.uvOffsetA.zw);
		sum += tex2D(_MainTex,i.uvOffsetB.xy);
		sum += tex2D(_MainTex,i.uvOffsetB.zw);
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

	v2fdfu vertDualFilteringUpSample(appdata_img v)
	{
		v2fdfu o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
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
		sum += tex2D(_MainTex,i.uvOffsetA.xy);
		sum += tex2D(_MainTex,i.uvOffsetA.zw);
		sum += tex2D(_MainTex,i.uvOffsetB.xy);
		sum += tex2D(_MainTex,i.uvOffsetB.zw);
		sum += tex2D(_MainTex,i.uvOffsetC.xy)*2;
		sum += tex2D(_MainTex,i.uvOffsetC.zw)*2;
		sum += tex2D(_MainTex,i.uvOffsetD.xy)*2;
		sum += tex2D(_MainTex,i.uvOffsetD.zw)*2;
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
			sum+=tex2D(_MainTex,i.uv+randomUV);
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
			half4 bokeh=tex2D(_MainTex,i.uv+(r-1.0)*rotate*_MainTex_TexelSize.xy);
			sum+=bokeh;
		}
		return sum/_Iteration;
	}

	//Hexagon Blur
	float4 HexagonBlurTexture(sampler2D tex,float2 uv,float2 direction)
	{
		float4 finalCol=0;
		for(int i=0;i<_Iteration;i++)
		{
			half4 hexagonBlur=tex2D(tex,uv+direction*(i+.5));
			finalCol+=hexagonBlur;
		}
		return finalCol/_Iteration;
	}
	
	float4 fragHexagonVertical(v2f_img i):SV_TARGET
	{
		float2 dir=float2(cos(_Angle -UNITY_PI/2),sin(_Angle-UNITY_PI/2))*_MainTex_TexelSize.xy*_BlurSize;
		return HexagonBlurTexture(_MainTex,i.uv,dir);
	}
	sampler2D _Hexagon_Vertical;
	float4 fragHexagonDiagonal(v2f_img i):SV_TARGET
	{
		float2 dir=float2(cos(_Angle+UNITY_PI/6),sin(_Angle+UNITY_PI/6))*_MainTex_TexelSize.xy*_BlurSize;
		return tex2D(_Hexagon_Vertical,i.uv)+HexagonBlurTexture(_MainTex,i.uv,dir);
	}
	sampler2D _Hexagon_Diagonal;

	float4 fragHexagonRamboid(v2f_img i):SV_TARGET
	{
		float4 vertical=tex2D(_Hexagon_Vertical,i.uv);
		float2 verticalBlurDirection=float2(cos(_Angle+UNITY_PI/6),sin(_Angle+UNITY_PI/6))*_MainTex_TexelSize.xy*_BlurSize;
		vertical=HexagonBlurTexture(_Hexagon_Vertical,i.uv,verticalBlurDirection);

		float4 diagonal=tex2D(_Hexagon_Diagonal,i.uv);
		float2 diagonalBlurDirection=float2(cos(_Angle+UNITY_PI*5/6),sin(_Angle+UNITY_PI*5/6))*_MainTex_TexelSize.xy*_BlurSize;
		diagonal=HexagonBlurTexture(_Hexagon_Diagonal,i.uv,diagonalBlurDirection);

		return (vertical+diagonal)/2;
	}

	//Radial
	float4 fragRadial(v2f_img i):SV_TARGET
	{
		float2 offset=(_Vector-i.uv)*_BlurSize*_MainTex_TexelSize.xy;
		float4 sum=0;
		for(int j=0;j<_Iteration;j++)
		{
			sum+=tex2D(_MainTex,i.uv);
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
			sum+=tex2D(_MainTex,i.uv+j*offset);
		}
		return sum/(_Iteration);
	}
	ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always Cull Off
		Pass
		{
			NAME "KAWASE_BLUR"
			CGPROGRAM
			#pragma vertex vertKawase
			#pragma fragment fragKawase
			ENDCG
		}

		Pass
		{
			NAME "AVERAGE_BLUR_HORIZONTAL"
			CGPROGRAM
			#pragma vertex vertDualPassHorizontal
			#pragma fragment fragAverageBlur
			ENDCG
		}
		
		Pass
		{
			NAME "AVERAGE_BLUR_VERTICAL"
			CGPROGRAM
			#pragma vertex vertDualPassVertical
			#pragma fragment fragAverageBlur
			ENDCG
		}

		Pass
		{
			NAME "GAUSSIAN_BLUR_HORIZONTAL"
			CGPROGRAM
			#pragma vertex vertDualPassHorizontal
			#pragma fragment fragGaussianBlur
			ENDCG
		}

		Pass		//Vert Blur
		{
			NAME "GAUSSIAN_BLUR_VERTICAL"
			CGPROGRAM
			#pragma vertex vertDualPassVertical
			#pragma fragment fragGaussianBlur
			ENDCG
		}
		
		Pass
		{
			Name "DUALFILTERING_DOWNSAMPLE"
			CGPROGRAM
			#pragma vertex vertDualFilteringDownSample
			#pragma fragment fragDualFilteringDownSample
			ENDCG
		}

		Pass
		{
			Name "DUALFILTERING_UPSAMPLE"
			CGPROGRAM
			#pragma vertex vertDualFilteringUpSample
			#pragma fragment fragDualFilteringUpSample
			ENDCG
		}
		
		Pass
		{
			Name "GRAINY"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragGrainy
			ENDCG
		}

		Pass
		{
			Name "Bokeh"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragBokeh

			ENDCG
		}
		Pass
		{
			Name "HEXAGON_VERTICAL"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonVertical
			ENDCG
		}
		Pass
		{
			Name "HEXAGON_DIAGONAL"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonDiagonal
			ENDCG
		}

		Pass 
		{
			Name "HEXAGON_RHOMBOID"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragHexagonRamboid
			ENDCG
		}

		pass
		{
			Name "Radial"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragRadial
			ENDCG
		}
		Pass
		{
			Name "Directional"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment fragDirectional
			ENDCG
		}

	}
}