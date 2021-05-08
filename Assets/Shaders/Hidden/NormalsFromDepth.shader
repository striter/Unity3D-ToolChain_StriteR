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
            #include "../CommonInclude.hlsl"
            #include "../CameraEffectInclude.hlsl"

            float4 frag (v2f_img i) : SV_Target
            {
                //To Be Continued
                return float4(ClipSpaceNormalFromDepth(i.uv),1);
            }
            ENDHLSL
        }
    }
}
