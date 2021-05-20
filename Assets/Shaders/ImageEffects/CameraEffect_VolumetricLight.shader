Shader "Hidden/CameraEffect_VolumetricLight"
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
            #include "../CommonInclude.hlsl"
            #include "../CommonLightingInclude.hlsl"
            #include "../BoundingCollision.hlsl"
            #include "../CameraEffectInclude.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma shader_feature_local _DITHER

            float _LightPow;
            float _LightStrength;
            int _MarchTimes;
            float _MarchDistance;

            float4 frag (v2f_img i) : SV_Target
            {
                float3 curPos=_WorldSpaceCameraPos;
                float3 marchDirWS=normalize( GetViewDirWS(i.uv));
                float depthDstWS=LinearEyeDepth(i.uv);
                float marchDstWS=min(depthDstWS,_MarchDistance);
                int marchTimes=min(_MarchTimes,128);
                float marchDelta=marchDstWS/_MarchDistance*1.0/marchTimes;
                float dstDelta=marchDstWS/marchTimes;
                float3 posDelta=marchDirWS*dstDelta;
                float curDst=0;
                float totalAtten=0;

                if(marchDstWS>0)
                {
                    for(int index=0;index<marchTimes;index++)
                    {
                        float3 samplePos=curPos;
                        #if _DITHER
                        samplePos+=posDelta*random01(samplePos);
                        #endif
                        totalAtten+=marchDelta*MainLightRealtimeShadow(float4(TransformWorldToShadowCoord(samplePos).xyz,1));
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
            #include "../CommonInclude.hlsl"
            #include "../CameraEffectInclude.hlsl"
            #pragma vertex vert_img
            #pragma fragment frag
            TEXTURE2D(_VolumetricLight_Sample);SAMPLER(sampler_VolumetricLight_Sample);
            float4 frag(v2f_img i):SV_TARGET
            {
                float3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;
                float sample=SAMPLE_TEXTURE2D(_VolumetricLight_Sample,sampler_VolumetricLight_Sample,i.uv).r;
                col+=_MainLightColor.rgb*sample;
                return float4(col,1);
            }
            ENDHLSL
        }
    }
}
