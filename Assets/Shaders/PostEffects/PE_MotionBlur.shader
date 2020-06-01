Shader "Hidden/PostEffect/PE_MotionBlur"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_BlurAMount("Blur Amount",Range(0,.8))=.8
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		CGINCLUDE
			#include "UnityCG.cginc"

			sampler2D _MainTex;
	fixed _BlurAmount;
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			v2f vert(appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}


			fixed4 fragRGB(v2f i):SV_TARGET
			{
			return fixed4(tex2D(_MainTex,i.uv).rgb,_BlurAmount);
			}

				fixed4 fragA(v2f i):SV_TARGET
			{
			return tex2D(_MainTex,i.uv);
			}
		ENDCG
		Pass
		{
				Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragRGB
			ENDCG
		}
			Pass
			{
			Blend One Zero
			ColorMask A
				CGPROGRAM
#pragma vertex vert
#pragma fragment fragA
				ENDCG
			}
	}
}
