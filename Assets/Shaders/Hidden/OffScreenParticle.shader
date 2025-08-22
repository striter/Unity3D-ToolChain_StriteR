Shader "Hidden/OffScreenParticle"
{
    Properties
    {
        [PreRenderData]_BaseTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite On ZTest Always Cull Off
        Pass
        {
            Name "VDM Clear & Depth DownSample"
            Blend Off
            
            HLSLPROGRAM
            #include "Assets/Shaders/Library/PostProcess.hlsl"

            struct f2o
            {
                float depth : SV_Depth;
                float4 color : SV_Target;
            };
            
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag

            #pragma multi_compile _ _MAX_DEPTH
            int _DownSample;

            f2o frag (v2f_img i) 
            {
                f2o o;
                o.depth = Z_NEAR;
                #if _MAX_DEPTH
                    for (int x = 0; x < _DownSample; x++)
                        for (int y = 0; y < _DownSample; y++)
                        {
                            float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.uv + _CameraDepthTexture_TexelSize.xy * int2(x,y)).r;
                            if (DepthGreater(depth,o.depth))
                                o.depth = depth;
                        }
                #else
                    o.depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.uv).r;
                #endif
                o.color = float4(0,0,0,1);
                return o;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "VDM Blend"
            Blend One SrcAlpha
            HLSLPROGRAM
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag

            #include "Assets/Shaders/Library/PostProcess.hlsl"

            TEXTURE2D(_BaseTex);SAMPLER(sampler_BaseTex);
            TEXTURE2D(_OffScreenParticleTexture);SAMPLER(sampler_OffScreenParticleTexture);
            
            float4 frag (v2f_img i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_OffScreenParticleTexture,sampler_OffScreenParticleTexture,i.uv);
            }
            ENDHLSL
        }
    }
}
