Shader "Hidden/MotionVectorCamera"
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
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #pragma vertex vert_img_procedural
            #pragma fragment frag

            float4 frag(v2f_img i) : SV_TARGET
            {
                float3 positionWS = TransformNDCToWorld(i.uv);
                float4 positionHClip = mul(_Matrix_VP,float4(positionWS,1));
                float4 prePositionHClip = mul(_Matrix_VP_Pre,float4(positionWS,1));
                float2 curUV = TransformHClipToNDC(positionHClip);
                float2 preUV = TransformHClipToNDC(prePositionHClip);
                float2 delta = curUV - preUV;
                return half4(delta,0,1);
            }
            ENDHLSL        
        }
    }
}