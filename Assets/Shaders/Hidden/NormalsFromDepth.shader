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
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag
            #include "Assets/Shaders/Library/PostProcess.hlsl"

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
