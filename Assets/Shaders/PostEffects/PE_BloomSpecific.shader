Shader "Hidden/PostEffect/PE_BloomSpecific"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_RenderTex("Render",2D) = "white"{}
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	sampler2D _RenderTex;
	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			name "Minus"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			fixed4 frag(v2f_img i) : SV_Target
			{
				fixed4 col =tex2D(_RenderTex,i.uv)- tex2D(_MainTex,i.uv) ;
				return col;
			}
			ENDCG
		}

		Pass
		{		
			name "Mix"
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			fixed4 frag(v2f_img i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex,i.uv) +tex2D(_RenderTex,i.uv);
				return col;
			}
			ENDCG
		}
	}
}
