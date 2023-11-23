Shader "Game/Lit/CustomGI"
{
    Properties
	{
    }
    SubShader
    {
		Pass
		{
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 uv : TEXCOORD0;
                float3 normalOS:NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
            	float3 normalWS:NORMAL;
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
            	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            SHL2Input();
            float4 frag (v2f i) : SV_Target
            {
				return float4(SHL2Sample(i.normalWS,),1);
            }

			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}
    }
}
