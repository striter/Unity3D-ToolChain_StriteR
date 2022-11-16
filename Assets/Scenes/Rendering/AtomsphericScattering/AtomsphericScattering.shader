Shader "Game/Skybox/AtomsphericScattering"
{
    Properties
    {
        [MinMaxRange]_Radius("_Radius",Range(0,500))=50
        [HideInInspector]_RadiusEnd("",float) = 60
        _DensityFallOff("Fall Off",Range(-30,30))=12
        [WaveLength(_ScatteringCoefficients)]_WaveLength("Wave Length",Vector)=(700,530,440,1)
        [HideInInspector]_ScatteringCoefficients("Scattering Coefficients",Vector)=(1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend One One
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            struct a2v
            {
                float3 positionOS : POSITION;
                float3 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float4 positionHCS :TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                o.positionHCS = o.positionCS;
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            float _Radius;
            float _RadiusEnd;
            float _DensityFallOff;
            float3 _ScatteringCoefficients;
            #define _ScatterTimes 10
            #define _OpticalDepthTimes 8
            
            float CalculateDensityAtPoint(float3 _position,GSphere _atmoSphere)
            {
                float density01 = saturate(invlerp(_Radius,_RadiusEnd,length(_atmoSphere.center-_position)));
                return exp(-density01*_DensityFallOff);;
            }
            
            float CalculateOpticalDepth(GRay _ray,GSphere _atomSphere,float _scatterLength)
            {
                float stepSize = _scatterLength / (_OpticalDepthTimes-1);
                float3 opticalDepthPoint = _ray.origin + _ray.direction*.5*stepSize;
                float opticalDepth = 0;
                for(int i=0;i<_OpticalDepthTimes;i++)
                {
                    float density = CalculateDensityAtPoint(opticalDepthPoint,_atomSphere);
                    opticalDepth += density * stepSize;
                    opticalDepthPoint += _ray.direction * stepSize;
                }
                return opticalDepth;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float3 scatterDirection = GetCameraRealDirectionWS(i.positionWS);// i.viewDirWS);
                
                float2 screenUV = TransformHClipToNDC(i.positionHCS);
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV);
                float depthDistance = RawToDistance(rawDepth,screenUV);
                GSphere atmoSphere = GSphere_Ctor(float3(0,0,0),_RadiusEnd);
                float2 travelDistance = SphereRayDistance(atmoSphere,GRay_Ctor(_WorldSpaceCameraPos,scatterDirection));

                float marchbegin = travelDistance.x;
                float marchDistance = min(depthDistance,travelDistance.y) - marchbegin;
                if(marchDistance<FLT_EPS)
                    return 0;

                float scatterStepSize = marchDistance / (_ScatterTimes-1);
                float3 inScatterPoint = _WorldSpaceCameraPos + scatterDirection * (marchbegin + scatterStepSize*.5); 
                float3 inScatterLight = 0;
                for(int t=0;t<_ScatterTimes;t++)
                {
                    GRay scatterRay = GRay_Ctor(inScatterPoint,normalize(inScatterPoint+_MainLightPosition.xyz*1000));
                    float inScatterRayLength = SphereRayDistance(atmoSphere,scatterRay).y;
                    float sunRayOpticalDepth = CalculateOpticalDepth(scatterRay,atmoSphere,inScatterRayLength);
                    float viewRayOpticalDepth = CalculateOpticalDepth(GRay_Ctor(inScatterPoint,-scatterDirection),atmoSphere,scatterStepSize*(t+0.5f));
                    float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth)*_ScatteringCoefficients);
                    float scatterDensity = CalculateDensityAtPoint(inScatterPoint,atmoSphere);
                    inScatterLight += scatterDensity * transmittance *_ScatteringCoefficients* scatterStepSize;
                    inScatterPoint += scatterDirection * scatterStepSize;
                }
                return float4(inScatterLight,1);
                
            }
            ENDHLSL
        }
    }
}
