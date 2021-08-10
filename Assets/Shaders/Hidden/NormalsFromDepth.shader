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

            float4 frag (v2f_img i) : SV_Target
            {
                //To Be Continued
                float3 positionWS;
                float depth;
                return float4(WorldSpaceNormalFromDepth(i.uv,positionWS,depth),1);
            }
            ENDHLSL
        }
    }
}
