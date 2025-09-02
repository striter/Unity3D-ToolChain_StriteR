Shader "Unlit/VolumetricLight"
{
    Properties
    {
        [ColorUsage(false,true)]_Color("Color",Color) = (1,1,1,1)
        _Pow("LightPow",Range(0.1,5)) = 1
        [Enum(_4,4,_8,8,_16,16,_32,32,_64,64,_128,128,_256,256)]_MarchTimes("MarchTimes",int)=32
        [Toggle(_DITHER)]_DITHER("Dither",int) = 0
        
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend One One , Zero One
        ZWrite Off
        ZTest Always
        Cull Front
        
        Pass
        {
            Name "SAMPLE"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

		    #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_local _ _DITHER
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            
            // #include "Assets/Shaders/Library/Additional/CloudShadow.hlsl"
            // #pragma multi_compile _ _CLOUDSHADOW

            float _Pow;
            int _MarchTimes;
            float4 _Color;
            struct a2v
            {
                float3 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 centerWS:TEXCOORD1;
                float3 sizeWS:TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.centerWS=TransformObjectToWorld(0);
                o.sizeWS=TransformObjectToWorldDir(1,false);
                o.screenPos=ComputeScreenPos(o.positionCS);
                o.positionWS=TransformObjectToWorld(v.positionOS);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float2 ndc = i.screenPos.xy/i.screenPos.w;
                GRay rayWS = GRay_Ctor(GetCameraRealPositionWS(ndc),GetCameraRealDirectionWS(i.positionWS));
                
                float3 directionOS = TransformWorldToObjectDir(rayWS.direction,false);
                GRay rayOS = GRay_Ctor(TransformWorldToObject(rayWS.origin),normalize(directionOS));
                GBox boxOS = GBox_Ctor(0,1);

                float objectToWorldScaling = length(directionOS);
                float2 rayDistancesOS = Distance(boxOS,rayOS);
                float2 rayDistancesWS = rayDistancesOS / objectToWorldScaling;

                float depthWS = RawToDistance(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, ndc).r,ndc);
                float distanceBegin = rayDistancesWS.x > 0 ? min(rayDistancesWS.x,depthWS) : 0;
                float distanceEnd = min(rayDistancesWS.x + rayDistancesWS.y,depthWS);
                float distanceTravel = distanceEnd - distanceBegin;

                float totalAtten=0;
                if(distanceTravel>0)
                {
                    float shadowStrength=GetMainLightShadowParams().x;
                    float dstDelta = distanceTravel/_MarchTimes;
                    float marchAtten = 1.0 / _MarchTimes;
                    for(uint index=0u;index<_MarchTimes;index++)
                    {
                        float3 samplePos = rayWS.GetPoint(distanceBegin + dstDelta * index);
                        #if _DITHER
                            samplePos += rayWS.direction * dstDelta *random(samplePos);
                        #endif
                        float shadowAttenuation=SampleHardShadow(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),TransformWorldToShadowCoord(samplePos).xyz,shadowStrength);
                        totalAtten += shadowAttenuation * marchAtten;
                    }
                }
                return pow(saturate(totalAtten),_Pow) * _MainLightColor * _Color;
            }
            ENDHLSL
        
            }
       }
}
