Shader "Hidden/PostProcess/VolumetricLight"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "SAMPLE"
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "../../PostProcessInclude.hlsl"
            #include "../../CommonLightingInclude.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma shader_feature_local _DITHER

            float _LightPow;
            float _LightStrength;
            int _MarchTimes;
            float _MarchDistance;

            half4 frag (v2f_img i) : SV_Target
            {
                float3 curPos=_WorldSpaceCameraPos;
                half3 marchDirWS=normalize( TransformNDCToViewDir(i.uv));
                float depthDstWS=SampleEyeDepth(i.uv);
                float marchDstWS=min(depthDstWS,_MarchDistance);
                uint marchTimes=min(_MarchTimes,128u);
                float marchDelta=marchDstWS/_MarchDistance*1.0/marchTimes;
                float dstDelta=marchDstWS/marchTimes;
                float3 posDelta=marchDirWS*dstDelta;
                half totalAtten=0;

                if(marchDstWS>0)
                {
                    float shadowStrength=GetMainLightShadowParams().x;
                    float curDst=0;
                    [unroll(128u)]
                    for(uint index=0u;index<marchTimes;index++)
                    {
                        float3 samplePos=curPos;
                        #if _DITHER
                        samplePos+=posDelta*random01(samplePos);
                        #endif
                        float shadowAttenuation=SampleHardShadow(_MainLightShadowmapTexture,sampler_MainLightShadowmapTexture,TransformWorldToShadowCoord(samplePos).xyz,shadowStrength);
                        totalAtten+=marchDelta*shadowAttenuation;
                        curPos+=posDelta;
                        curDst+=dstDelta;
                    }
                }
                totalAtten= pow(saturate(totalAtten),_LightPow)*_LightStrength;
                return totalAtten;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Combine"
            HLSLPROGRAM
            #include "../../PostProcessInclude.hlsl"
            #pragma vertex vert_img
            #pragma fragment frag
            TEXTURE2D(_VolumetricLight_Sample);SAMPLER(sampler_VolumetricLight_Sample);
            half _ColorStrength;
            half4 frag(v2f_img i):SV_TARGET
            {
                half3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;
                half sample=SAMPLE_TEXTURE2D(_VolumetricLight_Sample,sampler_VolumetricLight_Sample,i.uv).r;
                half3 lightCol=_MainLightColor.rgb*_ColorStrength;
                col+=lightCol*sample;
                return half4( col,1);
            }
            ENDHLSL
        }
    }
}
