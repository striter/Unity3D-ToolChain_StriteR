Shader "Hidden/ImageEffect_Blurs"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

	
    CGINCLUDE
    #include "UnityCG.cginc"
    uniform sampler2D _MainTex;
    uniform half4 _MainTex_TexelSize;
	half _BlurSize;

	struct v2f
	{
		half4 vertex : SV_POSITION;
		half2 uv: TEXCOORD0;
		half4 uvOffsetA:TEXCOORD1;
		half4 uvOffsetB:TEXCOORD2;
	};
		

	v2f vertSinglePass(appdata_img v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetA.zw = uv + half2(1,0)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, -1)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-1, 0)*_MainTex_TexelSize.x *_BlurSize;
		return o;
	}

	v2f vertDualPassHorizontal(appdata_img v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(1, 0)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetA.zw = uv + half2(-1,0)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetB.xy = uv + half2(2, 0)*_MainTex_TexelSize.x *_BlurSize;
		o.uvOffsetB.zw = uv + half2(-2, 0)*_MainTex_TexelSize.x *_BlurSize;
		return o;
	}
		
	v2f vertDualPassVertical(appdata_img v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv + half2(0, 1)*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetA.zw = uv + half2(0, -1)*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetB.xy = uv + half2(0, 2)*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetB.zw = uv + half2(0, -2)*_MainTex_TexelSize.y *_BlurSize;
		return o;
	}

	half4 fragAverageBlur(v2f i):SV_TARGET
	{
		half4 sum = tex2D(_MainTex,i.uv);
		sum += tex2D(_MainTex,i.uvOffsetA.xy);
		sum += tex2D(_MainTex,i.uvOffsetA.zw);
		sum += tex2D(_MainTex,i.uvOffsetB.xy);
		sum += tex2D(_MainTex,i.uvOffsetB.zw);
		return sum/=5;
	}
		
	static const half gaussianWeight[3] = {0.4026h,0.2442h,0.0545h};
	half4 fragGaussianBlur(v2f i) :SV_TARGET
	{
		half4 sum = tex2D(_MainTex,i.uv)*gaussianWeight[0];
		sum += tex2D(_MainTex,i.uvOffsetA.xy)*gaussianWeight[1];
		sum += tex2D(_MainTex,i.uvOffsetA.zw)*gaussianWeight[1];
		sum += tex2D(_MainTex,i.uvOffsetB.xy)*gaussianWeight[2];
		sum += tex2D(_MainTex,i.uvOffsetB.zw)*gaussianWeight[2];
		return sum;
	}
	ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
		//0
		Pass
		{
			NAME "AVERAGE_BLUR_SINGLEPASS"
			CGPROGRAM
			#pragma vertex vertSinglePass
			#pragma fragment fragAverageBlur
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

	}
}

//Deprecated
//         uniform half _BlurSize;
//         // weight curves
//         static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };  // gauss'ish blur weights

//       static const half4 curve4[7] = 
//       { 
//          half4(0.0205,0.0205,0.0205,0),
//          half4(0.0855,0.0855,0.0855,0),
//          half4(0.232,0.232,0.232,0),
//          half4(0.324,0.324,0.324,1),
//          half4(0.232,0.232,0.232,0),
//          half4(0.0855,0.0855,0.0855,0),
//          half4(0.0205,0.0205,0.0205,0) 
//       };

//       static const half4 curveCross7[4] =
//       {
//          half4(0.01025,0.01025,0.01025,0),
//          half4(0.04275,0.04275,0.04275,0),
//          half4(0.116,0.116,0.116,0),
//          half4(0.324,0.324,0.324,1),
//       };

//       static const half4 curveCross5[3] =
//       {
//          half4(0.02245,0.02245,0.02245,0),
//          half4(0.1214,0.1214,0.1214,0),
//          half4(0.4246,0.4246,0.4246,1),
//       };

//       static const half4 curveSingle5[3] =
//       {
//          half4(0.0449,0.0449,0.0449,0),
//          half4(0.2428,0.2428,0.2428,0),
//          half4(0.4246,0.4246,0.4246,1),
//       };

//       struct v2f_withBlurCoords8
//       {
//          half4 pos : SV_POSITION;
//          half4 uv : TEXCOORD0;
//          half2 offs : TEXCOORD1;
//       };

//       struct v2f_withBlurCoordsSGX7
//       {
//          half4 pos : SV_POSITION;
//          half2 uv : TEXCOORD0;
//          half4 offs[3] : TEXCOORD1;
//       };

//       struct v2f_withBlurCoordsSGX5
//       {
//          half4 pos : SV_POSITION;
//          half2 uv : TEXCOORD0;
//          half4 offs[2] : TEXCOORD1;
//       };

//       struct v2f_withBlurCoordsSGXMerge7
//       {
//          half4 pos : SV_POSITION;
//          half2 uv : TEXCOORD0;
//          half4 offs[6] : TEXCOORD1;
//       };

