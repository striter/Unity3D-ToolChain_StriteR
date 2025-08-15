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
            #include "Assets/Shaders/Library/Common.hlsl"
            struct a2v
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 ndc : TEXCOORD0;
            };

            struct f2o
            {
                float depth : SV_Depth;
                float4 color : SV_Target;
            };
            
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D_FLOAT(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = float4(v.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    o.positionCS.y *= -1;
                #endif
                o.ndc = ComputeNormalizedDeviceCoordinates(o.positionCS.xyz).xy;
                return o;
            }

            f2o frag (v2f i) 
            {
                f2o o;
                o.depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.ndc).r;
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
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 ndc : TEXCOORD0;
            };

            TEXTURE2D(_BaseTex);SAMPLER(sampler_BaseTex);
            TEXTURE2D(_OffScreenParticleTexture);SAMPLER(sampler_OffScreenParticleTexture);
            
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = float4(v.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    o.positionCS.y *= -1;
                #endif
                o.ndc = ComputeNormalizedDeviceCoordinates(o.positionCS.xyz).xy;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_OffScreenParticleTexture,sampler_OffScreenParticleTexture,i.ndc);
            }
            ENDHLSL
        }
    }
}
