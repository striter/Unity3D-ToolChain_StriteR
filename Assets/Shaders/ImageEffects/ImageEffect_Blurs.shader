Shader "Hidden/ImageEffect_Blurs"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }

	
    CGINCLUDE
    #include "UnityCG.cginc"
    uniform sampler2D _MainTex;
    uniform half4 _MainTex_TexelSize;
	half _BlurSize;

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


	//Dual Pass
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

	//Hexagon Blur
	uint _HexagonIteration;
	float _HexagonAngle;
	float4 HexagonBlurTexture(sampler2D tex,float2 uv,float2 direction)
	{
		float4 finalCol=0;
		float amount=0;
		for(uint i=0;i<_HexagonIteration;i++)
			finalCol+=tex2D(tex,uv+direction*(i+.5));
		return finalCol/_HexagonIteration;
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

	ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
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
			Name "HEXAGON_VERTICAL"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			float4 frag(v2f_img i):SV_TARGET
			{
				float2 dir=float2(cos(_HexagonAngle -UNITY_PI/2),sin(_HexagonAngle-UNITY_PI/2))*_MainTex_TexelSize.xy*_BlurSize;
				return HexagonBlurTexture(_MainTex,i.uv,dir);
			}
			ENDCG
		}
		Pass
		{
			Name "HEXAGON_DIAGONAL"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			sampler2D _Hexagon_Vertical;
			float4 frag(v2f_img i):SV_TARGET
			{
				float2 dir=float2(cos(_HexagonAngle+UNITY_PI/6),sin(_HexagonAngle+UNITY_PI/6))*_MainTex_TexelSize.xy*_BlurSize;
				return tex2D(_Hexagon_Vertical,i.uv)+HexagonBlurTexture(_MainTex,i.uv,dir);
			}
			ENDCG
		}

		Pass 
		{
			Name "HEXAGON_RHOMBOID"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			sampler2D _Hexagon_Vertical;
			sampler2D _Hexagon_Diagonal;

			float4 frag(v2f_img i):SV_TARGET
			{
				float4 vertical=tex2D(_Hexagon_Vertical,i.uv);
				float2 verticalBlurDirection=float2(cos(_HexagonAngle+UNITY_PI/6),sin(_HexagonAngle+UNITY_PI/6))*_MainTex_TexelSize.xy*_BlurSize;
				vertical=HexagonBlurTexture(_Hexagon_Vertical,i.uv,verticalBlurDirection);

				float4 diagonal=tex2D(_Hexagon_Diagonal,i.uv);
				float2 diagonalBlurDirection=float2(cos(_HexagonAngle+UNITY_PI*5/6),sin(_HexagonAngle+UNITY_PI*5/6))*_MainTex_TexelSize.xy*_BlurSize;
				diagonal=HexagonBlurTexture(_Hexagon_Diagonal,i.uv,diagonalBlurDirection);

				return (vertical+diagonal)/2;
			}
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
	}
}