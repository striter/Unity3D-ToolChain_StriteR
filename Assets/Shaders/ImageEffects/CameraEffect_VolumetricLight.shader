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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "../CommonInclude.hlsl"
            #include "../CommonLightingInclude.hlsl"
            #include "../BoundingCollision.hlsl"
            #include "CameraEffectInclude.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            float _LightPow;
            float _LightStrength;
            int _MarchTimes;
            float _MarchDistance;

            struct appdata
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDirWS:TEXCOORD1;
                float3 planeWS:TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.viewDirWS=GetInterpolatedRay(v.uv);
                o.planeWS=TransformObjectToWorldNormal(float3(0,0,1));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 curPos=_WorldSpaceCameraPos;
                float3 marchDirWS=normalize(i.viewDirWS);
                float depthDstWS=LinearEyeDepth(i.uv);
                float marchDstWS=min(depthDstWS,_MarchDistance);
                float marchDelta=1.0/_MarchTimes;
                float dstDelta=_MarchDistance/_MarchTimes;
                float3 posDelta=marchDirWS*dstDelta;
                float curDst=0;
                float totalAtten=0;

                if(marchDstWS>0)
                {
                    for(int index=0;index<_MarchTimes;index++)
                    {
                        if(curDst>=marchDstWS)
                            break;

                        totalAtten+=marchDelta*MainLightRealtimeShadow(TransformWorldToShadowCoord(curPos));
                        curPos+=posDelta;
                        curDst+=dstDelta;
                    }
                }
                totalAtten=pow(totalAtten,_LightPow)*_LightStrength;
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv)+_MainLightColor*totalAtten;
            }
            ENDHLSL
        }
    }
}
