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
	half2 _BlurDirection;

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

	v2f vertDirectional(appdata_img v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		half2 uv = v.texcoord;
		o.uv = uv;
		o.uvOffsetA.xy = uv - _BlurDirection*.5*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetA.zw = uv - _BlurDirection*1*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetB.xy = uv - _BlurDirection*1.5*_MainTex_TexelSize.y *_BlurSize;
		o.uvOffsetB.zw = uv - _BlurDirection*2*_MainTex_TexelSize.y *_BlurSize;
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

		Pass
		{
			Name "AVERAGE_DIRECTIONAL"
			CGPROGRAM
			#pragma vertex vertDirectional
			#pragma fragment fragAverageBlur
			ENDCG
		}

		Pass 
		{
			Name "HEXAGON_COMBINE"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			sampler2D _HexagonCell1;
			sampler2D _HexagonCell2;
			sampler2D _HexagonCell3;
			struct v2fc
			{
				float4 vertex:SV_POSITION;
				float2 uv:TEXCOORD0;
			};
			
			v2fc vert(appdata_img v)
			{
				v2fc o;
				o.vertex=UnityObjectToClipPos(v.vertex);
				o.uv=v.texcoord;
				return o;
			}

			float4 frag(v2f_img i):SV_TARGET
			{
				return (tex2D(_HexagonCell1,i.uv)+tex2D(_HexagonCell2,i.uv)+tex2D(_HexagonCell3,i.uv))/3;
			}
			ENDCG
		}
	}
}