//       struct v2f_withBlurCoordsSGXMerge5
//       {
//          half4 pos : SV_POSITION;
//          half2 uv : TEXCOORD0;
//          half4 offs[4] : TEXCOORD1;
//       };

//       v2f_withBlurCoords8 vertBlurHorizontal(appdata_img v)
//       {
//          v2f_withBlurCoords8 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = half4(v.texcoord.xy, 1, 1);
//          o.offs = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;

//          return o;
//       }

//       v2f_withBlurCoords8 vertBlurVertical(appdata_img v)
//       {
//          v2f_withBlurCoords8 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = half4(v.texcoord.xy, 1, 1);
//          o.offs = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize;

//          return o;
//       }

//       half4 fragBlur8(v2f_withBlurCoords8 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          half2 netFilterWidth = i.offs;
//          half2 coords = uv - netFilterWidth * 3.0;

//          half4 color = 0;

//          half4 tap;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0?tap:half4(0,0,0,0)) * curve4[0];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0)) * curve4[1];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords); 
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0)) * curve4[2];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0)) * curve4[3];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0)) * curve4[4];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0))* curve4[5];
//          coords += netFilterWidth;
//          tap = tex2D(_MainTex, coords);
//          color += (tap.a>0 ? tap : half4(0, 0, 0, 0)) * curve4[6];
//          return color;
//       }

//       v2f_withBlurCoordsSGX7 vertBlurHorizontalSGX7(appdata_img v)
//       {
//          v2f_withBlurCoordsSGX7 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = v.texcoord.xy;
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 3.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       v2f_withBlurCoordsSGX7 vertBlurVerticalSGX7(appdata_img v)
//       {
//          v2f_withBlurCoordsSGX7 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = half4(v.texcoord.xy, 1, 1);
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) *_BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 3.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       v2f_withBlurCoordsSGX5 vertBlurHorizontalSGX5(appdata_img v)
//       {
//          v2f_withBlurCoordsSGX5 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = v.texcoord.xy;
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 2.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       v2f_withBlurCoordsSGX5 vertBlurVerticalSGX5(appdata_img v)
//       {
//          v2f_withBlurCoordsSGX5 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = half4(v.texcoord.xy, 1, 1);
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 2.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       v2f_withBlurCoordsSGXMerge7 vertBlurSGXMerge7(appdata_img v)
//       {
//          v2f_withBlurCoordsSGXMerge7 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = v.texcoord.xy;
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 3.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize;
//          coords = -netFilterWidth.xyxy * 3.0;

//          o.offs[3] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[4] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[5] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       v2f_withBlurCoordsSGXMerge5 vertBlurSGXMerge5(appdata_img v)
//       {
//          v2f_withBlurCoordsSGXMerge5 o;
//          o.pos = UnityObjectToClipPos(v.vertex);

//          o.uv = v.texcoord.xy;
//          half2 netFilterWidth = _MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize;
//          half4 coords = -netFilterWidth.xyxy * 2.0;

//          o.offs[0] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[1] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          netFilterWidth = _MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize;
//          coords = -netFilterWidth.xyxy * 2.0;

//          o.offs[2] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);
//          coords += netFilterWidth.xyxy;
//          o.offs[3] = v.texcoord.xyxy + coords * half4(1.0h, 1.0h, -1.0h, -1.0h);

//          return o;
//       }

//       half4 fragBlurSGXClipZeroAlpha(v2f_withBlurCoordsSGX7 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          half4 color = tex2D(_MainTex, i.uv) * curve4[3];
//          color = color.a > 0 ? color : half4(0, 0, 0, 0);
//          half4 tapA, tapB;
//          tapA = tex2D(_MainTex, i.offs[0].xy);
//          tapB = tex2D(_MainTex, i.offs[0].zw);
//          color += ((tapA.a>0?tapA: half4(0, 0, 0, 0)) + (tapB.a>0?tapB: half4(0, 0, 0, 0))) * curve4[0];
//          tapA = tex2D(_MainTex, i.offs[1].xy);
//          tapB = tex2D(_MainTex, i.offs[1].zw);
//          color += ((tapA.a>0?tapA: half4(0, 0, 0, 0)) + (tapB.a>0?tapB: half4(0, 0, 0, 0))) * curve4[1];
//          tapA = tex2D(_MainTex, i.offs[2].xy);
//          tapB = tex2D(_MainTex, i.offs[2].zw);
//          color += ((tapA.a>0?tapA: half4(0, 0, 0, 0)) + (tapB.a>0?tapB: half4(0, 0, 0, 0))) * curve4[2];
//          return color;
//       }

//       half4 fragBlurSGX7(v2f_withBlurCoordsSGX7 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          half4 color = tex2D(_MainTex, i.uv) * curve4[3];
//          half4 tapA, tapB;
//          tapA = tex2D(_MainTex, i.offs[0].xy);
//          tapB = tex2D(_MainTex, i.offs[0].zw);
//          color += (tapA + tapB) * curve4[0];
//          tapA = tex2D(_MainTex, i.offs[1].xy);
//          tapB = tex2D(_MainTex, i.offs[1].zw);
//          color += (tapA + tapB) * curve4[1];
//          tapA = tex2D(_MainTex, i.offs[2].xy);
//          tapB = tex2D(_MainTex, i.offs[2].zw);
//          color += (tapA + tapB) * curve4[2];
//          return color;
//       }

