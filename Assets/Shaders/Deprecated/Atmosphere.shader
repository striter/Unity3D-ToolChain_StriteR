Shader "Hidden/PostProcess/Atmosphere"
{
    Properties
    {
        [PreRenderData]_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        HLSLINCLUDE
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #include "Assets/Shaders/Library/PBR/Atmosphere.hlsl"
        ENDHLSL
        
        Pass
        {
            Name "Multi Scatter-Precompute Particle Density"
            HLSLPROGRAM
            #pragma vertex vert_blit
            #pragma fragment frag

            float2 frag(v2f_img i):SV_TARGET
            {
                float cosAngle = i.uv.x * 2 -1;
                float sinAngle = sqrt(saturate(1-cosAngle*cosAngle));
                float startHeight = lerp(0,_AtmosphereHeight,i.uv.y);
                GRay ray = GRay_Ctor(float3(0,startHeight,0),float3(sinAngle,cosAngle,0));
                
                int _ParticleStepCount = 250;
                float2 intersection = Distance(GSphere_Ctor(_PlanetCenter,_PlanetRadius),ray);
                if (intersection.x > 0)
                    return 1e+20;
                GSphere atmosphere = GSphere_Ctor(_PlanetCenter,_PlanetRadius + _AtmosphereHeight);
                intersection = Distance(atmosphere,ray);

                float3 rayEnd = ray.GetPoint(intersection.y);
                float3 step = (rayEnd - ray.origin)/_ParticleStepCount;
                float stepSize = length(step);
                float2 density = 0;
                for(float s = 0.5 ; s < _ParticleStepCount; s++)
                {
                    float3 position = ray.GetPoint(s);
                    float height = abs(length(position-atmosphere.center))-_PlanetRadius;
                    float2 localDensity = exp(-height.xx/_DensityScaleHeight);
                    density += localDensity * stepSize;
                }
                
                return density;
            }
            
            ENDHLSL
        }
        
        Pass
        {
            Name "Multi Scatter-Integrate"
            HLSLPROGRAM
            #pragma vertex vert_blit
            #pragma fragment frag
            
            float _DistanceScale;
            float _SunIntensity;
                        
            float _MieG;
            float3 _ScatterR;
            float3 _ScatterM;
            
            float3 _ExtinctionR;
            float3 _ExtinctionM;
            
            TEXTURE2D(_AtmosphereDensityLUT);SAMPLER(sampler_AtmosphereDensityLUT);
            void GetAtmosphereDensity(float3 _position,float3 _lightDir,out float2 _localDensity,out float2 _densityToAtmTop)
            {
                float3 offset = _position - _PlanetCenter;
                float height = length(offset) - _PlanetRadius;
                _localDensity = exp(-height.xx/_DensityScaleHeight);

                float cosAngle = dot(normalize(offset),_lightDir);
                float2 sample = float2(cosAngle * .5 + .5,height/_AtmosphereHeight);
                _densityToAtmTop = SAMPLE_TEXTURE2D(_AtmosphereDensityLUT,sampler_AtmosphereDensityLUT,sample).rg;
            }

            void ComputeLocalInScattering(float2 _localDensity,float2 _densityPA,float2 _densityCP,out float3 _localScatterR,out float3 _localScatterM)
            {
                float2 densityCPA = _densityCP + _densityPA;
                float3 Tr = densityCPA.x * _ExtinctionR;
                float3 Tm = densityCPA.y * _ExtinctionM;
                float3 extinction = exp(-(Tr + Tm));
                _localScatterR = _localDensity.x * extinction;
                _localScatterM = _localDensity.y * extinction;
            }

            float4 IntegrateInScattering(float _sampleCount,GRay _scatterRay,float _scatterDistance,float _distanceScale,float3 _lightDir,out float4 _extinction)
            {
                _extinction = 0;
                float stepSize = _scatterDistance / _sampleCount;
                float3 step = _scatterRay.direction * stepSize;
                stepSize *= _distanceScale;

                float2 densityCP = 0;
                float3 scatterR = 0;
                float3 scatterM = 0;
                
                float2 densityPA = 0;
                float2 prevLocalDensity;
                float3 prevLocalInScatterR,prevLocalInScatterM;
                GetAtmosphereDensity(_scatterRay.origin,_lightDir,prevLocalDensity,densityPA);
                ComputeLocalInScattering(prevLocalDensity,densityPA,densityCP,prevLocalInScatterR,prevLocalInScatterM);
                for(float s = 1; s<_sampleCount;s++)
                {
                    float3 p = _scatterRay.origin + step * s;

                    float2 localDensity;
                    GetAtmosphereDensity(p,_lightDir,localDensity,densityPA);
                    densityCP += (localDensity + prevLocalDensity) * stepSize*.5;

                    float3 localInScatterR,localInScatterM;
                    ComputeLocalInScattering(localDensity,densityPA,densityCP,localInScatterR,localInScatterM);
                    scatterR += (localInScatterR + prevLocalInScatterR) * stepSize *.5;
                    scatterM += (localInScatterM + prevLocalInScatterM) * stepSize *.5;
                    prevLocalInScatterR = localInScatterR;
                    prevLocalInScatterM = localInScatterM;
                }

                _extinction = float4( exp(-(densityCP.x * _ExtinctionR + densityCP.y * _ExtinctionM)),0);
                float cosAngle = dot(_scatterRay.direction,_lightDir);
                float3 sun = RenderSun(scatterM,cosAngle) * _SunIntensity;
                scatterR *= RayleighScattering(cosAngle);
                scatterM *= MieScattering(cosAngle,_MieG);
                float3 lightInScatter = (scatterR*_ScatterR + scatterM * _ScatterM);
                lightInScatter += sun;
                return float4(lightInScatter  * _MainLightColor.rgb,1);
            }
            
            float4 frag(v2f_img i):SV_TARGET
            {
                float4 baseCol = SampleMainTex(i.uv);
                float3 scatterDirection = normalize(TransformNDCToFrustumCornersRay(i.uv));
                float2 screenUV = i.uv;
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV);
                float depthDistance = RawToDistance(rawDepth,screenUV);
                float depth01 = RawTo01Depth(rawDepth);
                GSphere atmoSphere = GSphere_Ctor(_PlanetCenter,_PlanetRadius + _AtmosphereHeight);
                GRay scatterRay = GRay_Ctor(_WorldSpaceCameraPos,scatterDirection);
                float2 travelDistance = Distance(atmoSphere,scatterRay);
                float3 lightDir = normalize(_MainLightPosition.xyz);

                float marchbegin = travelDistance.x;
                float marchDistance = min(depthDistance,travelDistance.y) - marchbegin;
                if(marchDistance<FLT_EPS)
                    return baseCol;
                if(depth01 > 0.999999)
                    marchDistance = min(1e5,marchDistance);

                float4 extinction = 0;
                float4 inScattering = IntegrateInScattering(16,scatterRay,marchDistance,_DistanceScale,lightDir, extinction);
                return baseCol * extinction + inScattering;
            }
            ENDHLSL
        }
    }
}
