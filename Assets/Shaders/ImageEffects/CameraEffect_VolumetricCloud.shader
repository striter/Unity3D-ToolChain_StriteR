Shader "Hidden/CameraEffect_VolumetricCloud"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "../CommonInclude.hlsl"
            #include "../GeometryInclude.hlsl"
            #include "../CameraEffectInclude.hlsl"

            #pragma shader_feature_local _LIGHTMARCH
            #pragma shader_feature_local _LIGHTSCATTER
            #pragma shader_feature_local _SHAPEMASK

            float _VerticalStart;
            float _VerticalEnd;
            
            int _RayMarchTimes;
            float _Distance;
            float _Density;
            float _DensityClip;
            float _DensitySmooth;
            float _Opacity;
            
            float _ScatterRange;
            float _ScatterStrength;

            float _LightAbsorption;
            float _LightMarchMinimalDistance;
            int _LightMarchTimes;
            sampler2D _ColorRamp;

            sampler3D _MainNoise;
            float3 _MainNoiseScale;
            float3 _MainNoiseFlow;
            sampler2D _ShapeMask;
            float2 _ShapeMaskScale;
            float2 _ShapeMaskFlow;

            float SampleDensity(float3 worldPos)  {
                float densityParam= saturate(min(abs(worldPos.y-_VerticalStart)/_DensitySmooth,abs(worldPos.y-_VerticalEnd)/_DensitySmooth));
                #if _SHAPEMASK
                float mask=tex2Dlod(_ShapeMask,float4(worldPos.xz/_ShapeMaskScale+_Time.y*_ShapeMaskFlow,0,0)).r;
                densityParam*=mask;
                #endif
                return  smoothstep(_DensityClip,1 , tex3Dlod(_MainNoise,float4( worldPos/_MainNoiseScale+_MainNoiseFlow*_Time.y,0)).r)*_Density*densityParam;
            }

            #if _LIGHTMARCH
            float lightMarch(GPlane _planeStart,GPlane _planeEnd, GRay lightRay,float marchDst)
            {
                float distance1=PlaneRayDistance(_planeStart,lightRay);
                float distance2=PlaneRayDistance(_planeEnd,lightRay);
                float distanceInside=max(distance1,distance2);
                float distanceLimitParam=saturate(distanceInside/_LightMarchMinimalDistance);
                float cloudDensity=0;
                float totalDst=0;
                for(int i=0;i<_LightMarchTimes;i++)
                {
                    float3 marchPos=lightRay.GetPoint(totalDst);
                    cloudDensity+=SampleDensity(marchPos);
                    totalDst+=marchDst;
                    if(totalDst>=distanceInside)
                        break;
                }
                return cloudDensity/_LightMarchTimes*distanceLimitParam;
            }
            #endif

            float4 frag (v2f_img i) : SV_Target
            {
                float3 marchDirWS=normalize(GetViewDirWS(i.uv));
                float3 lightDirWS=normalize(_MainLightPosition.xyz);
                float3 cameraPos=GetCameraPositionWS();
                GPlane planeStartWS=GetPlane( float3(0,1,0),_VerticalStart);
                GPlane planeEndWS=GetPlane(float3(0,1,0),_VerticalEnd);
                GRay viewRayWS=GetRay( cameraPos,marchDirWS);
                float distance1=PlaneRayDistance(planeStartWS,viewRayWS);
                float distance2=PlaneRayDistance(planeEndWS,viewRayWS);
				float linearDepth = LinearEyeDepth(i.uv);
                distance1=min(linearDepth,distance1);
                distance2=min(linearDepth,distance2);
                float3 marchBegin=cameraPos;
                float marchDistance=-1;
                if(_VerticalStart< cameraPos.y && cameraPos.y<_VerticalEnd)
                {
                    marchDistance=max(distance1,distance2);
                }
                else if(distance1>0)
                {
                    float distanceOffset=distance1-distance2;
                    marchBegin=_WorldSpaceCameraPos+marchDirWS* (distanceOffset>0?distance2:distance1);
                    marchDistance=abs(distanceOffset);
                }

                float cloudDensity=1;
                float lightIntensity=1;
                if(marchDistance>0)
                {
                    float scatter=1;
                    #if _LIGHTSCATTER
                    scatter=(1-smoothstep(_ScatterRange,1,dot(marchDirWS,lightDirWS))*_ScatterStrength);
                    #endif
                    float cloudMarchDst= _Distance/_RayMarchTimes;
                    float cloudMarchParam=1.0/_RayMarchTimes;
                    float lightMarchParam=_LightAbsorption*_Opacity;
                    float lightMarchDst=_Distance/_LightMarchTimes/2;
                    float dstMarched=0;
                    float totalDensity=0;
                    for(int index=0;index<_RayMarchTimes;index++)
                    {
                        float3 marchPos=marchBegin+marchDirWS*dstMarched;
                        float density=SampleDensity(marchPos)*cloudMarchParam;
                        if(density>0)
                        {
                            cloudDensity*= exp(-density*_Opacity);
                            #if _LIGHTMARCH
                            GRay lightRayWS=GetRay( marchPos,lightDirWS);
                            lightIntensity *= exp(-density*scatter*cloudDensity*lightMarchParam*lightMarch(planeStartWS,planeEndWS,lightRayWS,lightMarchDst));
                            #else
                            lightIntensity -= density*scatter*cloudDensity*lightMarchParam;
                            #endif
                        }

                        dstMarched+=cloudMarchDst;
                        if(cloudDensity<0.01||dstMarched>marchDistance)
                            break;
                    }
                }

                float3 rampCol=tex2D(_ColorRamp,  lightIntensity).rgb;
                float3 lightCol= lerp(rampCol,_MainLightColor.rgb, lightIntensity);
                float3 baseCol = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;
                float3 finalCol=lerp(lightCol,baseCol, cloudDensity) ;
                return float4(finalCol,1) ;
            }
            ENDHLSL
        }
    }
}
