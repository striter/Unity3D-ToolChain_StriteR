Shader "Hidden/PostEffect/PE_ViewDepth"
{
	Properties
	{
		[PreRenderData]_MainTex("Main Texture",2D)="white"{}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));

				return fixed4(depth,depth,depth,1) ;
			}
			ENDCG
		}
	}
}
