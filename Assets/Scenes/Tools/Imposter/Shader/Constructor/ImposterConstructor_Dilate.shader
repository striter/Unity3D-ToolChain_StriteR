Shader "Hidden/Imposter_Dilate"
{
	Properties
	{
		_MainTex( "", 2D ) = "white" {}
		_MaskTex( "", 2D ) = "black" {}
	}

	HLSLINCLUDE
		#include "Assets/Shaders/Library/PostProcess.hlsl"
		TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);

		float4 frag_dilate( v2f_img i, bool alpha )
		{
			float2 offsets[ 8 ] =
			{
				float2( -1, -1 ),
				float2(  0, -1 ),
				float2( +1, -1 ),
				float2( -1,  0 ),
				float2( +1,  0 ),
				float2( -1, +1 ),
				float2(  0, +1 ),
				float2( +1, +1 )
			};

			float4 ref_main = SAMPLE_TEXTURE2D( _MainTex,sampler_MainTex, i.uv );
			float ref_mask = max(SAMPLE_TEXTURE2D( _MaskTex,sampler_MaskTex, i.uv));
			float4 result = 0;

			if ( ref_mask == 0 )
			{
				float hits = 0;

				for ( int tap = 0; tap < 8; tap++ )
				{
					float2 uv = i.uv + offsets[ tap ] * _MainTex_TexelSize.xy;
					float4 main = SAMPLE_TEXTURE2D_LOD( _MainTex, sampler_MainTex,uv,0);
					float mask = SAMPLE_TEXTURE2D_LOD( _MaskTex,sampler_MainTex, uv,0).a;

					if ( mask != ref_mask )
					{
						result += main;
						hits++;
					}
				}

				if ( hits > 0 )
				{
					if ( alpha )
					{
						result /= hits;
					}
					else
					{
						result = float4( result.rgb / hits, ref_main.a );
					}
				}
				else
				{
					result = ref_main;
				}
			}
			else
			{
				result = ref_main;
			}

			return result;
		}
	ENDHLSL

	SubShader
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		Pass
		{
			HLSLPROGRAM
				#pragma vertex vert_blit
				#pragma fragment frag

				float4 frag( v2f_img i ) : SV_target
				{
					return frag_dilate( i, false );
				}
			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM
				#pragma vertex vert_blit
				#pragma fragment frag

				float4 frag( v2f_img i ) : SV_target
				{
					return frag_dilate( i, true );
				}
			ENDHLSL
		}
	}
}
