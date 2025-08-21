Shader "Hidden/ReflectiveShadowMapSample"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag

            #include "Assets/Shaders/Library/PostProcess.hlsl"
            
            TEXTURE2D(_RSMFlux); SAMPLER(sampler_RSMFlux);
            TEXTURE2D(_RSMNormal); SAMPLER(sampler_RSMNormal);
            TEXTURE2D(_RSMWorldPos); SAMPLER(sampler_RSMWorldPos);
            float4x4 _WorldToShadow;

            int _RandomVectorCount;
            float4 _RandomVectors[1024];
            float4 frag (v2f_img input) : SV_Target
            {
                float3 positionWS = TransformNDCToWorld(input.uv);
                float3 normalWS = WorldSpaceNormalFromDepth(input.uv);
                float4 positionSS = mul(_WorldToShadow,float4(positionWS,1));
                positionSS /= positionSS.w;
                float3 indirect = 0;
                for(int i = 0 ; i < _RandomVectorCount;i++)
                {
                    float3 randomVector = _RandomVectors[i].xyz;
                    float3 samplePosition = positionSS + randomVector;

                    float3 samplerPositionWS = SAMPLE_TEXTURE2D(_RSMWorldPos,sampler_RSMWorldPos,samplePosition.xy).xyz;
                    float3 samplerNormalWS = SAMPLE_TEXTURE2D(_RSMNormal,sampler_RSMNormal,samplePosition.xy).xyz * 2 - 1;
                    float3 sampleFlux = SAMPLE_TEXTURE2D(_RSMFlux,sampler_RSMFlux,samplePosition.xy).xyz;

                    float3 attenuation = _MainLightColor;
                    float3 result = attenuation * sampleFlux *
                            (max(0.0, dot(samplerNormalWS, positionWS - samplerPositionWS)) * max(0.0, dot(normalWS, samplerPositionWS - positionWS)) / pow(length(positionWS - samplerPositionWS), 4.0));
                    indirect += result;
                }

                indirect /= _RandomVectorCount;
                return float4(indirect,1);
            }
            ENDHLSL
        }
    }
}
