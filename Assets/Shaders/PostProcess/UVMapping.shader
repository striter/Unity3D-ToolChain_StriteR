Shader "Hidden/PostProcess/UVMapping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_blit
            #pragma fragment frag
            
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #include "Library/UVMapping.hlsl"
            #pragma multi_compile_local_fragment _ _VORTEXDISTORT
            float2 _VortexCenter;
            float _VortexStrength;
            
            float2 VortexDistort(float2 uv)
            {
                #if _VORTEXDISTORT
				    float2 dir = uv - _VortexCenter;
				    float2 distort = normalize(dir)*(1 - length(dir))*_VortexStrength;
                    uv+=distort;
                #endif
                
            	return uv;
            }

            #pragma multi_compile_local_fragment _ _PANINI_GENERAIC _PANINI_UNITDISTANCE
            #if _PANINI_GENERAIC || _PANINI_UNITDISTANCE
                #define _PANINI_PROJECTION
            #endif
            float4 _PaniniParams;
            float2 PaniniProjection(float2 uv)
            {
                float2 coords = uv;
                #if defined(_PANINI_PROJECTION)
                    float2 view_pos = (2.0 * coords - 1.0) * _PaniniParams.xy * _PaniniParams.w;
                    #if _PANINI_GENERAIC
                        float2 proj_pos = Panini_Generic(view_pos, _PaniniParams.z);
                    #else
                        float2 proj_pos = Panini_UnitDistance(view_pos);
                    #endif
                    float2 proj_ndc = proj_pos / _PaniniParams.xy;
                    coords = proj_ndc * 0.5 + 0.5;
                #endif
                return coords;
            }

            half4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                uv = PaniniProjection(uv);
                uv = VortexDistort(uv);
                half3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
