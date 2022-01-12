Shader "Hidden/PostProcess/Transparent"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
            #define IDEPTH
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #define IGeometryDetection
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #pragma multi_compile_local _ _VOLUMETRICLIGHT
        ENDHLSL
        Pass
        {
            Name "Combine"
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            TEXTURE2D(_Volumetric_Sample);SAMPLER(sampler_Volumetric_Sample);
            #if _VOLUMETRICLIGHT
                half _ColorStrength;
            #endif
            half4 frag(v2f_img i):SV_TARGET
            {
                half3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;

                half3 volumetricSample=SAMPLE_TEXTURE2D(_Volumetric_Sample,sampler_Volumetric_Sample,i.uv).rgb;
                
                #if _VOLUMETRICLIGHT
                    half lightSample=volumetricSample.r;
                    half3 lightCol=_MainLightColor.rgb*_ColorStrength;
                    col+=lightCol*lightSample;
                #endif
                
                return half4( col,1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "SAMPLE"
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag


            #if _VOLUMETRICLIGHT
            #pragma multi_compile_local _ _DITHER
            #include "Assets/Shaders/Library/Lighting.hlsl"
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            
            // #include "Assets/Shaders/Library/Additional/CloudShadow.hlsl"
            // #pragma multi_compile _ _CLOUDSHADOW

            float _LightPow;
            float _LightStrength;
            int _MarchTimes;
            float _MarchDistance;
            half VolumetricLight(float3 positionWS,float3 marchDirWS,float depthDstWS)
            {
                float marchDstWS=min(depthDstWS,_MarchDistance);
                uint marchTimes=min(_MarchTimes,128u);
                float marchDelta=marchDstWS/_MarchDistance*1.0/marchTimes;
                float dstDelta=marchDstWS/marchTimes;
                float3 posDelta=marchDirWS*dstDelta;
                float3 lightDirWS=normalize(_MainLightPosition.xyz);
                half totalAtten=0;
                if(marchDstWS>0)
                {
                    float shadowStrength=GetMainLightShadowParams().x;
                    float curDst=0;
                    for(uint index=0u;index<marchTimes;index++)
                    {
                        float3 samplePos=positionWS;
                        #if _DITHER
                        samplePos+=posDelta*random01(samplePos);
                        #endif
                        float shadowAttenuation=SampleHardShadow(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),TransformWorldToShadowCoord(samplePos).xyz,shadowStrength);
                        // shadowAttenuation*=CloudShadowAttenuation(samplePos,lightDirWS);
                        totalAtten+=marchDelta*shadowAttenuation;
                        positionWS+=posDelta;
                        curDst+=dstDelta;
                    }
                }
                return pow(saturate(totalAtten),_LightPow)*_LightStrength;
            }
            #endif

            
            half4 frag (v2f_img i) : SV_Target
            {
                float3 positionWS=_WorldSpaceCameraPos;
                half3 marchDirWS=normalize( TransformNDCToFrustumCornersRay(i.uv));
                float eyeDepth=SampleEyeDepth(i.uv);
                half light=0;
                #if _VOLUMETRICLIGHT
                    light=VolumetricLight(positionWS,marchDirWS,eyeDepth);
                #endif
                return half4(light,0.h,0.h,1.0h);
            }
            ENDHLSL
        }

    }
}
