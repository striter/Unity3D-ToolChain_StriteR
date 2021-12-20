Shader "Game/Effects/Depth/Raymarch_VolumetricCloud"
{
    Properties
    {
        _Strength("Strength",Range(0,5))=1
        _DensityClip("Cloud Clip",Range(0,1))=.1
        _Density("Cloud Density",Range(0,10))=1
        _Distance("March Distance",float)=50
        [Enum(_16,16,_32,32,_64,64,_128,128)]_RayMarchTimes("Ray March Times",int)=32
        [NoScaleOffset]_ColorRamp("Cloud Color Ramp",2D)="white"
        
        [Header(Noise Settings)]
        _Noise("Noise 3D",3D)="white"{}
        _NoiseScale("Noise Scale",Vector)=(10,10,1,1)
        _NoiseFlow("Noise Flow",Vector)=(0,0,0,1)

        [Header(Light March Settings)]
        _LightAbsorption("Light Absorption",Range(0,10))=1
        [Toggle(_LIGHTMARCH)]_EnableLightMarch("Enable Light March",int)=1
        [Enum(_4,4,_8,8,_16,16)]_LightMarchTimes("Light March Times",int)=8
        
        [Header(Scatter Settings)]
        _ScatterRange("Scatter Range",Range(0.5,1))=0.8
        _ScatterStrength("Scatter Strength",Range(0,1))=0.8
    }
    SubShader
    {
        Tags{"Queue"="Transparent" }
        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #define IGeometryDetection
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #pragma shader_feature_local _LIGHTMARCH

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD0;
                float3 centerWS:TEXCOORD1;
                float3 sizeWS:TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };
            
            TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            TEXTURE3D (_Noise);SAMPLER(sampler_Noise);
            CBUFFER_START(UnityPerMaterial)
            uint _RayMarchTimes;
            float _Distance;
            float _Strength;
            float _Density;
            float _DensityClip;
            
            float _ScatterRange;
            float _ScatterStrength;

            int _LightMarchTimes;
            float _LightAbsorption;
            sampler2D _ColorRamp;

            float3 _NoiseScale;
            float3 _NoiseFlow;
            CBUFFER_END
            float SampleDensity(float3 worldPos)  {
                return saturate(smoothstep(_DensityClip,1 , SAMPLE_TEXTURE3D_LOD(_Noise,sampler_Noise, worldPos/_NoiseScale+_NoiseFlow*_Time.y,0).r)*_Density);
            }

            #if _LIGHTMARCH
            float lightMarch(GBox box,GRay ray,float marchDst)
            {
                float dstInsideBox=AABBRayDistance(box,ray).y;
                float cloudDensity=0;
                float totalDst=0;
                for(int i=0;i<_LightMarchTimes;i++)
                {
                    float3 marchPos=ray.GetPoint(totalDst);
                    cloudDensity+=SampleDensity(marchPos);
                    totalDst+=marchDst;
                    if(totalDst>dstInsideBox)
                        break;
                }
                return  cloudDensity/_LightMarchTimes;
            }
            #endif
            
            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS=TransformObjectToWorld(v.positionOS);
                o.centerWS=TransformObjectToWorld(0);
                o.sizeWS=TransformObjectToWorldDir(1,false);
                o.screenPos=ComputeScreenPos(o.positionCS);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float3 marchDirWS=GetCameraRealDirectionWS( i.positionWS);
                GBox boxWS=GBox_Ctor(i.centerWS,i.sizeWS);
                GRay rayWS=GRay_Ctor(i.positionWS,marchDirWS);
                float marchDstWS=AABBRayDistance(boxWS,rayWS).y;
                float depthDstWS=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r-i.screenPos.w;
                float marchDistance= min(depthDstWS, marchDstWS);

                float cloudDensity=1;
                float lightIntensity=1;
                if(marchDistance>0)
                {
                    float3 lightDirWS=normalize(_MainLightPosition.xyz);
                    float scatter=(1-smoothstep(_ScatterRange,1,dot(marchDirWS,lightDirWS))*_ScatterStrength);
                    float cloudMarchDst= _Distance/_RayMarchTimes;
                    float lightMarchDst=_Distance/_LightMarchTimes;
                    float dstMarched=0;
                    float marchParam=1.0/_RayMarchTimes;
                    float totalDensity=0;
                    for(uint index=0u;index<_RayMarchTimes;index++)
                    {
                        float3 marchPos=i.positionWS+marchDirWS*dstMarched;
                        float density=SampleDensity(marchPos);
                        density*=marchParam;
                        if(density>0)
                        {
                            cloudDensity*= exp(-density*_Strength);
                            #if _LIGHTMARCH
                            GRay lightRayWS=GRay_Ctor( marchPos,lightDirWS);
                            lightIntensity *= exp(-density*scatter*cloudDensity*lerp(0,_LightAbsorption, lightMarch(boxWS,lightRayWS,lightMarchDst)));
                            #else
                            lightIntensity -= density*scatter*cloudDensity*_LightAbsorption;
                            #endif
                        }

                        dstMarched+=cloudMarchDst;
                        if(cloudDensity<0.01||dstMarched>marchDistance)
                            break;
                    }
                }
                float3 rampCol=tex2D(_ColorRamp, lightIntensity).rgb;
                float3 lightCol= lerp(rampCol,_MainLightColor.rgb, lightIntensity);
                return float4(lightCol,1-cloudDensity);
            }
            ENDHLSL
        }
    }
}
