Shader "Hidden/ShadowCaster"
{
    SubShader
    {
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			CGPROGRAM
			#pragma multi_compile_shadowcaster
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#include "Lighting.cginc"
				
			struct v2fs
			{
				V2F_SHADOW_CASTER;
			};

			v2fs ShadowVertex(appdata_base v)
			{
				v2fs o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 ShadowFragment(v2fs i) :SV_TARGET
			{
				SHADOW_CASTER_FRAGMENT(i);
			}
			ENDCG
		}
    }
}
