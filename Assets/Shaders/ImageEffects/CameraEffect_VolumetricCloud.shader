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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "CameraEffectInclude.cginc"
            #include "../BoundingCollision.cginc"
            #pragma shader_feature _LIGHTMARCH
            #pragma shader_feature _LIGHTSCATTER

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

            sampler3D _Noise;
            float4 _NoiseScale;
            float4 _NoiseFlow;
            float SampleDensity(float3 worldPos)  {
                float smoothParam=(worldPos.y);
                smoothParam=saturate(min(abs(worldPos.y-_VerticalStart)/_DensitySmooth,abs(worldPos.y-_VerticalEnd)/_DensitySmooth));
                return  smoothstep(_DensityClip,1 , tex3Dlod(_Noise,float4( worldPos/_NoiseScale+_NoiseFlow*_Time.y,0)).r)*_Density*smoothParam;
            }

            #if _LIGHTMARCH
            float lightMarch(float3 position,float3 marchDir,float marchDst)
            {
                float distance1=PRayDistance(float3(0,_VerticalStart,0),float3(0,0,1),float3(0,1,0),position,marchDir);
                float distance2=PRayDistance(float3(0,_VerticalEnd,0),float3(0,0,1),float3(0,1,0),position,marchDir);
                float distanceInside=max(distance1,distance2);
                float distanceLimitParam=saturate(distanceInside/_LightMarchMinimalDistance);
                float cloudDensity=0;
                float totalDst=0;
                for(int i=0;i<_LightMarchTimes;i++)
                {
                    float3 marchPos=position+marchDir*totalDst;
                    cloudDensity+=SampleDensity(marchPos);
                    totalDst+=marchDst;
                    if(totalDst>=distanceInside)
                        break;
                }
                return cloudDensity/_LightMarchTimes*distanceLimitParam;
            }
            #endif

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 viewDir:TEXCOORD1;
                float3 lightDir:TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.viewDir=GetInterpolatedRay(v.vertex);
                o.lightDir=_WorldSpaceLightPos0;
                return o;
            }


            float4 frag (v2f _input) : SV_Target
            {
                float3 marchDir=normalize(_input.viewDir);
                float3 cameraPos=_WorldSpaceCameraPos;
                float distance1=PRayDistance(float3(0,_VerticalStart,0),float3(0,0,1),float3(0,1,0),cameraPos,marchDir);
                float distance2=PRayDistance(float3(0,_VerticalEnd,0),float3(0,0,1),float3(0,1,0),cameraPos,marchDir);
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,_input.uv));
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
                    marchBegin=_WorldSpaceCameraPos+marchDir* (distanceOffset>0?distance2:distance1);
                    marchDistance=abs(distanceOffset);
                }

                float cloudDensity=1;
                float lightIntensity=1;
                if(marchDistance>0)
                {
                    float3 lightDir=normalize(_input.lightDir);
                    float scatter=1;
                    #if _LIGHTSCATTER
                    scatter=(1-smoothstep(_ScatterRange,1,dot(marchDir,lightDir))*_ScatterStrength);
                    #endif
                    float cloudMarchDst= _Distance/_RayMarchTimes;
                    float cloudMarchParam=1.0/_RayMarchTimes;
                    float lightMarchParam=_LightAbsorption*_Opacity;
                    float lightMarchDst=_Distance/_LightMarchTimes/2;
                    float dstMarched=0;
                    float totalDensity=0;
                    for(int i=0;i<_RayMarchTimes;i++)
                    {
                        float3 marchPos=marchBegin+marchDir*dstMarched;
                        float density=SampleDensity(marchPos)*cloudMarchParam;
                        if(density>0)
                        {
                            cloudDensity*= exp(-density*_Opacity);
                            #if _LIGHTMARCH
                            lightIntensity *= exp(-density*scatter*cloudDensity*lightMarchParam*lightMarch(marchPos,lightDir,lightMarchDst));
                            #else
                            lightIntensity -= density*scatter*cloudDensity*lightMarchParam;
                            #endif
                        }

                        dstMarched+=cloudMarchDst;
                        if(cloudDensity<0.01||dstMarched>marchDistance)
                            break;
                    }
                }
                float3 rampCol=tex2D(_ColorRamp, lightIntensity).rgb;
                float3 lightCol= lerp(rampCol,_LightColor0.rgb, lightIntensity);
                float4 cloudCol=float4(lightCol,1-cloudDensity);
                
                float4 baseCol = tex2D(_MainTex, _input.uv);
                return float4(cloudCol.rgb*cloudCol.a+baseCol.rgb*(1-cloudCol.a) ,1) ;
            }
            ENDCG
        }
    }
}