//       half4 fragBlurSGX5(v2f_withBlurCoordsSGX5 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          half4 color = tex2D(_MainTex, i.uv) * curveSingle5[2];
//          half4 tapA, tapB;
//          tapA = tex2D(_MainTex, i.offs[0].xy);
//          tapB = tex2D(_MainTex, i.offs[0].zw);
//          color += (tapA + tapB) * curveSingle5[0];
//          tapA = tex2D(_MainTex, i.offs[1].xy);
//          tapB = tex2D(_MainTex, i.offs[1].zw);
//          color += (tapA + tapB) * curveSingle5[1];
//          return color;
//       }

//       half4 fragBlurSGXMerge7(v2f_withBlurCoordsSGXMerge7 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          fixed4 color = tex2D(_MainTex, i.uv) * curveCross7[3];
//          half4 tapA, tapB;
//          tapA = tex2D(_MainTex, i.offs[0].xy);
//          tapB = tex2D(_MainTex, i.offs[0].zw);
//          color += (tapA + tapB) * curveCross7[0];
//          tapA = tex2D(_MainTex, i.offs[1].xy);
//          tapB = tex2D(_MainTex, i.offs[1].zw);
//          color += (tapA + tapB) * curveCross7[1];
//          tapA = tex2D(_MainTex, i.offs[2].xy);
//          tapB = tex2D(_MainTex, i.offs[2].zw);
//          color += (tapA + tapB) * curveCross7[2];
//          tapA = tex2D(_MainTex, i.offs[3].xy);
//          tapB = tex2D(_MainTex, i.offs[3].zw);
//          color += (tapA + tapB) * curveCross7[0];
//          tapA = tex2D(_MainTex, i.offs[4].xy);
//          tapB = tex2D(_MainTex, i.offs[4].zw);
//          color += (tapA + tapB) * curveCross7[1];
//          tapA = tex2D(_MainTex, i.offs[5].xy);
//          tapB = tex2D(_MainTex, i.offs[5].zw);
//          color += (tapA + tapB) * curveCross7[2];
//          return color;
//       }

//       half4 fragBlurSGXMerge5(v2f_withBlurCoordsSGXMerge5 i) : SV_Target
//       {
//          half2 uv = i.uv.xy;
//          fixed4 color = tex2D(_MainTex, i.uv) * curveCross5[2];
//          half4 tapA, tapB;
//          tapA = tex2D(_MainTex, i.offs[0].xy);
//          tapB = tex2D(_MainTex, i.offs[0].zw);
//          color += (tapA + tapB) * curveCross5[0];
//          tapA = tex2D(_MainTex, i.offs[1].xy);
//          tapB = tex2D(_MainTex, i.offs[1].zw);
//          color += (tapA + tapB) * curveCross5[1];
//          tapA = tex2D(_MainTex, i.offs[2].xy);
//          tapB = tex2D(_MainTex, i.offs[2].zw);
//          color += (tapA + tapB) * curveCross5[0];
//          tapA = tex2D(_MainTex, i.offs[3].xy);
//          tapB = tex2D(_MainTex, i.offs[3].zw);
//          color += (tapA + tapB) * curveCross5[1];
//          return color;
//       }

//        ENDCG

        
//         // 0
//         Pass{
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurVertical
//#pragma fragment fragBlur8

//         ENDCG
//      }

//         // 1	
//         Pass{
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurHorizontal
//#pragma fragment fragBlur8

//         ENDCG
//      }

//      // alternate blur
//      // 2
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurVerticalSGX7
//#pragma fragment fragBlurSGX7

//         ENDCG
//      }

//      // 3
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurHorizontalSGX7
//#pragma fragment fragBlurSGX7

//         ENDCG
//      }

//      // blur clip zero alpha
//      // 4
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurVerticalSGX7
//#pragma fragment fragBlurSGXClipZeroAlpha

//         ENDCG
//      }

//      // 5
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurHorizontalSGX7
//#pragma fragment fragBlurSGXClipZeroAlpha

//         ENDCG
//      }


//      // 6
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurVerticalSGX5
//#pragma fragment fragBlurSGX5

//         ENDCG
//      }


//      // 7
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurHorizontalSGX5
//#pragma fragment fragBlurSGX5

//         ENDCG
//      }

//      // 8
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurSGXMerge5
//#pragma fragment fragBlurSGXMerge5

//         ENDCG
//      }

//      // 9
//      Pass
//      {
//         ZTest Always
//         Cull Off

//         CGPROGRAM

//#pragma vertex vertBlurSGXMerge7
//#pragma fragment fragBlurSGXMerge7

//         ENDCG
//      }
//    }
