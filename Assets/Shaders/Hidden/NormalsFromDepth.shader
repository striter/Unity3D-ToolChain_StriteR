Shader "Hidden/NormalsFromDepth"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        ZWrite Off ZTest Always Cull Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #define IDEPTH
            #include "Assets/Shaders/Library/PostProcess.hlsl"

            half3 WorldSpaceNormalFromDepth(float2 uv)
            {
                float3 position = TransformNDCToWorld(uv);
                float3 position1=TransformNDCToWorld(uv+_MainTex_TexelSize.xy*uint2(1,0));
                float3 position2=TransformNDCToWorld(uv+_MainTex_TexelSize.xy*uint2(0,1));
                return normalize(cross(position2-position,position1-position));
            }
            
            // half3 ClipSpaceNormalFromDepth(float2 uv) 
            // {
            //     half depth = SampleEyeDepth(uv);
            //     half depth1 = SampleEyeDepth(uv + _MainTex_TexelRight);
            //     half depth2 = SampleEyeDepth(uv + _MainTex_TexelUp);
				        //     
            //     half3 p1 = half3(_MainTex_TexelRight, depth1 - depth);
            //     half3 p2 = half3(_MainTex_TexelUp, depth2 - depth);
            //     return normalize(cross(p1, p2));
            // }

            float4 frag (v2f_img i) : SV_Target
            {
                half3 normal=WorldSpaceNormalFromDepth(i.uv);
                normal=normal*.5h+.5;
                return float4(normal,1);
            }
            ENDHLSL
        }
    }
}
