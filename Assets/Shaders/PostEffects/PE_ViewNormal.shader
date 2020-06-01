Shader "Hidden/PostEffect/PE_ViewNormal"
{
	Properties
	{
		[PreRenderData]_MainTex("Main Texture",2D) = "white"{}
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

			sampler2D _CameraDepthNormalsTexture;
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture,i.uv));
				return fixed4(normal*.5+.5,1);
			}
			ENDCG
		}
	}
}
