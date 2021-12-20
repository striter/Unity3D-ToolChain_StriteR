Shader "Game/Effects/Depth/Raymarch_VolumetricFog"
{
    Properties
    {
        [HDR]_Color("Color",Color)=(1,1,1,1)
        _Distance("March Distance",Range(0,500))=5
        _Density("Density",Range(0,5))=1
        _DensityClip("_Density Clip",Range(0,1))=.2
        [Enum(_16,16,_32,32,_64,64,_128,128)]_RayMarch("Ray March Times",int)=128
        _Noise("Noise 3D",3D)="white"{}
        [Vector3]_NoiseScale("Noise Scale",Vector)=(50,50,50,1)
        [Vector3]_NoiseFlow("Noise Flow",Vector)=(0,0,0,1)
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "DisableBatching"="True"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #define IGeometryDetection
            #include "Assets/Shaders/Library/Geometry.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewDirWS:TEXCOORD2;
                float3 centerWS:TEXCOORD4;
                float3 sizeWS:TEXCOORD5;
            };

            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            TEXTURE3D(_Noise);SAMPLER(sampler_Noise);
            CBUFFER_START(UnityPerMaterial)
            int _RayMarch;
            float4 _ScanColor;
            float _Distance;
            float _Density;
            float _DensityClip;

            float4 _NoiseScale;
            float4 _NoiseFlow;
            CBUFFER_END

            float SampleDensity(float3 worldPos)  {
                float density=saturate(SAMPLE_TEXTURE3D_LOD(_Noise,sampler_Noise, worldPos/_NoiseScale.xyz+_NoiseFlow.xyz*_Time.y,0).r);
                return smoothstep(_DensityClip,1,density);
            }

            v2f vert (a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS=TransformObjectToWorld(v.positionOS);
                o.viewDirWS=o.positionWS-GetCameraPositionWS();
                o.centerWS=TransformObjectToWorld(0);
                o.sizeWS=TransformObjectToWorldDir(1,false);
                o.screenPos=ComputeScreenPos(o.positionCS);
                return o;
            }
            

            float4 frag (v2f i) : SV_Target
            {
                float3 marchDirWS=normalize( i.viewDirWS);
                float marchDstWS=AABBRayDistance(GBox_Ctor( i.centerWS,i.sizeWS),GRay_Ctor(i.positionWS,marchDirWS)).y;
                float depthDstWS=RawToEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w).r);
                float marchDistance= max(0,min(marchDstWS, depthDstWS));
                
                float sumDensity=0;
                if(marchDistance>0)
                {
                    uint rayMarchCount=min(_RayMarch,128u);
                    float marchOffset=1.0/rayMarchCount;
                    float distanceOffset=_Distance/rayMarchCount;
                    float dstMarched=0.;
                    float shadowStrength=GetMainLightShadowParams().x;
                    [unroll(128u)]
                    for(uint index=0u;index<rayMarchCount;index++)
                    {
                        float3 marchPos=i.positionWS+marchDirWS*dstMarched;
                        float density=SampleDensity(marchPos)*_Density;
                        sumDensity+=marchOffset*density;
                        sumDensity*=SampleHardShadow(_MainLightShadowmapTexture,sampler_MainLightShadowmapTexture,marchPos,shadowStrength);
                        dstMarched+=distanceOffset;

                        if(sumDensity>=1||dstMarched>marchDistance)
                            break;
                    }
                }
                sumDensity=saturate(sumDensity);

                return _ScanColor*sumDensity;
            }
            ENDHLSL
        }
    }
}
