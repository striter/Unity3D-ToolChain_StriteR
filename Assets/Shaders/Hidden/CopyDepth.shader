Shader "Hidden/CopyDepth"
{
    SubShader
    {
        ZWrite Off ZTest Always Cull Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "../CommonInclude.hlsl"
            
            struct a2v_img
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f_img
            {
                float4 positionCS : SV_Position;
                float2 uv : TEXCOORD0;
            };

            v2f_img vert_img(a2v_img v)
            {
                v2f_img o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            TEXTURE2D_FLOAT(_CameraDepthAttachment);
            SAMPLER(sampler_CameraDepthAttachment);
            
#if UNITY_REVERSED_Z
    #define DEPTH_DEFAULT_VALUE 1.0
    #define DEPTH_OP min
#else
    #define DEPTH_DEFAULT_VALUE 0.0
    #define DEPTH_OP max
#endif

            float frag (v2f_img i):SV_DEPTH
            {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthAttachment,sampler_CameraDepthAttachment,i.uv).r;
            }
            ENDHLSL
        }
    }
}
